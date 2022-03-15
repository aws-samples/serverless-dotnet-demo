using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Shared.DataAccess;

[assembly: LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<ApiGatewayProxyJsonSerializerContext>))]

AWSSDKHandler.RegisterXRayForAllServices();
ProductsDAO dataAccess = new DynamoDbProducts();

var handler = async (APIGatewayHttpApiV2ProxyRequest apigProxyEvent, ILambdaContext context) =>
{
    if (!apigProxyEvent.RequestContext.Http.Method.Equals(HttpMethod.Get.Method))
    {
        return new APIGatewayHttpApiV2ProxyResponse
        {
            Body = "Only GET allowed",
            StatusCode = (int)HttpStatusCode.MethodNotAllowed,
        };
    }
    
    context.Logger.LogInformation($"Received {apigProxyEvent}");

    var products = await dataAccess.GetAllProducts();
    
    context.Logger.LogInformation($"Found {products.Products.Count} product(s)");
    
    return new APIGatewayHttpApiV2ProxyResponse
    {
        Body = JsonSerializer.Serialize(products),
        StatusCode = 200,
        Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
    };
};

await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
    .Build()
    .RunAsync();

[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
[JsonSerializable(typeof(List<string>))]
public partial class ApiGatewayProxyJsonSerializerContext : JsonSerializerContext
{
}
