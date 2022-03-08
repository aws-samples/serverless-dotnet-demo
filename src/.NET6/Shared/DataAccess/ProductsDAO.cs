using System.Threading.Tasks;
using Shared.Models;

namespace Shared.DataAccess
{
    public interface ProductsDAO
    {
        Task<Product> GetProduct(string id);

        Task PutProduct(Product product);

        Task DeleteProduct(string id);

        Task<ProductWrapper> GetAllProducts();
    }
}