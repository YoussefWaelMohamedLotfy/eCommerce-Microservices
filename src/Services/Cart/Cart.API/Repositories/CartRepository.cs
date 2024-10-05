using System.Text.Json;
using Cart.API.Data;
using Microsoft.Extensions.Caching.Distributed;

namespace Cart.API.Repositories;

public sealed class CartRepository : ICartRepository
{
    private readonly IDistributedCache _redisCache;

    public CartRepository(IDistributedCache redisCache)
        => _redisCache = redisCache;

    public async Task DeleteBasket(string userName, CancellationToken cancellationToken = default)
        => await _redisCache.RemoveAsync(userName, cancellationToken).ConfigureAwait(false);

    public async Task<ShoppingCart?> GetBasket(string userName, CancellationToken cancellationToken = default)
    {
        var basket = await _redisCache.GetStringAsync(userName, cancellationToken).ConfigureAwait(false);
        return string.IsNullOrEmpty(basket) ? null : JsonSerializer.Deserialize<ShoppingCart>(basket, ShoppingCartContext.Default.ShoppingCart);
    }

    public async Task<ShoppingCart?> UpdateBasket(ShoppingCart basket, CancellationToken cancellationToken = default)
    {
        await _redisCache.SetStringAsync(basket.UserName, JsonSerializer.Serialize(basket, ShoppingCartContext.Default.ShoppingCart), cancellationToken).ConfigureAwait(false);
        return await GetBasket(basket.UserName, cancellationToken).ConfigureAwait(false);
    }
}
