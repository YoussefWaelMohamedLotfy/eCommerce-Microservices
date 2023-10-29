using Discount.gRPC.Data;
using Discount.gRPC.Protos;
using Discount.gRPC.Repositories;
using Grpc.Core;

namespace Discount.gRPC.Services;

public sealed partial class DiscountServiceV2 : DiscountProtoServiceV2.DiscountProtoServiceV2Base
{
    private readonly ILogger<DiscountServiceV1> _logger;

    public DiscountServiceV2(ILogger<DiscountServiceV1> logger)
        => _logger = logger;

    public override async Task<EchoMessage> GetDiscount(GetDiscountRequestV2 request, ServerCallContext context)
    {
        LogSuccessGet(request.ProductName);
        return new() { ProductNameEcho = $"Are you looking for Product {request.ProductName} in V2?" };
    }

    [LoggerMessage(Message = "Discount is retrieved for Product {productName} in V2",
        Level = LogLevel.Information)]
    public partial void LogSuccessGet(string productName);
}
