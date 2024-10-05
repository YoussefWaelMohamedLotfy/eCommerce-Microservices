using Discount.gRPC.Data;

namespace Discount.gRPC.Repositories;
public interface IDiscountRepository
{
    Task<bool> CreateDiscount(Coupon coupon, CancellationToken cancellationToken = default);
    Task<bool> DeleteDiscount(string productName, CancellationToken cancellationToken = default);
    Task<Coupon> GetDiscount(string productName, CancellationToken cancellationToken = default);
    Task<bool> UpdateDiscount(Coupon coupon, CancellationToken cancellationToken = default);
}