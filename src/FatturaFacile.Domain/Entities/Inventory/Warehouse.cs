// <copyright file="Warehouse.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Inventory;

/// <summary>
/// Represents a physical storage facility, logistics hub, or shop depository (magazzino / deposito merci),
/// maintaining facility identification, geographic location, and floor area capacity.
/// </summary>
public sealed class Warehouse : Entity<Guid>, ITenantEntity
{
    private Warehouse()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant organization owning or operating the storage facility.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique internal warehouse identifier code (e.g., "WH-MAIN", "DEP-01").
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the descriptive name or label of the warehouse facility.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the physical geographic and postal address of the warehouse site.
    /// </summary>
    public Address Address { get; private set; } = null!;

    /// <summary>
    /// Gets the total operational floor storage capacity expressed in square meters.
    /// </summary>
    public decimal TotalCapacitySquareMeters { get; private set; }

    /// <summary>
    /// Creates a new physical warehouse facility record.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="code">The warehouse facility identification code.</param>
    /// <param name="name">The facility name or description.</param>
    /// <param name="address">The physical location address.</param>
    /// <param name="capacity">The usable storage area in square meters.</param>
    /// <returns>A newly created <see cref="Warehouse"/> entity.</returns>
    public static Warehouse Create(
        Guid tenantId,
        string code,
        string name,
        Address address,
        decimal capacity)
    {
        return new Warehouse
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = code?.Trim().ToUpperInvariant() ?? string.Empty,
            Name = name?.Trim() ?? string.Empty,
            Address = address,
            TotalCapacitySquareMeters = capacity,
        };
    }
}
