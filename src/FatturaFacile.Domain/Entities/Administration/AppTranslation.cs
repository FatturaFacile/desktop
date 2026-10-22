// <copyright file="AppTranslation.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities.Administration;

/// <summary>
/// Represents a localized user interface text resource, supporting tenant-specific custom overrides over system-level default strings.
/// </summary>
public sealed class AppTranslation : Entity<Guid>
{

    /// <summary>
    /// Initializes a new instance of the <see cref="AppTranslation"/> class for entity framework or serialization.
    /// </summary>
    private AppTranslation()
    {
    }

    /// <summary>
    /// Gets the unique identifier of the tenant that customized this translation, or <see langword="null"/> when this record represents a global system default.
    /// </summary>
    public Guid? TenantId { get; private set; }

    /// <summary>
    /// Gets the ISO two-letter language or locale code (e.g., "it", "de", "fr", "en").
    /// </summary>
    public string Locale { get; private set; } = "it";

    /// <summary>
    /// Gets the unique hierarchical localization key identifying the UI text element (e.g., "Invoice.Headers.VatNumber").
    /// </summary>
    public string Key { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the default fallback translated text shipped with the system.
    /// </summary>
    public string DefaultValue { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the optional tenant-specific custom translation override text.
    /// </summary>
    public string? CustomValue { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp indicating when this translation entry was created or last updated.
    /// </summary>
    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Gets the effective translation text, returning <see cref="CustomValue"/> when non-blank or falling back to <see cref="DefaultValue"/>.
    /// </summary>
    public string EffectiveValue => !string.IsNullOrWhiteSpace(value: CustomValue) ? CustomValue : DefaultValue;

    /// <summary>
    /// Creates and initializes a new <see cref="AppTranslation"/> instance.
    /// </summary>
    /// <param name="locale">The ISO language or locale code (e.g., "it", "de", "fr", "en").</param>
    /// <param name="key">The hierarchical localization key identifying the UI string.</param>
    /// <param name="defaultValue">The system default translated text.</param>
    /// <param name="customValue">An optional custom translation override for the specified tenant.</param>
    /// <param name="tenantId">The optional tenant identifier scoping the custom translation override; <see langword="null"/> for global defaults.</param>
    /// <returns>A fully initialized <see cref="AppTranslation"/> entity.</returns>
    public static AppTranslation Create(
        string  locale,
        string  key,
        string  defaultValue,
        string? customValue = null,
        Guid?   tenantId    = null)
    {
        return new AppTranslation
               {
                   Id           = Guid.NewGuid(),
                   TenantId     = tenantId,
                   Locale       = locale?.ToLowerInvariant() ?? string.Empty,
                   Key          = key?.Trim()                ?? string.Empty,
                   DefaultValue = defaultValue,
                   CustomValue  = customValue,
                   UpdatedAtUtc = DateTime.UtcNow,
               };
    }

    /// <summary>
    /// Updates the custom translation text override and records the modification timestamp.
    /// </summary>
    /// <param name="customValue">The customized translation string override, or <see langword="null"/> to revert to the system default.</param>
    public void UpdateCustomValue(string? customValue)
    {
        CustomValue  = customValue;
        UpdatedAtUtc = DateTime.UtcNow;
    }

}
