using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using OpenTelemetry.Contrib.Instrumentation.AWSLambda.Implementation;
using Shared;
using Shared.DataAccess;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DeleteProduct
{
    public class Function
    {
        private readonly ProductsDAO dataAccess;
        public Function()
        {
            Tracing.Init();
            
            this.dataAccess = new DynamoDbProducts();
        }
        
        public Task<APIGatewayProxyResponse> TracingFunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return AWSLambdaWrapper.Trace(Tracing.TraceProvider, FunctionHandler, request, context);
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent,
            ILambdaContext context)
        {
            if (!apigProxyEvent.HttpMethod.Equals(HttpMethod.Delete.Method))
            {
                return new APIGatewayProxyResponse
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
                    return new APIGatewayProxyResponse
                    {
                        Body = "Not Found",
                        StatusCode = (int)HttpStatusCode.NotFound,
                    };
                }

                await dataAccess.DeleteProduct(product.Id);
        
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = $"Product with id {id} deleted"
                };
            }
            catch (Exception e)
            {
                context.Logger.LogLine($"Error deleting product {e.Message} {e.StackTrace}");
        
                return new APIGatewayProxyResponse
                {
                    Body = "Not Found",
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                };
            }
        }
    }
}
