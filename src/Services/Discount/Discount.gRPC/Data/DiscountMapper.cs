using Discount.gRPC.Protos;

using Riok.Mapperly.Abstractions;

namespace Discount.gRPC.Data;

[Mapper]
public sealed partial class DiscountMapper
{
    public partial CouponModel MapToCouponModel(Coupon coupon);
    public partial Coupon MapToCoupon(CouponModel couponModel);
}
