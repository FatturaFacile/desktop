// <copyright file="BillOfQuantitiesItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Construction;

/// <summary>
/// Represents bill of quantities item.
/// </summary>
public sealed class BillOfQuantitiesItem : Entity<Guid>, ITenantEntity
{
    private BillOfQuantitiesItem()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the construction site id.
    /// </summary>
    public Guid ConstructionSiteId { get; private set; }

    /// <summary>
    /// Gets the regional tariff code (e.g. Lombardia or SIA/CRB/NPK).
    /// </summary>
    public string ItemCode { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the standard type.
    /// </summary>
    public StandardType StandardType { get; private set; }

    /// <summary>
    /// Gets the description.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the unit of measure.
    /// </summary>
    public string UnitOfMeasure { get; private set; } = "m2";

    /// <summary>
    /// Gets the quantity.
    /// </summary>
    public decimal Quantity { get; private set; }

    /// <summary>
    /// Gets the unit price.
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// Gets the subtotal.
    /// </summary>
    public decimal Subtotal => decimal.Round(
        Quantity * UnitPrice,
        2,
        MidpointRounding.AwayFromZero);

    /// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="siteId">The site id.</param>
    /// <param name="code">The code.</param>
    /// <param name="standard">The standard.</param>
    /// <param name="desc">The desc.</param>
    /// <param name="uom">The uom.</param>
    /// <param name="qty">The qty.</param>
    /// <param name="unitPrice">The unit price.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static BillOfQuantitiesItem Create(
        Guid tenantId,
        Guid siteId,
        string code,
        StandardType standard,
        string desc,
        string uom,
        decimal qty,
        decimal unitPrice)
    {
        return new BillOfQuantitiesItem
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ConstructionSiteId = siteId,
            ItemCode = code?.Trim() ?? string.Empty,
            StandardType = standard,
            Description = desc?.Trim() ?? string.Empty,
            UnitOfMeasure = uom?.Trim() ?? string.Empty,
            Quantity = qty,
            UnitPrice = unitPrice,
        };
    }
}
