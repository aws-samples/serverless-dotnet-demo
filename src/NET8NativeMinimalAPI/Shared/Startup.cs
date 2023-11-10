using System.Text.Json;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.DataAccess;

namespace Shared
{
    public static class Startup
    {
        public static WebApplication Build(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.AddContext<ApiSerializerContext>();
            });

            builder.Services.AddSingleton<ProductsDAO, DynamoDbProducts>();

            builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi, options =>
            {
                options.Serializer = new SourceGeneratorLambdaJsonSerializer<ApiSerializerContext>();
            });
            
            builder.Logging.ClearProviders();
            builder.Logging.AddJsonConsole(options =>
            {
                options.IncludeScopes = true;
                options.UseUtcTimestamp = true;
                options.TimestampFormat = "hh:mm:ss ";
            });

            return builder.Build();
        }
    }
}