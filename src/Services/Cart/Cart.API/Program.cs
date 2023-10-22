using Cart.API.Data;
using Cart.API.Mappings;
using Cart.API.Repositories;
using Cart.API.Services;
using Discount.gRPC.Protos;
using MassTransit;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(Serilogger.Configure);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(o => o.Protocols = HttpProtocols.Http1AndHttp2AndHttp3);
    options.ConfigureHttpsDefaults(o => o.AllowAnyClientCertificate());
});

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:5001";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "api1");
    });
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

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, config) =>
    {
        config.Host(builder.Configuration["EventBusSettings:RabbitMQHostAddress"], "/", h =>
        {
            h.Username(builder.Configuration["EventBusSettings:RabbitMQHostUsername"]);
            h.Password(builder.Configuration["EventBusSettings:RabbitMQHostPassword"]);
        });

        config.ConfigureEndpoints(context);
    });
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
    IdentityModelEventSource.ShowPII = true;
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

var carEndpointGroup = app.MapGroup("/api/v1/Cart")
    .RequireAuthorization("ApiScope")
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

carEndpointGroup.MapPost("/Checkout", async (CartCheckout cartCheckout, ICartRepository repo,
    IPublishEndpoint publishEndpoint, CancellationToken ct) =>
{
    var cart = await repo.GetBasket(cartCheckout.UserName, ct).ConfigureAwait(false);

    if (cart is null)
    {
        return Results.BadRequest();
    }

    var eventMessage = CartMapper.MapToEvent(cartCheckout);
    eventMessage.TotalPrice = cart.TotalPrice;
    await publishEndpoint.Publish(eventMessage, ct).ConfigureAwait(false);

    await repo.DeleteBasket(cart.UserName!, ct).ConfigureAwait(false);

    return Results.Accepted();
})
    .WithSummary("Checkout User Cart and Create new Order");

app.Run();
