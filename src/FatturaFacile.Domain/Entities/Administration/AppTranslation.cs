using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities.Administration;

/// <summary>
/// Represents app translation.
/// </summary>
public sealed class AppTranslation : Entity<Guid>
{
    /// <summary>
    /// Gets the null for system defaults.
    /// </summary>
    public Guid? TenantId { get; private set; }

/// <summary>
    /// Gets the &amp;quot;it&amp;quot;, &amp;quot;de&amp;quot;, &amp;quot;fr&amp;quot;, &amp;quot;en&amp;quot;.
    /// </summary>
    public string Locale { get; private set; } = "it";

/// <summary>
    /// Gets the key.
    /// </summary>
    public string Key { get; private set; } = string.Empty;

/// <summary>
    /// Gets the default value.
    /// </summary>
    public string DefaultValue { get; private set; } = string.Empty;

/// <summary>
    /// Gets the custom value.
    /// </summary>
    public string? CustomValue { get; private set; }

/// <summary>
    /// Gets the updated at utc.
    /// </summary>
    public DateTime UpdatedAtUtc { get; private set; }

/// <summary>
    /// Gets the effective value.
    /// </summary>
    public string EffectiveValue => !string.IsNullOrWhiteSpace(CustomValue) ? CustomValue : DefaultValue;

private AppTranslation()
    {
    }

/// <summary>
    /// Create.
    /// </summary>
    /// <param name="locale">The locale.</param>
    /// <param name="key">The key.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="customValue">The custom value.</param>
    /// <param name="tenantId">The tenant id.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static AppTranslation Create(string locale, string key, string defaultValue, string? customValue = null, Guid? tenantId = null)
    {
        return new AppTranslation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Locale = locale.ToLowerInvariant(),
            Key = key.Trim(),
            DefaultValue = defaultValue,
            CustomValue = customValue,
            UpdatedAtUtc = DateTime.UtcNow,
        };
    }

/// <summary>
    /// Update Custom Value.
    /// </summary>
    /// <param name="customValue">The custom value.</param>
    public void UpdateCustomValue(string? customValue)
    {
        CustomValue = customValue;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
