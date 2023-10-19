using Mediator;
using Microsoft.Extensions.Logging;
using Ordering.Application.Mappings;
using Ordering.Domain.Entities;
using Ordering.Infrastructure.Persistence;

namespace Ordering.Application.Features.Orders.Commands.CheckoutOrder;

internal sealed partial class CheckoutOrderCommandHandler : IRequestHandler<CheckoutOrderCommand, Order>
{
    private readonly OrderingDbContext _dbContext;
    private readonly ILogger<CheckoutOrderCommandHandler> _logger;

    public CheckoutOrderCommandHandler(OrderingDbContext dbContext, ILogger<CheckoutOrderCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async ValueTask<Order> Handle(CheckoutOrderCommand request, CancellationToken cancellationToken)
    {
        var newOrder = OrderMapper.MapToOrder(request);
        await _dbContext.AddAsync(newOrder, cancellationToken).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        LogCreateNewOrderSuccess(newOrder.ID);
        return newOrder;
    }

    [LoggerMessage(Message = "Order {newOrderId} is successfully created.", Level = LogLevel.Information)]
    public partial void LogCreateNewOrderSuccess(int newOrderId);
}
