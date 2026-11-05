// <copyright file="IOptionalTenantEntity.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Defines a domain entity that supports global/shared system defaults when TenantId is null,
/// or tenant-isolated records when TenantId is assigned.
/// </summary>
public interface IOptionalTenantEntity : IBaseEntity
{
    /// <summary>
    /// Gets or sets the optional identifier of the owning tenant.
    /// If null, the entity is a global system default shared across all tenants.
    /// </summary>
    public Guid? TenantId { get; set; }
}
