// <copyright file="BomLine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities.Manufacturing;

/// <summary>
/// Represents bom line.
/// </summary>
public sealed class BomLine : Entity<Guid>
{

    /// <summary>
    /// Initializes a new instance of the <see cref="BomLine"/> class.
    /// </summary>
    /// <param name="bomId">The bom id.</param>
    /// <param name="componentItemId">The component item id.</param>
    /// <param name="quantity">The quantity.</param>
    /// <param name="scrapRate">The scrap rate.</param>
    internal BomLine(
        Guid    bomId,
        Guid    componentItemId,
        decimal quantity,
        decimal scrapRate)
    {
        Id                  = Guid.NewGuid();
        BillOfMaterialsId   = bomId;
        ComponentItemId     = componentItemId;
        QuantityPerUnit     = quantity;
        ScrapRatePercentage = scrapRate;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BomLine"/> class for entity framework or serialization.
    /// </summary>
    private BomLine()
    {
    }

    /// <summary>
    /// Gets the bill of materials id.
    /// </summary>
    public Guid BillOfMaterialsId { get; }

    /// <summary>
    /// Gets the component item id.
    /// </summary>
    public Guid ComponentItemId { get; }

    /// <summary>
    /// Gets the quantity per unit.
    /// </summary>
    public decimal QuantityPerUnit { get; }

    /// <summary>
    /// Gets the scrap rate percentage.
    /// </summary>
    public decimal ScrapRatePercentage { get; }

}
