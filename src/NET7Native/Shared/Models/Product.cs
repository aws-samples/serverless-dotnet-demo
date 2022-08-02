using System;

namespace Shared.Models
{
    public class Product
    {
        public Product()
        {
            this.Id = string.Empty;
            this.Name = string.Empty;
        }

        public Product(string id, string name, decimal price)
        {
            this.Id = id;
            this.Name = name;
            this.Price = price;
        }
        
        public string Id { get; set; }
        
        public string Name { get; set; }
        
        public decimal Price { get; private set; }

        public void SetPrice(decimal newPrice)
        {
            this.Price = Math.Round(newPrice, 2);
        }

        public override string ToString()
        {
            return "Product{" +
                   "id='" + this.Id + '\'' +
                   ", name='" + this.Name + '\'' +
                   ", price=" + this.Price +
                   '}';
        }
    }
}