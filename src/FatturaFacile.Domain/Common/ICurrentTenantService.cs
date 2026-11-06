// <copyright file="ICurrentTenantService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Provides access to and mutation of the currently active tenant context in the offline-first desktop application.
/// </summary>
public interface ICurrentTenantService
{
    /// <summary>
    /// Occurs when the active tenant context changes.
    /// </summary>
    public event EventHandler? CurrentTenantChanged;

    /// <summary>
    /// Gets the unique identifier of the currently active tenant.
    /// </summary>
    public Guid TenantId { get; }

    /// <summary>
    /// Gets the ISO country code of the active tenant (e.g., "CH", "IT").
    /// </summary>
    public string CountryCode { get; }

    /// <summary>
    /// Gets the display name of the active tenant company.
    /// </summary>
    public string TenantName { get; }

    /// <summary>
    /// Gets the default currency code of the active tenant (e.g., "CHF", "EUR").
    /// </summary>
    public string DefaultCurrency { get; }

    /// <summary>
    /// Sets the active tenant context for the desktop application session.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the tenant.</param>
    /// <param name="tenantName">The name of the tenant company.</param>
    /// <param name="countryCode">The ISO country code ("CH" or "IT").</param>
    /// <param name="defaultCurrency">The ISO currency code ("CHF" or "EUR").</param>
    public void SetCurrentTenant(Guid tenantId, string tenantName, string countryCode, string defaultCurrency);
}
