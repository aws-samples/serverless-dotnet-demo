using System;
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

        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = JsonSerializer.Serialize(product),
            Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
        };
    }
    catch (Exception e)
    {
        context.Logger.LogError($"Error getting product {e.Message} {e.StackTrace}");
        
        return new APIGatewayHttpApiV2ProxyResponse
        {
            Body = "Not Found",
            StatusCode = (int)HttpStatusCode.InternalServerError,
        };
    }
};

await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
    .Build()
    .RunAsync();

[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
[JsonSerializable(typeof(Dictionary<string, string>))]
public partial class ApiGatewayProxyJsonSerializerContext : JsonSerializerContext
{
}
