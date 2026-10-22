// <copyright file="TenantProvider.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;

namespace FatturaFacile.Infrastructure.Security;

/// <summary>
/// Provides and tracks the active tenant context for multi-tenant data partitioning in the desktop client.
/// </summary>
public sealed class TenantProvider : ITenantProvider
{

    /// <summary>
    /// Gets the unique identifier of the currently active tenant, or <see cref="Guid.Empty"/> if none selected.
    /// </summary>
    public Guid CurrentTenantId { get; private set; } = Guid.Empty;

    /// <summary>
    /// Gets the alphanumeric tenant code (e.g., "TENANT-IT", "TENANT-CH") of the currently selected tenant.
    /// </summary>
    public string CurrentTenantCode { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether a valid tenant context is currently active.
    /// </summary>
    public bool HasTenant => CurrentTenantId != Guid.Empty;

    /// <summary>
    /// Sets the active tenant context for multi-tenant querying and entity partitioning.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the tenant.</param>
    /// <param name="tenantCode">The business code identifying the tenant.</param>
    public void SetTenant(
        Guid   tenantId,
        string tenantCode)
    {
        CurrentTenantId   = tenantId;
        CurrentTenantCode = tenantCode;
    }

}
