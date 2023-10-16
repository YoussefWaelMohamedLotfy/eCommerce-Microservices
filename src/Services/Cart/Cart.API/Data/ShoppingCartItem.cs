namespace Cart.API.Data;

public sealed class ShoppingCartItem
{
    public int Quantity { get; set; }

    public string Color { get; set; } = default!;

    public decimal Price { get; set; }

    public string ProductId { get; set; } = default!;

    public string ProductName { get; set; } = default!;
}
