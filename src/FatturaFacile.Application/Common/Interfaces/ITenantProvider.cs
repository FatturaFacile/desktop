// <copyright file="ITenantProvider.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Provides access to the active multi-tenant context and isolation partition for the current request execution.
/// </summary>
public interface ITenantProvider
{
    /// <summary>
    /// Gets the unique identifier of the currently active tenant organization.
    /// </summary>
    public Guid CurrentTenantId { get; }

    /// <summary>
    /// Gets the unique alphanumeric organization code associated with the active tenant.
    /// </summary>
    public string CurrentTenantCode { get; }

    /// <summary>
    /// Gets a value indicating whether a valid tenant context has been resolved and established.
    /// </summary>
    public bool HasTenant { get; }

    /// <summary>
    /// Sets the active tenant organizational boundary for the current execution scope.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the target tenant organization.</param>
    /// <param name="tenantCode">The unique alphanumeric identifier code of the target tenant.</param>
    public void SetTenant(
        Guid tenantId,
        string tenantCode);
}
