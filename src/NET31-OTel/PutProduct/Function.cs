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
using Shared.Models;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PutProduct
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
            if (!apigProxyEvent.HttpMethod.Equals(HttpMethod.Put.Method))
            {
                return new APIGatewayProxyResponse
                {
                    Body = "Only PUT allowed",
                    StatusCode = (int)HttpStatusCode.MethodNotAllowed,
                };
            }
    
            try
            {
                var id = apigProxyEvent.PathParameters["id"];

                var product = JsonSerializer.Deserialize<Product>(apigProxyEvent.Body);

                if (product == null || id != product.Id)
                {
                    return new APIGatewayProxyResponse
                    {
                        Body = "Product ID in the body does not match path parameter",
                        StatusCode = (int)HttpStatusCode.BadRequest,
                    };
                }

                await dataAccess.PutProduct(product);
        
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    Body = $"Created product with id {id}"
                };
            }
            catch (Exception e)
            {
                context.Logger.LogLine($"Error creating product {e.Message} {e.StackTrace}");
        
                return new APIGatewayProxyResponse
                {
                    Body = "Not Found",
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                };
            }
        }
    }
}
