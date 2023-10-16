using Catalog.API.Data;

namespace Catalog.API.Repositories;
public interface IProductRepository
{
    Task CreateProduct(Product product, CancellationToken cancellationToken = default);
    Task<bool> DeleteProduct(string id, CancellationToken cancellationToken = default);
    Task<Product> GetProduct(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetProductByCategory(string categoryName, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetProductByName(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetProducts(CancellationToken cancellationToken = default);
    Task<bool> UpdateProduct(Product product, CancellationToken cancellationToken = default);
}