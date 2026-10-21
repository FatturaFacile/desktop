// <copyright file="ITenantProvider.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Defines the contract for tenant provider.
/// </summary>
public interface ITenantProvider
{
    /// <summary>
    /// Gets the current tenant id.
    /// </summary>
    public Guid CurrentTenantId { get; }

    /// <summary>
    /// Gets the current tenant code.
    /// </summary>
    public string CurrentTenantCode { get; }

    /// <summary>
    /// Gets a value indicating whether tenant.
    /// </summary>
    public bool HasTenant { get; }

    /// <summary>
    /// Set Tenant.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="tenantCode">The tenant code.</param>
    public void SetTenant(
        Guid tenantId,
        string tenantCode);
}
