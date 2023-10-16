using Cart.API.Data;

namespace Cart.API.Repositories;
public interface ICartRepository
{
    Task DeleteBasket(string userName, CancellationToken cancellationToken = default);
    Task<ShoppingCart?> GetBasket(string userName, CancellationToken cancellationToken = default);
    Task<ShoppingCart?> UpdateBasket(ShoppingCart basket, CancellationToken cancellationToken = default);
}