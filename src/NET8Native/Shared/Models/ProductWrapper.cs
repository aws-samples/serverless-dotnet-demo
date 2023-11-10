using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Shared.Models
{
    public class ProductWrapper
    {
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ProductWrapper))]
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