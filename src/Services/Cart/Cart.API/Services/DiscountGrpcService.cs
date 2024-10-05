using Discount.gRPC.Protos;

namespace Cart.API.Services;

public sealed class DiscountGrpcService
{
    private readonly DiscountProtoService.DiscountProtoServiceClient _discountProtoService;

    public DiscountGrpcService(DiscountProtoService.DiscountProtoServiceClient discountProtoService)
        => _discountProtoService = discountProtoService;

    public async Task<CouponModel> GetDiscount(string productName, CancellationToken cancellationToken = default)
    {
        var discountRequest = new GetDiscountRequest { ProductName = productName };
        return await _discountProtoService.GetDiscountAsync(discountRequest, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
