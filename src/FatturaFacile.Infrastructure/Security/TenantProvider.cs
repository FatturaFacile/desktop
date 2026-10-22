// <copyright file="TenantProvider.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;

namespace FatturaFacile.Infrastructure.Security;

/// <summary>
/// Represents tenant provider.
/// </summary>
public sealed class TenantProvider : ITenantProvider
{

    /// <summary>
    /// Gets the current tenant id.
    /// </summary>
    public Guid CurrentTenantId { get; private set; } = Guid.Empty;

    /// <summary>
    /// Gets the current tenant code.
    /// </summary>
    public string CurrentTenantCode { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether tenant.
    /// </summary>
    public bool HasTenant => CurrentTenantId != Guid.Empty;

    /// <summary>
    /// Set Tenant.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="tenantCode">The tenant code.</param>
    public void SetTenant(
        Guid   tenantId,
        string tenantCode)
    {
        CurrentTenantId   = tenantId;
        CurrentTenantCode = tenantCode;
    }

}
