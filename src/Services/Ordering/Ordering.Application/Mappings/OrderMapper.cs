using Mediator;

using Ordering.Application.Features.Orders.Commands.CheckoutOrder;
using Ordering.Application.Features.Orders.Commands.UpdateOrder;
using Ordering.Application.Features.Orders.Queries.GetOrdersList;
using Ordering.Domain.Common;
using Ordering.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Ordering.Application.Mappings;

[Mapper]
public static partial class OrderMapper
{
    public static partial IQueryable<OrderDto> ProjectToDto(this IQueryable<Order> q);

    public static partial Order MapToOrder(CheckoutOrderCommand command);

    public static partial void MapToOrder(UpdateOrderCommand command, Order order);

    public static partial OrderDto MapToOrderDto(Order order);
}
