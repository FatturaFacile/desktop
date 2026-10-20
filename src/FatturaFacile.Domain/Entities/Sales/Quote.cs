using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Sales;

/// <summary>
/// Represents quote.
/// </summary>
public sealed class Quote : AggregateRoot<Guid>, ITenantEntity
{
    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

/// <summary>
    /// Gets the quote number.
    /// </summary>
    public string QuoteNumber { get; private set; } = string.Empty;

/// <summary>
    /// Gets the client id.
    /// </summary>
    public Guid ClientId { get; private set; }

/// <summary>
    /// Gets the expiry date.
    /// </summary>
    public DateTime ExpiryDate { get; private set; }

/// <summary>
    /// Gets the total amount.
    /// </summary>
    public Money TotalAmount { get; private set; } = Money.Euro(0);

/// <summary>
    /// Gets a value indicating whether accepted.
    /// </summary>
    public bool IsAccepted { get; private set; }

private Quote()
    {
    }

/// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="quoteNumber">The quote number.</param>
    /// <param name="clientId">The client id.</param>
    /// <param name="expiry">The expiry.</param>
    /// <param name="total">The total.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
