using Cart.API.Data;
using Cart.API.Repositories;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(Serilogger.Configure);

builder.Services.AddScoped<ICartRepository, CartRepository>();

builder.Services.AddStackExchangeRedisCache(x =>
{
    x.InstanceName = "Cart.API";
    x.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
});

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

var carEndpointGroup = app.MapGroup("/api/v1/Cart")
    .WithOpenApi();

carEndpointGroup.MapGet("/{username}", async (string username, ICartRepository repo) =>
{
    var basket = await repo.GetBasket(username).ConfigureAwait(false);
    return Results.Ok(basket ?? new ShoppingCart(username));
})
    .WithSummary("Gets the cart for a certain username");

carEndpointGroup.MapPost("/", async (ShoppingCart updatedCart, ICartRepository repo) =>
{
    // TODO : Communicate with Discount.Grpc
    // and Calculate latest prices of product into shopping cart
    // consume Discount Grpc

    
    return Results.Ok(await repo.UpdateBasket(updatedCart).ConfigureAwait(false));
})
    .WithSummary("Updates the cart for a certain username");

carEndpointGroup.MapDelete("/{userName}", async (string username, ICartRepository repo) =>
{
    await repo.DeleteBasket(username).ConfigureAwait(false);
    return Results.NoContent();
})
    .WithSummary("Deletes the cart for a certain username");

app.Run();
