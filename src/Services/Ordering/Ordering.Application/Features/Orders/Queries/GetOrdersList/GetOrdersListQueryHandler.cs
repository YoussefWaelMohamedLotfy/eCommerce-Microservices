using Mediator;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Mappings;
using Ordering.Infrastructure.Persistence;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersList;

public sealed record GetOrdersListQuery(string UserName) : IRequest<IEnumerable<OrderDto>>;

internal sealed class GetOrdersListQueryHandler : IRequestHandler<GetOrdersListQuery, IEnumerable<OrderDto>>
{
    private readonly OrderingDbContext _dbContext;

    public GetOrdersListQueryHandler(OrderingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<IEnumerable<OrderDto>> Handle(GetOrdersListQuery request, CancellationToken cancellationToken)
    {
        var orders = await _dbContext.Orders
            .Where(x => x.UserName == request.UserName)
            .ProjectToDto()
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return !orders.Any() ? Enumerable.Empty<OrderDto>() : orders;
    }
}
