// <copyright file="DesktopTenantContext.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Infrastructure.Common.Tenancy;

namespace FatturaFacile.Desktop.Wpf.Services;

/// <summary>
/// Thread-safe in-memory desktop tenant session context with query filter suppression support.
/// </summary>
internal sealed class DesktopTenantContext : ITenantContext
{
    /// <inheritdoc />
    public Guid TenantId { get; private set; }

    /// <inheritdoc />
    public string? UserId { get; private set; }

    /// <inheritdoc />
    public bool IsTenantFilterIgnored { get; private set; }

    /// <inheritdoc />
    public void SetTenant(Guid tenantId, string? userId = null)
    {
        TenantId = tenantId;
        UserId = userId;
    }

    /// <inheritdoc />
    public IDisposable IgnoreTenantFilters()
    {
        var previousState = IsTenantFilterIgnored;
        IsTenantFilterIgnored = true;
        return new TenantFilterScope(() => IsTenantFilterIgnored = previousState);
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
