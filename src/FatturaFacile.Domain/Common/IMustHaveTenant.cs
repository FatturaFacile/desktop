// <copyright file="IMustHaveTenant.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Enforces tenant ownership on multi-tenant domain entities.
/// </summary>
public interface IMustHaveTenant
{
    /// <summary>
    /// Gets or sets the unique tenant organization identifier that owns this entity record.
    /// </summary>
    Guid TenantId { get; set; }
}
