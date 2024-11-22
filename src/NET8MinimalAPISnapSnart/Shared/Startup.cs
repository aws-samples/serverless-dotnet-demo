using Amazon.DynamoDBv2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.DataAccess;

namespace Shared
{
    public static class Startup
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(new AmazonDynamoDBClient());
            services.AddSingleton<ProductsDAO, DynamoDbProducts>();

            services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
            
            return services;
        }
    }
}