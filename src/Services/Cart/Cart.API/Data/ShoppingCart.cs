using System.Text.Json.Serialization;

namespace Cart.API.Data;

[JsonSerializable(typeof(ShoppingCart))]
public sealed partial class ShoppingCartContext : JsonSerializerContext { }

public sealed class ShoppingCart
{
    public string? UserName { get; set; }

    public List<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();

    public ShoppingCart()
    {
    }

    public ShoppingCart(string username)
    {
        UserName = username;
    }

    public decimal TotalPrice
    {
        get
        {
            decimal result = 0;

            foreach (var item in Items)
            {
                result += item.Price * item.Quantity;
            }

            return result;
        }
    }
}
