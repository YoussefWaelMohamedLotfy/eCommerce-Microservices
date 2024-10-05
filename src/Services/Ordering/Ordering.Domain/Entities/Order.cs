using Ordering.Domain.Common;

namespace Ordering.Domain.Entities;

public sealed class Order : BaseEntity<int>
{
    public string UserName { get; set; } = default!;

    public decimal TotalPrice { get; set; }

    // BillingAddress
    public string FirstName { get; set; } = default!;

    public string LastName { get; set; } = default!;

    public string EmailAddress { get; set; } = default!;

    public string AddressLine { get; set; } = default!;

    public string Country { get; set; } = default!;

    public string State { get; set; } = default!;

    public string ZipCode { get; set; } = default!;

    // Payment
    public string? CardName { get; set; }

    public string? CardNumber { get; set; }

    public string? Expiration { get; set; }

    public string? CVV { get; set; }

    public int PaymentMethod { get; set; }
}
