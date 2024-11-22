using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Shared;
using Shared.DataAccess;
using Shared.Models;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DeleteProduct
{
    public class Function
    {
        private readonly ProductsDAO dataAccess;
        public Function()
        {
            this.dataAccess = new DynamoDbProducts();
            Amazon.Lambda.Core.SnapshotRestore.RegisterBeforeSnapshot(StuffToRunBeforeSnapshotIsTaken);
            Amazon.Lambda.Core.SnapshotRestore.RegisterAfterRestore(StuffToRunAfterRestore);
        }

        private ValueTask StuffToRunAfterRestore()
        {           
            /*
            We don't use this method in this code, but keeping it here for others to use as a sample.
            Some reason that you may need to run code after restore:
            * Reestablish connections
            * Re-seed uniqueness such as recreating a pseudo-random number generator or creating a new GUID
            * Refresh configuration, such as config that is downloaded from the network
            */
            return ValueTask.CompletedTask;
        }

        private async ValueTask StuffToRunBeforeSnapshotIsTaken()
        {           
            /*
            We use this method to warm up the JIT compiler to improve post-restore performance, but it can also 
            be used to flush logs, or close connections, or general cleanup before the snapshot is taken.
            */
            
            // Run this 100 times to make sure JIT is fully warmed up, make sure to stay under the 10 second limit
            for(int i = 1; i <= 100; i++)
            {
                Console.WriteLine($"Running warmup number {i}");
                string itemId = "12345ForWarming";
                var badRequest = new APIGatewayHttpApiV2ProxyRequest
                {
                    RequestContext = new APIGatewayHttpApiV2ProxyRequest.ProxyRequestContext
                    {
                        Http = new APIGatewayHttpApiV2ProxyRequest.HttpDescription
                        {
                            Method = HttpMethod.Get.Method // Wrong method
                        }
                    }
                };
                var goodRequest = new APIGatewayHttpApiV2ProxyRequest
                {
                    PathParameters = new Dictionary<string, string>{{"id",itemId}},
                    RequestContext = new APIGatewayHttpApiV2ProxyRequest.ProxyRequestContext
                    {
                        Http = new APIGatewayHttpApiV2ProxyRequest.HttpDescription
                        {
                            Method = HttpMethod.Delete.Method
                        }
                    }
                };

                await CallHandlerAsync(badRequest);
                // Create a dummy item in the db to delete
                await dataAccess.PutProduct(new Product(itemId, "forWarming", default));
                // Hit the path that deletes the item
                await CallHandlerAsync(goodRequest);
                // Hit the path where the item is already deleted
                await CallHandlerAsync(goodRequest);
            }
        }

        private async Task CallHandlerAsync(APIGatewayHttpApiV2ProxyRequest request)
        {
            var response = await FunctionHandler(request, new FakeLambdaContext());
            if (response.StatusCode == (int)HttpStatusCode.InternalServerError)
            {
                throw new Exception($"We shouldn't be catching an exception in warmup. Request was: {request}");
            }
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest apigProxyEvent,
            ILambdaContext context)
        {
            if (!apigProxyEvent.RequestContext.Http.Method.Equals(HttpMethod.Delete.Method))
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = "Only DELETE allowed",
                    StatusCode = (int)HttpStatusCode.MethodNotAllowed,
                };
            }

            try
            {
                var id = apigProxyEvent.PathParameters["id"];

                var product = await dataAccess.GetProduct(id);

                if (product == null)
                {
                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        Body = "Not Found",
                        StatusCode = (int)HttpStatusCode.NotFound,
                    };
                }

                await dataAccess.DeleteProduct(product.Id);
        
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = $"Product with id {id} deleted"
                };
            }
            catch (Exception e)
            {
                context.Logger.LogLine($"Error deleting product {e.Message} {e.StackTrace}");
        
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = "Not Found",
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                };
            }
        }
    }
}
