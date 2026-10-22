// <copyright file="BillOfQuantitiesItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Construction;

/// <summary>
/// Represents a bill of quantities line item (Computo Metrico Estimativo) within a construction project,
/// mapped to standard regional civil engineering price lists (Prezzari Regionali OO.PP.) or Swiss CRB/NPK standards.
/// </summary>
public sealed class BillOfQuantitiesItem : Entity<Guid>, ITenantEntity
{
    private BillOfQuantitiesItem()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant organization executing the construction project.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique identifier of the construction site to which this item belongs.
    /// </summary>
    public Guid ConstructionSiteId { get; private set; }

    /// <summary>
    /// Gets the official standardized tariff code from the regional price book (e.g., Lombardia tariff code) or Swiss CRB/NPK catalogue.
    /// </summary>
    public string ItemCode { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the standard classification system applied to this bill of quantities entry (e.g., Regional Italian or Swiss CRB/NPK).
    /// </summary>
    public StandardType StandardType { get; private set; }

    /// <summary>
    /// Gets the specification and technical description of the construction work or material supplied.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the unit of measurement for estimating this item (e.g., "m2", "m3", "kg", "cad").
    /// </summary>
    public string UnitOfMeasure { get; private set; } = "m2";

    /// <summary>
    /// Gets the measured quantity of work or materials estimated for this item.
    /// </summary>
    public decimal Quantity { get; private set; }

    /// <summary>
    /// Gets the unit price allocated according to the applicable regional or Swiss construction tariff.
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// Gets the computed line subtotal, determined as quantity multiplied by unit price, rounded to two decimal places.
    /// </summary>
    public decimal Subtotal => decimal.Round(
        Quantity * UnitPrice,
        2,
        MidpointRounding.AwayFromZero);

    /// <summary>
    /// Creates a new bill of quantities line item for a construction project work package.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="siteId">The associated construction site identifier.</param>
    /// <param name="code">The standardized regional tariff or NPK catalog code.</param>
    /// <param name="standard">The tariff classification standard (e.g., Regional or Swiss NPK).</param>
    /// <param name="desc">The technical specification description of the work.</param>
    /// <param name="uom">The unit of measure.</param>
    /// <param name="qty">The planned engineering work quantity.</param>
    /// <param name="unitPrice">The contractual or tariff unit rate.</param>
    /// <returns>A newly created <see cref="BillOfQuantitiesItem"/> instance.</returns>
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
