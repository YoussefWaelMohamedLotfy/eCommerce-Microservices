using Catalog.API.Data;
using Catalog.API.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ICatalogContext, CatalogContext>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var catalogEndpointGroup = app.MapGroup("/api/v1/Catalog")
    .WithOpenApi();

catalogEndpointGroup.MapGet("/", async (IProductRepository repo) =>
{
    var products = await repo.GetProducts().ConfigureAwait(false);
    return Results.Ok(products);
})
    .WithSummary("Get all products");

catalogEndpointGroup.MapGet("/{id:length(24)}", async (string id, IProductRepository repo) =>
{
    var product = await repo.GetProduct(id).ConfigureAwait(false);
    return Results.Ok(product);
})
    .WithName("GetProduct")
    .WithSummary("Get product with an ID");

catalogEndpointGroup.MapGet("/Category/{category}", async (string category, IProductRepository repo) =>
{
    var products = await repo.GetProductByCategory(category).ConfigureAwait(false);
    return Results.Ok(products);
})
    .WithSummary("Get products for a category");

catalogEndpointGroup.MapPost("/", async (Product newProduct, IProductRepository repo) =>
{
    await repo.CreateProduct(newProduct).ConfigureAwait(false);
    return Results.CreatedAtRoute("GetProduct", new { id = newProduct.Id }, newProduct);
})
    .WithSummary("Creates a new product in catalog");

catalogEndpointGroup.MapPut("/", async (Product updatedProduct, IProductRepository repo)
    => Results.Ok(await repo.UpdateProduct(updatedProduct).ConfigureAwait(false)))
    .WithSummary("Updates existing product in catalog");

catalogEndpointGroup.MapDelete("/{id:length(24)}", async (string id, IProductRepository repo) =>
{
    var isDeleted = await repo.DeleteProduct(id).ConfigureAwait(false);
    return isDeleted ? Results.NoContent() : Results.NotFound();
})
    .WithSummary("Deletes a product from catalog");

app.Run();
