using FatturaFacile.Application.Shared.Context;

namespace FatturaFacile.Desktop.Wpf.Services;

/// <summary>
/// Thread-safe in-memory desktop user session context.
/// </summary>
public sealed class DesktopUserContext : ICurrentUserContext
{
    public Guid? UserId { get; private set; }
    public Guid? TenantId { get; private set; }
    public string Culture { get; private set; } = "en";

    /// <summary>
    /// Sets authenticated user credentials and preferences.
    /// </summary>
    public void SetUser(Guid userId, Guid tenantId, string culture)
    {
        UserId = userId;
        TenantId = tenantId;
        Culture = string.IsNullOrWhiteSpace(culture) ? "en" : culture;
    }

    /// <summary>
    /// Updates the active UI culture code.
    /// </summary>
    public void SetCulture(string culture)
    {
        if (!string.IsNullOrWhiteSpace(culture))
        {
            Culture = culture;
        }
    }

    /// <summary>
    /// Clears the user session upon logout.
    /// </summary>
    public void Clear()
    {
        UserId = null;
        TenantId = null;
        Culture = "en";
    }
}
