// <copyright file="ITenantEntity.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Interface implemented by any entity subject to multi-tenant isolation.
/// </summary>
public interface ITenantEntity
{
    /// <summary>
    /// Gets or sets the tenant identifier for multi-tenant isolation.
    /// </summary>
    public Guid TenantId { get; set; }
}
