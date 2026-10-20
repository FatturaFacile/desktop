using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Sales;

/// <summary>
/// Represents price list.
/// </summary>
public sealed class PriceList : Entity<Guid>, ITenantEntity
{
    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

/// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

/// <summary>
    /// Gets the currency.
    /// </summary>
    public Currency Currency { get; private set; }

/// <summary>
    /// Gets a value indicating whether default.
    /// </summary>
    public bool IsDefault { get; private set; }

private PriceList()
    {
    }

/// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="name">The name.</param>
    /// <param name="currency">The currency.</param>
    /// <param name="isDefault">The is default.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static PriceList Create(Guid tenantId, string name, Currency currency, bool isDefault = false)
    {
        return new PriceList
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Currency = currency,
            IsDefault = isDefault,
        };
    }
}
