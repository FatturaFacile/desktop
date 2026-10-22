// <copyright file="PriceList.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Sales;

/// <summary>
/// Represents a commercial pricing list entity defining default or customer-specific catalog tariffs,
/// applicable currency, and default selection flags.
/// </summary>
public sealed class PriceList : Entity<Guid>, ITenantEntity
{

    /// <summary>
    /// Initializes a new instance of the <see cref="PriceList"/> class for entity framework or serialization.
    /// </summary>
    private PriceList()
    {
    }

    /// <summary>
    /// Gets or sets the unique tenant identifier for multi-tenant data partitioning.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the human-readable catalog or tier name of this price list (e.g., "Standard Wholesale", "Retail B2C").
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the ISO currency applied to all items priced within this list.
    /// </summary>
    public Currency Currency { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this price list is automatically chosen as the default fallback for new customers and quotes.
    /// </summary>
    public bool IsDefault { get; private set; }

    /// <summary>
    /// Creates a new <see cref="PriceList"/> instance within the specified tenant boundary.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="name">The name or description of the pricing tier.</param>
    /// <param name="currency">The currency in which prices are listed.</param>
    /// <param name="isDefault"><see langword="true"/> if this pricing list serves as the tenant-wide default; otherwise, <see langword="false"/>.</param>
    /// <returns>A configured <see cref="PriceList"/> entity.</returns>
    public static PriceList Create(
        Guid     tenantId,
        string   name,
        Currency currency,
        bool     isDefault = false)
    {
        return new PriceList
               {
                   Id        = Guid.NewGuid(),
                   TenantId  = tenantId,
                   Name      = name,
                   Currency  = currency,
                   IsDefault = isDefault,
               };
    }

}
