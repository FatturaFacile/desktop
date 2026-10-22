// <copyright file="Branch.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Administration;

/// <summary>
/// Represents an operational or legal office location (branch, headquarters, or warehouse) belonging to an organization tenant.
/// </summary>
public sealed class Branch : Entity<Guid>, ITenantEntity
{

    /// <summary>
    /// Initializes a new instance of the <see cref="Branch"/> class.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the owning tenant organization.</param>
    /// <param name="code">The unique organizational branch code (e.g., "HQ", "LUG-01").</param>
    /// <param name="name">The descriptive or display name of the branch office.</param>
    /// <param name="address">The physical postal address of the branch.</param>
    /// <param name="isHq">A flag indicating whether this branch operates as the primary corporate headquarters.</param>
    internal Branch(
        Guid    tenantId,
        string  code,
        string  name,
        Address address,
        bool    isHq)
    {
        Id             = Guid.NewGuid();
        TenantId       = tenantId;
        BranchCode     = code.Trim().ToUpperInvariant();
        Name           = name.Trim();
        Address        = address;
        IsHeadquarters = isHq;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Branch"/> class for entity framework or serialization.
    /// </summary>
    private Branch()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant organization that owns this branch location.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique internal branch code, normalized to uppercase.
    /// </summary>
    public string BranchCode { get; } = string.Empty;

    /// <summary>
    /// Gets the display name of the branch office.
    /// </summary>
    public string Name { get; } = string.Empty;

    /// <summary>
    /// Gets the physical street and postal address of the branch office.
    /// </summary>
    public Address Address { get; } = null!;

    /// <summary>
    /// Gets a value indicating whether this branch serves as the primary corporate headquarters for the tenant.
    /// </summary>
    public bool IsHeadquarters { get; }

}
