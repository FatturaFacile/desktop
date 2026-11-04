// <copyright file="IMayHaveTenant.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Shared.Abstractions;

/// <summary>
/// Defines a contract for optional or host-level entities that may or may not be tied to a tenant.
/// </summary>
public interface IMayHaveTenant
{
    /// <summary>
    /// Gets the optional tenant identifier, or null if the entity belongs to system/host scope.
    /// </summary>
    public Guid? TenantId { get; }
}
