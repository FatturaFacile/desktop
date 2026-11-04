// <copyright file="IMustHaveTenant.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Shared.Abstractions;

/// <summary>
/// Defines a contract for entities that strictly belong to a specific tenant.
/// Access and mutations are automatically constrained to this tenant identifier.
/// </summary>
public interface IMustHaveTenant
{
    /// <summary>
    /// Gets the unique identifier of the tenant owning this entity.
    /// </summary>
    public Guid TenantId { get; }
}
