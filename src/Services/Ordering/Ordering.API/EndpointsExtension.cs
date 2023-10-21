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
        var orderingEndpointGroup = app.MapGroup("/api/v1/Orders")
            .WithOpenApi();

        orderingEndpointGroup.MapGet("/{id}", async (int id, IMediator mediator, CancellationToken ct) =>
        {
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
