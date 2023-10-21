using Mediator;
using Microsoft.Extensions.Logging;
using Ordering.Application.Mappings;
using Ordering.Domain.Entities;
using Ordering.Infrastructure.Persistence;

namespace Ordering.Application.Features.Orders.Commands.UpdateOrder;

internal sealed partial class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, Order?>
{
    private readonly OrderingDbContext _dbContext;
    private readonly ILogger<UpdateOrderCommandHandler> _logger;

    public UpdateOrderCommandHandler(OrderingDbContext dbContext, ILogger<UpdateOrderCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async ValueTask<Order?> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderToUpdate = await _dbContext.Orders.FindAsync(new object[] { request.ID }, cancellationToken).ConfigureAwait(false);
        
        if (orderToUpdate is null)
        {
            LogOrderNotFound();
            return null;
        }

        OrderMapper.MapToOrder(request, orderToUpdate);
        _dbContext.Update(orderToUpdate);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return orderToUpdate;
    }

    [LoggerMessage(Message = "Order does not exist in Database", Level = LogLevel.Error)]
    public partial void LogOrderNotFound();
}
