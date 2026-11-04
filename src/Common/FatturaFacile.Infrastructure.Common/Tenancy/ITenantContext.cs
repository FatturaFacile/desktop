// <copyright file="ITenantContext.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Infrastructure.Common.Tenancy;

/// <summary>
/// Provides tenancy information, isolation controls, and filter suppression scopes.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Gets the active tenant identifier.
    /// </summary>
    public Guid TenantId { get; }

    /// <summary>
    /// Gets the optional user identifier executing in this context.
    /// </summary>
    public string? UserId { get; }

    /// <summary>
    /// Gets a value indicating whether global tenant query filters should be ignored (e.g. for administrative background tasks).
    /// </summary>
    public bool IsTenantFilterIgnored { get; }

    /// <summary>
    /// Sets the active tenant and optional user identifier.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the active tenant.</param>
    /// <param name="userId">The optional identifier of the current user.</param>
    public void SetTenant(Guid tenantId, string? userId = null);

    /// <summary>
    /// Creates an <see cref="IDisposable"/> scope that temporarily bypasses multi-tenant query filters.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> token restoring tenant isolation upon disposal.</returns>
    public IDisposable IgnoreTenantFilters();
}
