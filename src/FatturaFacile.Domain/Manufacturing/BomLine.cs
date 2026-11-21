// <copyright file="BomLine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Manufacturing;

/// <summary>
/// A BOM component line.
/// </summary>
public class BomLine : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the local identifier (equals the server identifier for pulled records).
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the local tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the owning BOM.
    /// </summary>
    public Guid BomHeaderId { get; set; }

    /// <summary>
    /// Gets or sets the component item identifier.
    /// </summary>
    public Guid ComponentItemId { get; set; }

    /// <summary>
    /// Gets or sets the component item code.
    /// </summary>
    public string ComponentItemCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the line sequence.
    /// </summary>
    public int Sequence { get; set; }

    /// <summary>
    /// Gets or sets the quantity per BOM base quantity.
    /// </summary>
    public decimal QuantityPer { get; set; }

    /// <summary>
    /// Gets or sets the scrap allowance percent.
    /// </summary>
    public decimal ScrapPercentage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the line is a phantom (exploded through, not stocked).
    /// </summary>
    public bool IsPhantom { get; set; }

    /// <summary>
    /// Gets or sets the standard unit cost of the component (0 when unknown).
    /// </summary>
    public decimal StandardUnitCost { get; set; }

    /// <summary>
    /// Gets or sets the creation instant (UTC).
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last local modification instant (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
