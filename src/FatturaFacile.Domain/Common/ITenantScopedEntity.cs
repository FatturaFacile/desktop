// <copyright file="ITenantScopedEntity.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Defines a domain entity strictly scoped to a specific tenant.
/// Non-nullable TenantId guarantees data isolation.
/// </summary>
public interface ITenantScopedEntity : IBaseEntity
{
    /// <summary>
    /// Gets or sets the mandatory unique identifier of the owning tenant.
    /// </summary>
    public Guid TenantId { get; set; }
}
