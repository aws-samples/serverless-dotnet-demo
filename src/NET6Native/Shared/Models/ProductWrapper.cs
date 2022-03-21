using System.Collections.Generic;

namespace Shared.Models
{
    public class ProductWrapper
    {
        public ProductWrapper()
        {
            this.Products = new List<Product>();
        }

        public ProductWrapper(List<Product> products)
        {
            this.Products = products;
        }
        
        public List<Product> Products { get; set; }
    }
}