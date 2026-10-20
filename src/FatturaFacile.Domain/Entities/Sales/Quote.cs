using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Sales;

public sealed class Quote : AggregateRoot<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string QuoteNumber { get; private set; } = string.Empty;
    public Guid ClientId { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public Money TotalAmount { get; private set; } = Money.Euro(0);
    public bool IsAccepted { get; private set; }

    private Quote()
    {
    }

    public static Quote Create(Guid tenantId, string quoteNumber, Guid clientId, DateTime expiry, Money total)
    {
        return new Quote
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            QuoteNumber = quoteNumber,
            ClientId = clientId,
            ExpiryDate = expiry,
            TotalAmount = total,
            IsAccepted = false,
        };
    }
}