using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.DataAccess;
using Shared.Models;

var builder = WebApplication.CreateSlimBuilder(args);
            
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolver = ApiSerializerContext.Default;
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

var app = builder.Build();

Handlers.DataAccess = app.Services.GetRequiredService<ProductsDAO>();
Handlers.Logger = app.Logger;

app.MapGet("/", Handlers.GetAllProducts);

app.MapDelete("/{id}", Handlers.DeleteProduct);

app.MapPut("/{id}", Handlers.PutProduct);

app.MapGet("/{id}", Handlers.GetProduct);

app.Run();

static class Handlers
{
    internal static ProductsDAO DataAccess;
    internal static ILogger Logger;
    
    public static async Task GetAllProducts(HttpContext context)
    {
        Logger.LogInformation("Received request to list all products");

        var products = await DataAccess.GetAllProducts();

        Logger.LogInformation($"Found {products.Products.Count} products(s)");

        await context.WriteResponse(HttpStatusCode.OK, products);
    }

    public static async Task DeleteProduct(HttpContext context)
    {
        try
        {
            var id = context.Request.RouteValues["id"].ToString();
            
            Logger.LogInformation($"Received request to delete {id}");

            var product = await DataAccess.GetProduct(id);

            if (product == null)
            {
                Logger.LogWarning($"Id {id} not found.");

                await context.WriteResponse(HttpStatusCode.NotFound);
                
                return;
            }

            Logger.LogInformation($"Deleting {product.Name}");

            await DataAccess.DeleteProduct(product.Id);

            Logger.LogInformation("Delete complete");
        
            await context.WriteResponse(HttpStatusCode.OK, $"Product with id {id} deleted");
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failure deleting product");

            await context.WriteResponse(HttpStatusCode.BadRequest);
        }
    }

    public static async Task GetProduct(HttpContext context)
    {
        var id = context.Request.RouteValues["id"].ToString();
        
        Logger.LogInformation($"Received request to get {id}");

        var product = await DataAccess.GetProduct(id);

        if (product == null)
        {
            Logger.LogWarning($"{id} not found");
            await context.WriteResponse(HttpStatusCode.NotFound, $"{id} not found");
        }

        await context.WriteResponse(HttpStatusCode.OK, product);
    }
    
    public static async Task PutProduct(HttpContext context)
    {
        var id = context.Request.RouteValues["id"].ToString();
        var product = await JsonSerializer.DeserializeAsync<Product>(context.Request.Body, ApiSerializerContext.Default.Product);
        
        if (product == null || id != product.Id)
        {
            await context.WriteResponse(HttpStatusCode.BadRequest, "Product ID in the body does not match path parameter");
        }

        await DataAccess.PutProduct(product);

        await context.WriteResponse(HttpStatusCode.OK, $"Created product with id {id}");
    }
}

static class ResponseWriter
{
    public static async Task WriteResponse(this HttpContext context, HttpStatusCode statusCode)
    {
        await context.WriteResponse<string>(statusCode, "");
    }
    
    public static async Task WriteResponse<TResponseType>(this HttpContext context, HttpStatusCode statusCode, TResponseType body) where TResponseType : class
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, typeof(TResponseType), ApiSerializerContext.Default));
    }
}