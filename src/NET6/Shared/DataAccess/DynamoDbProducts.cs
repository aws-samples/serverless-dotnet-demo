using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Shared.Models;

namespace Shared.DataAccess
{
    public class DynamoDbProducts : ProductsDAO
    {
        private static string PRODUCT_TABLE_NAME = Environment.GetEnvironmentVariable("PRODUCT_TABLE_NAME");
        private readonly AmazonDynamoDBClient _dynamoDbClient;

        public DynamoDbProducts()
        {
            this._dynamoDbClient = new AmazonDynamoDBClient();
        }
        
        public async Task<Product> GetProduct(string id)
        {
            var getItemResponse = await this._dynamoDbClient.GetItemAsync(new GetItemRequest(PRODUCT_TABLE_NAME,
                new Dictionary<string, AttributeValue>(1)
                {
                    {ProductMapper.PK, new AttributeValue(id)}
                }));

            return getItemResponse.IsItemSet ? ProductMapper.ProductFromDynamoDB(getItemResponse.Item) : null;
        }

        public async Task PutProduct(Product product)
        {
            await this._dynamoDbClient.PutItemAsync(PRODUCT_TABLE_NAME, ProductMapper.ProductToDynamoDb(product));
        }

        public async Task DeleteProduct(string id)
        {
            await this._dynamoDbClient.DeleteItemAsync(PRODUCT_TABLE_NAME, new Dictionary<string, AttributeValue>(1)
            {
                {ProductMapper.PK, new AttributeValue(id)}
            });
        }

        public async Task<ProductWrapper> GetAllProducts()
        {
            var data = await this._dynamoDbClient.ScanAsync(new ScanRequest()
            {
                TableName = PRODUCT_TABLE_NAME,
                Limit = 20
            });

            var products = new List<Product>();

            if (data.Items != null)
            {
                foreach (var item in data.Items)
                {
                    products.Add(ProductMapper.ProductFromDynamoDB(item));
                }
            }

            return new ProductWrapper(products);
        }
    }
}