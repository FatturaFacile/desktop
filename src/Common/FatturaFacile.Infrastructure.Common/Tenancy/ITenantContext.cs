using FatturaFacile.Domain.Shared.Errors;

namespace FatturaFacile.Infrastructure.Common.Tenancy;

/// <summary>
/// Provides tenancy information, isolation controls, and filter suppression scopes.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Gets the active tenant identifier.
    /// </summary>
    Guid TenantId { get; }

    /// <summary>
    /// Gets the optional user identifier executing in this context.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets a value indicating whether global tenant query filters should be ignored (e.g. for administrative background tasks).
    /// </summary>
    bool IsTenantFilterIgnored { get; }

    /// <summary>
    /// Sets the active tenant and optional user identifier.
    /// </summary>
    void SetTenant(Guid tenantId, string? userId = null);

    /// <summary>
    /// Creates an <see cref="IDisposable"/> scope that temporarily bypasses multi-tenant query filters.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> token restoring tenant isolation upon disposal.</returns>
    IDisposable IgnoreTenantFilters();
}

/// <summary>
/// Default thread-safe implementation of <see cref="ITenantContext"/>.
/// </summary>
public sealed class TenantContext : ITenantContext
{
    private readonly AsyncLocal<Guid> _tenantId = new();
    private readonly AsyncLocal<string?> _userId = new();
    private readonly AsyncLocal<bool> _isTenantFilterIgnored = new();

    public Guid TenantId => _tenantId.Value;
    public string? UserId => _userId.Value;
    public bool IsTenantFilterIgnored => _isTenantFilterIgnored.Value;

    public void SetTenant(Guid tenantId, string? userId = null)
    {
        _tenantId.Value = tenantId;
        _userId.Value = userId;
    }

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
            var action = Interlocked.Exchange(ref _onDispose, null);
            action?.Invoke();
        }
    }
}
