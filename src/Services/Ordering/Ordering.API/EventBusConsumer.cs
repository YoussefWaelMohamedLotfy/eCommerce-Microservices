using MassTransit;
using Mediator;
using Ordering.Application.Mappings;
using Shared.Utilites.EventBus.Events;

namespace Ordering.API;

public sealed partial class EventBusConsumer : IConsumer<CartCheckoutEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<EventBusConsumer> _logger;

    public EventBusConsumer(IMediator mediator, ILogger<EventBusConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CartCheckoutEvent> context)
    {
        var command = OrderEventMapper.MapToOrderCommand(context.Message);
        var result = await _mediator.Send(command, context.CancellationToken).ConfigureAwait(false);
        LogBusConsumeMessageSuccess(result.ID);
    }

    [LoggerMessage(Message = "CartCheckoutEvent consumed successfully. Created Order Id : {newOrderId}", Level = LogLevel.Information)]
    public partial void LogBusConsumeMessageSuccess(int newOrderId);
}
