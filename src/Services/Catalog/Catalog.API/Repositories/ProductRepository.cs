using Catalog.API.Data;
using MongoDB.Driver;

namespace Catalog.API.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly ICatalogContext _context;

    public ProductRepository(ICatalogContext context)
        => _context = context;

    public async Task CreateProduct(Product product, CancellationToken cancellationToken = default)
        => await _context.Products.InsertOneAsync(product, cancellationToken: cancellationToken).ConfigureAwait(false);

    public async Task<bool> DeleteProduct(string id, CancellationToken cancellationToken = default)
    {
        FilterDefinition<Product> filter = Builders<Product>.Filter.Eq(p => p.Id, id);
        DeleteResult deleteResult = await _context.Products.DeleteOneAsync(filter, cancellationToken).ConfigureAwait(false);

        return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
    }

    public async Task<Product> GetProduct(string id, CancellationToken cancellationToken = default)
        => await _context.Products.Find(p => p.Id == id).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);

    public async Task<IEnumerable<Product>> GetProductByCategory(string categoryName, CancellationToken cancellationToken = default)
    {
        FilterDefinition<Product> filter = Builders<Product>.Filter.Eq(p => p.Category, categoryName);

        return await _context.Products.Find(filter).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Product>> GetProductByName(string name, CancellationToken cancellationToken = default)
    {
        FilterDefinition<Product> filter = Builders<Product>.Filter.Eq(p => p.Name, name);

        return await _context.Products.Find(filter).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Product>> GetProducts(CancellationToken cancellationToken = default)
        => await _context.Products.Find(p => true).ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<bool> UpdateProduct(Product product, CancellationToken cancellationToken = default)
    {
        var updateResult = await _context.Products
            .ReplaceOneAsync(filter: g => g.Id == product.Id, replacement: product, cancellationToken: cancellationToken).ConfigureAwait(false);

        return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
    }
}
