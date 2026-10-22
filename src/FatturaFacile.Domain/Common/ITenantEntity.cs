// <copyright file="ITenantEntity.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Defines multi-tenant data isolation constraints, binding an entity to a specific tenant partition.
/// </summary>
public interface ITenantEntity
{

    /// <summary>
    /// Gets or sets the foreign identifier of the tenant partition owning this entity record.
    /// </summary>
    public Guid TenantId { get; set; }

}
