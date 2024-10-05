using Mediator;
using Ordering.Application.Features.Orders.Commands.CheckoutOrder;
using Ordering.Application.Features.Orders.Commands.DeleteOrder;
using Ordering.Application.Features.Orders.Commands.UpdateOrder;
using Ordering.Application.Features.Orders.Queries.GetOrder;
using Ordering.Application.Features.Orders.Queries.GetOrdersList;

namespace Catalog.API;

public static class EndpointsExtension
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var versionSet = app.NewVersionedApi("Orders");

        var orderingEndpointGroup = versionSet.MapGroup("/api/v{version:apiVersion}/Orders")
            .HasApiVersion(1)
            .RequireAuthorization("ApiScope")
            .WithOpenApi();

        var orderingEndpointGroup2 = versionSet.MapGroup("/api/v{version:apiVersion}/Orders")
            .HasApiVersion(2);

        orderingEndpointGroup2.MapGet("/{id}", (int id) => $"Are you looking for ID {id} in V2?")
            .MapToApiVersion(2);

        orderingEndpointGroup.MapGet("/{id}", async (int id, HttpContext context, IMediator mediator, CancellationToken ct) =>
        {
            Console.WriteLine($"Requested API Version: {context.GetRequestedApiVersion()}");
            var order = await mediator.Send(new GetOrderByIdQuery(id), ct).ConfigureAwait(false);
            return order is null ? Results.NotFound() : Results.Ok(order);
        })
            .WithName("GetOrder")
            .WithSummary("Get an order by ID");

        orderingEndpointGroup.MapGet("/username/{username}", async (string username, IMediator mediator, CancellationToken ct)
            => Results.Ok(await mediator.Send(new GetOrdersListQuery(username), ct).ConfigureAwait(false)))
            .WithSummary("Get orders for a certain username");

        orderingEndpointGroup.MapPost("/", async (CheckoutOrderCommand newOrder, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(newOrder, ct).ConfigureAwait(false);
            return Results.CreatedAtRoute("GetOrder", new { id = result.ID }, result);
        })
            .WithSummary("Create a new Order")
            .WithDescription("This endpoint is for testing purposes only.");

        orderingEndpointGroup.MapPut("/", async (UpdateOrderCommand updatedOrder, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(updatedOrder, ct).ConfigureAwait(false);
            return Results.Ok(result);
        })
            .WithSummary("Updates an Existing Order");

        orderingEndpointGroup.MapDelete("/{id}", async (int id, IMediator mediator, CancellationToken ct)
            => Results.Ok(await mediator.Send(new DeleteOrderCommand(id), ct).ConfigureAwait(false)))
            .WithSummary("Delete an orders by ID");

        return app;
    }
}
