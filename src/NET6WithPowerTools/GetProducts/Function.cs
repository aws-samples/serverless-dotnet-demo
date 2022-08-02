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

namespace GetProducts
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
    
            Logger.LogInformation($"Received {apigProxyEvent}");

            var products = await dataAccess.GetAllProducts();
    
            Logger.LogInformation($"Found {products.Products.Count} product(s)");
    
            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = JsonSerializer.Serialize(products),
                StatusCode = 200,
                Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
            };
        }
    }
}
