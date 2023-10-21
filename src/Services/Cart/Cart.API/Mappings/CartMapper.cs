using Cart.API.Data;
using Riok.Mapperly.Abstractions;
using Shared.Utilites.EventBus.Events;

namespace Cart.API.Mappings;

[Mapper]
public static partial class CartMapper
{
    public static partial CartCheckoutEvent MapToEvent(CartCheckout cartCheckout);
}
