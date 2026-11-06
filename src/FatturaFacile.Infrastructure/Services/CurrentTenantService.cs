// <copyright file="CurrentTenantService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Infrastructure.Services;

/// <summary>
/// In-memory session provider managing the active tenant context for offline desktop operation.
/// </summary>
public sealed class CurrentTenantService : ICurrentTenantService
{
    /// <inheritdoc/>
    public event EventHandler? CurrentTenantChanged;

    /// <inheritdoc/>
    public Guid TenantId { get; private set; } = new Guid(0x11111111, 0x1111, 0x1111, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11);

    /// <inheritdoc/>
    public string CountryCode { get; private set; } = "IT";

    /// <inheritdoc/>
    public string TenantName { get; private set; } = "FatturaFacile Italia S.r.l.";

    /// <inheritdoc/>
    public string DefaultCurrency { get; private set; } = "EUR";

    /// <inheritdoc/>
    public void SetCurrentTenant(Guid tenantId, string tenantName, string countryCode, string defaultCurrency)
    {
        TenantId = tenantId;
        TenantName = tenantName;
        CountryCode = countryCode;
        DefaultCurrency = defaultCurrency;
        CurrentTenantChanged?.Invoke(this, EventArgs.Empty);
    }
}
