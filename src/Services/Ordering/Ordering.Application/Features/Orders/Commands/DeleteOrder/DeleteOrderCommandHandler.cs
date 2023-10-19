using Mediator;
using Microsoft.Extensions.Logging;
using Ordering.Infrastructure.Persistence;

namespace Ordering.Application.Features.Orders.Commands.DeleteOrder;

public sealed record DeleteOrderCommand(int ID) : IRequest<bool>;

internal sealed partial class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, bool>
{
    private readonly OrderingDbContext _dbContext;
    private readonly ILogger<DeleteOrderCommandHandler> _logger;

    public DeleteOrderCommandHandler(OrderingDbContext dbContext, ILogger<DeleteOrderCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async ValueTask<bool> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var orderToDelete = await _dbContext.Orders.FindAsync(new object[] { request.ID }, cancellationToken).ConfigureAwait(false);

        if (orderToDelete is null)
        {
            LogOrderNotFound();
            return false;
        }

        _dbContext.Orders.Remove(orderToDelete);
        var affectedCount = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        LogOrderDeleteSuccess(orderToDelete.ID);
        return affectedCount == 1;
    }

    [LoggerMessage(Message = "Order does not exist in Database", Level = LogLevel.Error)]
    public partial void LogOrderNotFound();

    [LoggerMessage(Message = "Order {orderId} is successfully deleted.", Level = LogLevel.Information, EventId = 1)]
    public partial void LogOrderDeleteSuccess(int orderId);
}
