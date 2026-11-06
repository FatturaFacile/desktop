// <copyright file="Warehouse.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents a storage depot, distribution center, or physical warehouse.
/// </summary>
public class Warehouse : IOptionalTenantEntity
{

    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the optional owning tenant identifier. If null, warehouse is shared.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the alphanumeric unique warehouse code (e.g. "WH-MAIN", "DEP-01").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the descriptive name of the facility.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the physical address location.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the primary shipping facility.
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Gets or sets the facility category ("standard", "bonded", "retail").
    /// </summary>
    public string WarehouseType { get; set; } = "standard";

    /// <summary>
    /// Gets or sets a value indicating whether the facility is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the soft delete timestamp.
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
