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
using Shared.DataAccess;

namespace GetProducts;

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
        await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<CustomJsonSerializerContext>())
            .Build()
            .RunAsync();
    }

    public static async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest apigProxyEvent, ILambdaContext context)
    {
        // if (!apigProxyEvent.RequestContext.Http.Method.Equals(HttpMethod.Get.Method))
        // {
        //     return new APIGatewayHttpApiV2ProxyResponse
        //     {
        //         Body = "Only GET allowed",
        //         StatusCode = (int)HttpStatusCode.MethodNotAllowed,
        //     };
        // }

        context.Logger.LogInformation(JsonSerializer.Serialize(apigProxyEvent));
        
        context.Logger.LogInformation($"Received {apigProxyEvent}");

        var products = await dataAccess.GetAllProducts();
        
        context.Logger.LogInformation($"Found {products.Products.Count} product(s)");
        
        return new APIGatewayHttpApiV2ProxyResponse
        {
            Body = JsonSerializer.Serialize(products, typeof(Shared.Models.ProductWrapper), new CustomJsonSerializerContext()),
            StatusCode = 200,
            Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
        };
    }
}

[JsonSerializable(typeof(Shared.Models.Product))]
[JsonSerializable(typeof(Shared.Models.ProductWrapper))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
public partial class CustomJsonSerializerContext : JsonSerializerContext
{
}
