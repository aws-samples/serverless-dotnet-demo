using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.DataAccess;
using Shared.Models;

var app = Startup.Build(args);

var dataAccess = app.Services.GetRequiredService<ProductsDAO>();

app.MapGet("/", async (HttpContext context) =>
{
    app.Logger.LogInformation("Received request to list all products");

    var products = await dataAccess.GetAllProducts();

    app.Logger.LogInformation($"Found {products.Products.Count} products(s)");

    context.Response.StatusCode = (int) HttpStatusCode.OK;
    context.Response.ContentType = "application/json";
    
    await context.Response.WriteAsync(JsonSerializer.Serialize(products, ApiSerializerContext.Default.ProductWrapper));
});

app.MapDelete("/{id}", async (HttpContext context) =>
{
    try
    {
        var id = context.Request.RouteValues["id"].ToString();

        app.Logger.LogInformation($"Received request to delete {id}");

        var product = await dataAccess.GetProduct(id);

        if (product == null)
        {
            app.Logger.LogWarning($"Id {id} not found.");

            Results.NotFound();
            return;
        }

        app.Logger.LogInformation($"Deleting {product.Name}");

        await dataAccess.DeleteProduct(product.Id);

        app.Logger.LogInformation("Delete complete");

        context.Response.StatusCode = (int) HttpStatusCode.OK;
        context.Response.ContentType = "application/json";
        
        await context.Response.WriteAsync($"Product with id {id} deleted");
    }
    catch (Exception e)
    {
        app.Logger.LogError(e, "Failure deleting product");

        context.Response.StatusCode = (int) HttpStatusCode.NotFound;
    }
});

app.MapPut("/{id}", async (HttpContext context) =>
{
    try
    {
        var id = context.Request.RouteValues["id"].ToString();

        app.Logger.LogInformation($"Received request to put {id}");

        var product = await JsonSerializer.DeserializeAsync<Product>(context.Request.Body, ApiSerializerContext.Default.Product);

        if (product == null || id != product.Id)
        {
            app.Logger.LogWarning("Product ID in the body does not match path parameter");

            await context.WriteResponse(HttpStatusCode.BadRequest, "Product ID in the body does not match path parameter");
            
            return;
        }

        app.Logger.LogInformation("Putting product");

        await dataAccess.PutProduct(product);

        app.Logger.LogTrace("Done");

        await context.WriteResponse(HttpStatusCode.OK, $"Created product with id {id}");
    }
    catch (Exception e)
    {
        app.Logger.LogError(e, "Failure deleting product");

        await context.WriteResponse(HttpStatusCode.BadRequest);
    }
});

app.MapGet("/{id}", async (HttpContext context) =>
{
    var id = context.Request.RouteValues["id"].ToString();

    app.Logger.LogInformation($"Received request to get {id}");

    var product = await dataAccess.GetProduct(id);

    if (product == null)
    {
        app.Logger.LogWarning($"{id} not found");
        await context.WriteResponse(HttpStatusCode.NotFound);

        return;
    }

    await context.WriteResponse(HttpStatusCode.OK,
        JsonSerializer.Serialize(product, ApiSerializerContext.Default.Product));
});

app.Run();

static class ResponseWriter
{
    public static async Task<HttpContext> WriteResponse(this HttpContext context, HttpStatusCode statusCode)
    {
        return await context.WriteResponse(statusCode, "");
    }
    
    public static async Task<HttpContext> WriteResponse(this HttpContext context, HttpStatusCode statusCode, string body)
    {
        if (statusCode != HttpStatusCode.NotFound)
        {
            Results.NotFound();
            return context;
        }
        
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(body);

        return context;
    }
}