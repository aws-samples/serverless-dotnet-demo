using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using Serilog;
using Shared.DataAccess;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GetProducts
{
    public class Function
    {
        private readonly ProductsDAO dataAccess;
        
        public Function()
        {
            AWSSDKHandler.RegisterXRayForAllServices();
            
            this.dataAccess = new DynamoDbProducts();
        }

        [Logging(LogEvent = true)]
        [Metrics(CaptureColdStart = true)]
        [Tracing]
        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(
            APIGatewayHttpApiV2ProxyRequest apigProxyEvent,
            ILambdaContext context)
        {
            using var span = Activity.Current?.Source.StartActivity("UIGnwiugnw");
            
            Logger.LogInformation($"Received {apigProxyEvent}");

            Tracing.WithSubsegment("A really slow method call", async (subsegment) =>
            {
                subsegment.AddAnnotation("Test", "Test");
                
                await Task.Delay(1000);
            });

            var products = await dataAccess.GetAllProducts();

            Logger.LogInformation($"Found {products.Products.Count} product(s)");

            Metrics.AddMetric("ProductCount", 1, MetricUnit.Count);
    
            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = JsonSerializer.Serialize(products),
                StatusCode = 200,
                Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
            };
        }
    }
}
