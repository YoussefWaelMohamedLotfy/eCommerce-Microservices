using Catalog.API.Data;
using Catalog.API.Repositories;

namespace Catalog.API;

public static class EndpointExtension
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var versionSet = app.NewVersionedApi("Catalog");

        var catalogEndpointGroup = versionSet.MapGroup("/api/v{version:apiVersion}/Catalog")
            .HasApiVersion(1)
            .RequireAuthorization("ApiScope")
            .WithOpenApi();

        var catalogEndpointGroup2 = versionSet.MapGroup("/api/v{version:apiVersion}/Catalog")
            .HasApiVersion(2);

        catalogEndpointGroup2.MapGet("/{id:length(24)}", (string id) => $"Are you looking for ID {id} in V2?")
            .MapToApiVersion(2);

        catalogEndpointGroup.MapGet("/", async (IProductRepository repo, CancellationToken ct) =>
        {
            var products = await repo.GetProducts(ct).ConfigureAwait(false);
            return Results.Ok(products);
        })
            .WithSummary("Get all products");

        catalogEndpointGroup.MapGet("/{id:length(24)}", async (string id, IProductRepository repo, CancellationToken ct) =>
        {
            var product = await repo.GetProduct(id, ct).ConfigureAwait(false);
            return Results.Ok(product);
        })
            .CacheOutput()
            .WithName("GetProduct")
            .WithSummary("Get product with an ID");

        catalogEndpointGroup.MapGet("/Category/{category}", async (string category, IProductRepository repo,
            CancellationToken ct) =>
        {
            var products = await repo.GetProductByCategory(category, ct).ConfigureAwait(false);
            return Results.Ok(products);
        })
            .WithSummary("Get products for a category");

        catalogEndpointGroup.MapPost("/", async (Product newProduct, IProductRepository repo, CancellationToken ct) =>
        {
            await repo.CreateProduct(newProduct, ct).ConfigureAwait(false);
            return Results.CreatedAtRoute("GetProduct", new { id = newProduct.Id }, newProduct);
        })
            .WithSummary("Creates a new product in catalog");

        catalogEndpointGroup.MapPut("/", async (Product updatedProduct, IProductRepository repo, CancellationToken ct)
            => Results.Ok(await repo.UpdateProduct(updatedProduct, ct).ConfigureAwait(false)))
            .WithSummary("Updates existing product in catalog");

        catalogEndpointGroup.MapDelete("/{id:length(24)}", async (string id, IProductRepository repo, CancellationToken ct) =>
        {
            var isDeleted = await repo.DeleteProduct(id, ct).ConfigureAwait(false);
            return isDeleted ? Results.NoContent() : Results.NotFound();
        })
            .WithSummary("Deletes a product from catalog");

        return app;
    }
}
