using Asp.Versioning.Builder;

using Cart.API.Data;
using Cart.API.Mappings;
using Cart.API.Repositories;
using Cart.API.Services;
using MassTransit;

using Nest;

namespace Cart.API;

public static class EndpointsExtension
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var versionSet = app.NewVersionedApi("Cart");

        var cartEndpointGroup = versionSet.MapGroup("/api/v{version:apiVersion}/Cart")
            .HasApiVersion(1)
            .RequireAuthorization("ApiScope")
            .WithOpenApi();

        var cartEndpointGroup2 = versionSet.MapGroup("/api/v{version:apiVersion}/Cart")
            .HasApiVersion(2);

        cartEndpointGroup2.MapGet("/{username}", (string username) => $"Are you looking for Username {username} in V2?")
            .MapToApiVersion(2);

        cartEndpointGroup.MapGet("/{username}", async (string username, ICartRepository repo, CancellationToken ct) =>
        {
            var basket = await repo.GetBasket(username, ct).ConfigureAwait(false);
            return Results.Ok(basket ?? new ShoppingCart(username));
        })
            .WithSummary("Gets the cart for a certain username");

        cartEndpointGroup.MapPost("/", async (ShoppingCart updatedCart, DiscountGrpcService discountService,
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

        cartEndpointGroup.MapDelete("/{userName}", async (string username, ICartRepository repo,
            CancellationToken ct) =>
        {
            await repo.DeleteBasket(username, ct).ConfigureAwait(false);
            return Results.NoContent();
        })
            .WithSummary("Deletes the cart for a certain username");

        cartEndpointGroup.MapPost("/Checkout", async (CartCheckout cartCheckout, ICartRepository repo,
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

        return app;
    }
}
