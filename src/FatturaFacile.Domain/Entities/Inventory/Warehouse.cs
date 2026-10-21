// <copyright file="Warehouse.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Inventory;

/// <summary>
/// Represents warehouse.
/// </summary>
public sealed class Warehouse : Entity<Guid>, ITenantEntity
{
    private Warehouse()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the code.
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the address.
    /// </summary>
    public Address Address { get; private set; } = null!;

    /// <summary>
    /// Gets the total capacity square meters.
    /// </summary>
    public decimal TotalCapacitySquareMeters { get; private set; }

    /// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="code">The code.</param>
    /// <param name="name">The name.</param>
    /// <param name="address">The address.</param>
    /// <param name="capacity">The capacity.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
