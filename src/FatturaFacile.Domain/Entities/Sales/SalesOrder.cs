using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Sales;

/// <summary>
/// Represents sales order.
/// </summary>
public sealed class SalesOrder : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

/// <summary>
    /// Gets the order number.
    /// </summary>
    public string OrderNumber { get; private set; } = string.Empty;

/// <summary>
    /// Gets the client id.
    /// </summary>
    public Guid ClientId { get; private set; }

/// <summary>
    /// Gets the order date.
    /// </summary>
    public DateTime OrderDate { get; private set; }

/// <summary>
    /// Gets the currency.
    /// </summary>
    public Currency Currency { get; private set; } = Currency.EUR;

/// <summary>
    /// Gets the total amount.
    /// </summary>
    public decimal TotalAmount { get; }

/// <summary>
    /// Gets the status.
    /// </summary>
    public string Status { get; } = "Confirmed";

/// <summary>
    /// Gets or sets the created at utc.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

/// <summary>
    /// Gets or sets the created by.
    /// </summary>
    public string? CreatedBy { get; set; }

/// <summary>
    /// Gets or sets the updated at utc.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

/// <summary>
    /// Gets or sets the updated by.
    /// </summary>
    public string? UpdatedBy { get; set; }

private SalesOrder()
    {
    }

/// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="orderNumber">The order number.</param>
    /// <param name="clientId">The client id.</param>
    /// <param name="currency">The currency.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static SalesOrder Create(Guid tenantId, string orderNumber, Guid clientId, Currency currency)
    {
        return new SalesOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OrderNumber = orderNumber,
            ClientId = clientId,
            OrderDate = DateTime.UtcNow,
            Currency = currency,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
