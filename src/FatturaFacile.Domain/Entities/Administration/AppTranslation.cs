using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities.Administration;

public sealed class AppTranslation : Entity<Guid>
{
    /// <summary>
    /// Null for system defaults
    /// </summary>
    public Guid? TenantId { get; private set; }

    /// <summary>
    /// &quot;it&quot;, &quot;de&quot;, &quot;fr&quot;, &quot;en&quot;
    /// </summary>
    public string Locale { get; private set; } = "it";
    public string Key { get; private set; } = string.Empty;
    public string DefaultValue { get; private set; } = string.Empty;
    public string? CustomValue { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public string EffectiveValue => !string.IsNullOrWhiteSpace(CustomValue) ? CustomValue : DefaultValue;

    private AppTranslation()
    {
    }

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

    public void UpdateCustomValue(string? customValue)
    {
        CustomValue = customValue;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}