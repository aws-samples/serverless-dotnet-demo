using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Shared;
using Shared.DataAccess;
using Shared.Models;

public class Function
{
    private static ProductsDAO dataAccess;

    static Function()
    {
        AWSSDKHandler.RegisterXRayForAllServices();
        dataAccess = new DynamoDbProducts();
    }

    /// <summary>
    /// The main entry point for the custom runtime.
    /// </summary>
    /// <param name="args"></param>
    private static async Task Main()
    {
        Func<APIGatewayHttpApiV2ProxyRequest, ILambdaContext, Task<APIGatewayHttpApiV2ProxyResponse>> handler = FunctionHandler;
        await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<HttpApiJsonSerializerContext>(options => {
                options.PropertyNameCaseInsensitive = true;
            }))
            .Build()
            .RunAsync();
    }

    public static async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest apigProxyEvent, ILambdaContext context)
    {
        if (!apigProxyEvent.RequestContext.Http.Method.Equals(HttpMethod.Put.Method))
        {
            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = "Only PUT allowed",
                StatusCode = (int)HttpStatusCode.MethodNotAllowed,
            };
        }

        try
        {
            context.Logger.LogLine(JsonSerializer.Serialize(apigProxyEvent, HttpApiJsonSerializerContext.Default.APIGatewayHttpApiV2ProxyRequest));
            
            var id = apigProxyEvent.PathParameters["id"];

            var product = JsonSerializer.Deserialize(apigProxyEvent.Body, HttpApiJsonSerializerContext.Default.Product);

            if (product == null || id != product.Id)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = "Product ID in the body does not match path parameter",
                    StatusCode = (int)HttpStatusCode.BadRequest,
                };
            }

            await dataAccess.PutProduct(product);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.Created,
                Body = $"Created product with id {id}"
            };
        }
        catch (Exception e)
        {
            context.Logger.LogError($"Error creating product {e.Message} {e.StackTrace}");

            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = "Not Found",
                StatusCode = (int)HttpStatusCode.InternalServerError,
            };
        }
    }
}