using Dapper;
using Discount.gRPC.Data;
using Npgsql;

namespace Discount.gRPC.Repositories;

public sealed class DiscountRepository : IDiscountRepository
{
    private readonly string? _connectionString;

    public DiscountRepository(IConfiguration config)
        => _connectionString = config.GetConnectionString("DefaultConnection");

    public async Task<Coupon> GetDiscount(string productName, CancellationToken cancellationToken = default)
    {
        using var connection = new NpgsqlConnection(_connectionString);

        var command = new CommandDefinition
            ("SELECT * FROM Coupon WHERE ProductName = @ProductName", new { ProductName = productName }, cancellationToken: cancellationToken);

        var coupon = await connection.QueryFirstOrDefaultAsync<Coupon>(command).ConfigureAwait(false);

        return coupon is null
            ? new Coupon { ProductName = "No Discount", Amount = 0, Description = "No Discount Desc" }
            : coupon;
    }

    public async Task<bool> CreateDiscount(Coupon coupon, CancellationToken cancellationToken = default)
    {
        using var connection = new NpgsqlConnection(_connectionString);

        var command = new CommandDefinition
            ("INSERT INTO Coupon (ProductName, Description, Amount) VALUES (@ProductName, @Description, @Amount)",
                        new { coupon.ProductName, coupon.Description, coupon.Amount }, cancellationToken: cancellationToken);


        var affected = await connection.ExecuteAsync(command).ConfigureAwait(false);
        return affected != 0;
    }

    public async Task<bool> UpdateDiscount(Coupon coupon, CancellationToken cancellationToken = default)
    {
        using var connection = new NpgsqlConnection(_connectionString);

        var command = new CommandDefinition
            ("UPDATE Coupon SET ProductName=@ProductName, Description = @Description, Amount = @Amount WHERE Id = @Id",
                        new { coupon.ProductName, coupon.Description, coupon.Amount, coupon.Id }, cancellationToken: cancellationToken);

        var affected = await connection.ExecuteAsync(command).ConfigureAwait(false);
        return affected != 0;
    }

    public async Task<bool> DeleteDiscount(string productName, CancellationToken cancellationToken = default)
    {
        using var connection = new NpgsqlConnection(_connectionString);

        var command = new CommandDefinition("DELETE FROM Coupon WHERE ProductName = @productName",
            new { productName }, cancellationToken: cancellationToken);

        var affected = await connection.ExecuteAsync(command).ConfigureAwait(false);
        return affected != 0;
    }
}
