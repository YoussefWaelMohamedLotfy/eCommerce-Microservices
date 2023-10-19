using Mediator;
using Ordering.Application.Features.Orders.Queries.GetOrdersList;
using Ordering.Application.Mappings;
using Ordering.Infrastructure.Persistence;

namespace Ordering.Application.Features.Orders.Queries.GetOrder;

public sealed record GetOrderByIdQuery(int ID) : IRequest<OrderDto?>;

internal sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly OrderingDbContext _dbContext;

    public GetOrderByIdQueryHandler(OrderingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .FindAsync(new object[] { request.ID }, cancellationToken).ConfigureAwait(false);

        return order is null ? null : OrderMapper.MapToOrderDto(order);
    }
}
