using Discount.gRPC.Data;
using Discount.gRPC.Protos;
using Discount.gRPC.Repositories;
using Grpc.Core;

namespace Discount.gRPC.Services;

public sealed class DiscountService : DiscountProtoService.DiscountProtoServiceBase
{
    private readonly IDiscountRepository _repository;
    private readonly DiscountMapper _mapper;
    private readonly ILogger<DiscountService> _logger;

    public DiscountService(IDiscountRepository repository, ILogger<DiscountService> logger, DiscountMapper mapper)
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
    }

    public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
    {
        var coupon = await _repository.GetDiscount(request.ProductName, context.CancellationToken).ConfigureAwait(false)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Discount with ProductName={request.ProductName} is not found."));

        DiscountLogger.LogSuccessGet(_logger, coupon.ProductName, coupon.Amount);

        return _mapper.MapToCouponModel(coupon);
    }

    public override async Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
    {
        var coupon = _mapper.MapToCoupon(request.Coupon);

        await _repository.CreateDiscount(coupon, context.CancellationToken).ConfigureAwait(false);
        DiscountLogger.LogSuccessCreate(_logger, coupon.ProductName);

        return _mapper.MapToCouponModel(coupon);
    }

    public override async Task<CouponModel> UpdateDiscount(UpdateDiscountRequest request, ServerCallContext context)
    {
        var coupon = _mapper.MapToCoupon(request.Coupon);

        await _repository.UpdateDiscount(coupon, context.CancellationToken).ConfigureAwait(false);
        DiscountLogger.LogSuccessUpdate(_logger, coupon.ProductName);

        return _mapper.MapToCouponModel(coupon);
    }

    public override async Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request, ServerCallContext context)
    {
        bool deleted = await _repository.DeleteDiscount(request.ProductName, context.CancellationToken).ConfigureAwait(false);
        return new() { Success = deleted };
    }
}

public static partial class DiscountLogger
{
    [LoggerMessage(Message = "Discount is retrieved for ProductName: {productName}, Amount: {amount}",
        Level = LogLevel.Information, EventId = 0)]
    public static partial void LogSuccessGet(ILogger logger, string productName, int amount);

    [LoggerMessage(Message = "Discount is successfully created. ProductName : {productName}",
        Level = LogLevel.Information, EventId = 1)]
    public static partial void LogSuccessCreate(ILogger logger, string productName);

    [LoggerMessage(Message = "Discount is successfully updated. ProductName : {productName}",
        Level = LogLevel.Information, EventId = 2)]
    public static partial void LogSuccessUpdate(ILogger logger, string productName);
}