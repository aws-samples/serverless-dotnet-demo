using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using Shared.DataAccess;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GetProduct
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
            if (!apigProxyEvent.RequestContext.Http.Method.Equals(HttpMethod.Get.Method))
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = "Only GET allowed",
                    StatusCode = (int)HttpStatusCode.MethodNotAllowed,
                };
            }

            try
            {
                Logger.LogInformation("Received get product request");

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

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = JsonSerializer.Serialize(product),
                    Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
                };
            }
            catch (Exception e)
            {
                context.Logger.LogLine($"Error getting product {e.Message} {e.StackTrace}");
        
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = "Not Found",
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                };
            }
        }
    }
}
