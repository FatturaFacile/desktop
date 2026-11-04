// <copyright file="DesktopTenantContext.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Infrastructure.Common.Tenancy;

namespace FatturaFacile.Desktop.Wpf.Services;

/// <summary>
/// Thread-safe in-memory desktop tenant session context with query filter suppression support.
/// </summary>
public sealed class DesktopTenantContext : ITenantContext
{
    private Guid _tenantId;
    private string? _userId;
    private bool _isTenantFilterIgnored;

    /// <inheritdoc />
    public Guid TenantId => _tenantId;

    /// <inheritdoc />
    public string? UserId => _userId;

    /// <inheritdoc />
    public bool IsTenantFilterIgnored => _isTenantFilterIgnored;

    /// <inheritdoc />
    public void SetTenant(Guid tenantId, string? userId = null)
    {
        _tenantId = tenantId;
        _userId = userId;
    }

    /// <inheritdoc />
    public IDisposable IgnoreTenantFilters()
    {
        var previousState = _isTenantFilterIgnored;
        _isTenantFilterIgnored = true;
        return new TenantFilterScope(() => _isTenantFilterIgnored = previousState);
    }

    private sealed class TenantFilterScope(Action onDispose) : IDisposable
    {
        private Action? _onDispose = onDispose;

        public void Dispose()
        {
            var action = Interlocked.Exchange(ref _onDispose, value: null);
            action?.Invoke();
        }
    }
}
