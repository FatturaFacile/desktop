// <copyright file="Branch.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Administration;

/// <summary>
/// Represents branch.
/// </summary>
public sealed class Branch : Entity<Guid>, ITenantEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Branch"/> class.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="code">The code.</param>
    /// <param name="name">The name.</param>
    /// <param name="address">The address.</param>
    /// <param name="isHq">The is hq.</param>
    internal Branch(
        Guid tenantId,
        string code,
        string name,
        Address address,
        bool isHq)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        BranchCode = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Address = address;
        IsHeadquarters = isHq;
    }

    private Branch()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the branch code.
    /// </summary>
    public string BranchCode { get; } = string.Empty;

    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; } = string.Empty;

    /// <summary>
    /// Gets the address.
    /// </summary>
    public Address Address { get; } = null!;

    /// <summary>
    /// Gets a value indicating whether headquarters.
    /// </summary>
    public bool IsHeadquarters { get; }
}
