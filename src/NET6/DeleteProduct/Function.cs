using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Shared.DataAccess;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DeleteProduct
{
    public class Function
    {
        private readonly ProductsDAO dataAccess;
        public Function()
        {
            this.dataAccess = new DynamoDbProducts();
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest apigProxyEvent,
            ILambdaContext context)
        {
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
