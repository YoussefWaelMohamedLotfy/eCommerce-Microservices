using Ordering.Application.Features.Orders.Commands.CheckoutOrder;
using Riok.Mapperly.Abstractions;
using Shared.Utilites.EventBus.Events;

namespace Ordering.Application.Mappings;

[Mapper]
public static partial class OrderEventMapper
{
    public static partial CheckoutOrderCommand MapToOrderCommand(CartCheckoutEvent checkoutEvent);
}
