namespace Shared.Models
{
    public class Product
    {
        public Product()
        {
        }

        public Product(string id, string name, decimal price)
        {
            this.Id = id;
            this.Name = name;
            this.Price = price;
        }
        
        public string Id { get; set; }
        
        public string Name { get; set; }
        
        public decimal Price { get; set; }

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