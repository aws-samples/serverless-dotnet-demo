using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Shared.Models
{
    public class ProductWrapper
    {
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ProductWrapper))]
        public ProductWrapper(){}

        public ProductWrapper(List<Product> products)
        {
            this.Products = products;
        }
        
        public List<Product> Products { get; set; }
    }
}