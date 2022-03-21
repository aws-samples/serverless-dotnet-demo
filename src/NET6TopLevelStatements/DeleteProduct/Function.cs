using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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
        context.Logger.LogError($"Error deleting product {e.Message} {e.StackTrace}");
        
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
public partial class ApiGatewayProxyJsonSerializerContext : JsonSerializerContext
{
}
