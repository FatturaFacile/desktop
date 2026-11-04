// <copyright file="TenantContext.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Infrastructure.Common.Tenancy;

/// <summary>
/// Default thread-safe implementation of <see cref="ITenantContext"/>.
/// </summary>
public sealed class TenantContext : ITenantContext
{
    private readonly AsyncLocal<Guid> _tenantId = new();
    private readonly AsyncLocal<string?> _userId = new();
    private readonly AsyncLocal<bool> _isTenantFilterIgnored = new();

    /// <inheritdoc />
    public Guid TenantId => _tenantId.Value;

    /// <inheritdoc />
    public string? UserId => _userId.Value;

    /// <inheritdoc />
    public bool IsTenantFilterIgnored => _isTenantFilterIgnored.Value;

    /// <inheritdoc />
    public void SetTenant(Guid tenantId, string? userId = null)
    {
        _tenantId.Value = tenantId;
        _userId.Value = userId;
    }

    /// <inheritdoc />
    public IDisposable IgnoreTenantFilters()
    {
        var previousState = _isTenantFilterIgnored.Value;
        _isTenantFilterIgnored.Value = true;
        return new TenantFilterScope(() => _isTenantFilterIgnored.Value = previousState);
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
