using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Catalog.API.Data;

public sealed class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string Category { get; set; } = default!;

    public string Summary { get; set; } = default!;

    public string Description { get; set; } = default!;

    public string ImageFileUrl { get; set; } = default!;

    public decimal Price { get; set; }
}
