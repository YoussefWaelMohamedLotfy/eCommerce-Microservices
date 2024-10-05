using MongoDB.Driver;

namespace Catalog.API.Data;

public sealed class CatalogContext : ICatalogContext
{
    public IMongoCollection<Product> Products { get; }

    public CatalogContext(IConfiguration conf)
    {
        var client = new MongoClient(conf.GetValue<string>("DatabaseSettings:ConnectionString"));
        var db = client.GetDatabase(conf.GetValue<string>("DatabaseSettings:DatabaseName"));

        Products = db.GetCollection<Product>(conf.GetValue<string>("DatabaseSettings:CollectionName"));
        CatalogContextSeed.SeedDatabase(Products);
    }
}
