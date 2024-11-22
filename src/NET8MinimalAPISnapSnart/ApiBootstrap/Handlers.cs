using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.DataAccess;
using Shared.Models;

namespace ApiBootstrap;

public class Handlers(ILogger<Handlers> logger, ProductsDAO products)
{
    public async Task<ActionResult<List<Product>>> ListProducts()
    {
        logger.LogInformation("Received request to list all products");

        var productList = await products.GetAllProducts();

        logger.LogInformation($"Found {productList.Products.Count} products(s)");
        
        return new OkObjectResult(products);
    }
    
    public async Task<ActionResult<Product>> GetProduct(string productId)
    {
        logger.LogInformation("Received request to list all products");

        var product = await products.GetProduct(productId);

        if (product == null)
        {
            logger.LogWarning($"{productId} not found");
            return new NotFoundResult();
        }
        
        return new OkObjectResult(product);
    }
}