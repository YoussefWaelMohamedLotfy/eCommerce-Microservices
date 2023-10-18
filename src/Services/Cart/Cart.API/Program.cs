using Cart.API.Data;
using Cart.API.Repositories;
using Cart.API.Services;
using Discount.gRPC.Protos;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(Serilogger.Configure);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(o => o.Protocols = HttpProtocols.Http1AndHttp2AndHttp3);
    options.ConfigureHttpsDefaults(o => o.AllowAnyClientCertificate());
});

builder.Services.AddScoped<ICartRepository, CartRepository>();

builder.Services.AddStackExchangeRedisCache(x =>
{
    x.InstanceName = "Cart.API-";
    x.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
});

builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(o
    => o.Address = new Uri(builder.Configuration.GetConnectionString("DiscountGrpcUrl")!));

builder.Services.AddScoped<DiscountGrpcService>();

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

carEndpointGroup.MapGet("/{username}", async (string username, ICartRepository repo, CancellationToken ct) =>
{
    var basket = await repo.GetBasket(username, ct).ConfigureAwait(false);
    return Results.Ok(basket ?? new ShoppingCart(username));
})
    .WithSummary("Gets the cart for a certain username");

carEndpointGroup.MapPost("/", async (ShoppingCart updatedCart, DiscountGrpcService discountService,
    ICartRepository repo, CancellationToken ct) =>
{
    // Communicate with Discount.Grpc and Calculate latest prices of product into shopping cart
    foreach (var item in updatedCart.Items)
    {
        var coupon = await discountService.GetDiscount(item.ProductName, ct).ConfigureAwait(false);
        item.Price -= coupon.Amount;
    }
    
    return Results.Ok(await repo.UpdateBasket(updatedCart, ct).ConfigureAwait(false));
})
    .WithSummary("Updates the cart for a certain username");

carEndpointGroup.MapDelete("/{userName}", async (string username, ICartRepository repo,
    CancellationToken ct) =>
{
    await repo.DeleteBasket(username, ct).ConfigureAwait(false);
    return Results.NoContent();
})
    .WithSummary("Deletes the cart for a certain username");

app.Run();
