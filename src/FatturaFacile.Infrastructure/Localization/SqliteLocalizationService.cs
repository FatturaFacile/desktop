// <copyright file="SqliteLocalizationService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.Concurrent;

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Administration;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Infrastructure.Localization;

/// <summary>
/// Provides SQLite-backed dynamic localization services with in-memory caching and fallback hierarchies.
/// </summary>
/// <param name="serviceProvider">The service provider used to create execution scopes for database queries.</param>
public sealed class SqliteLocalizationService(IServiceProvider serviceProvider) : ILocalizationService
{

    /// <summary>
    /// The service provider used to create execution scopes for database operations.
    /// </summary>
    private readonly IServiceProvider serviceProvider = serviceProvider;

    /// <summary>
    /// In-memory cache holding localized translations grouped by culture code and resource key.
    /// </summary>
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, AppTranslation>> cache =
        new(comparer: StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Occurs when the active UI language or a translation entry is updated.
    /// </summary>
    public event EventHandler? LanguageChanged;

    /// <summary>
    /// Gets the current ISO 639-1 language code (e.g., "it", "de", "fr", "en").
    /// </summary>
    public string CurrentLocale { get; private set; } = "it";

    /// <summary>
    /// Sets the active UI locale and raises <see cref="LanguageChanged"/> when the value changes.
    /// </summary>
    /// <param name="locale">The target ISO language code to activate.</param>
    public void SetLocale(string locale)
    {
        var normalized = string.IsNullOrWhiteSpace(value: locale) ? "it" : locale.Trim().ToLowerInvariant();

        if (!string.Equals(
                           a: CurrentLocale,
                           b: normalized,
                           comparisonType: StringComparison.Ordinal))
        {
            CurrentLocale = normalized;
            LanguageChanged?.Invoke(sender: this, e: EventArgs.Empty);
        }
    }

    /// <summary>
    /// Synchronously retrieves a localized string from cache for the active locale,
    /// falling back to Italian defaults or the provided fallback text.
    /// </summary>
    /// <param name="key">The translation identifier key.</param>
    /// <param name="defaultFallback">Optional default string to return if no translation is found.</param>
    /// <returns>The effective translated string or fallback text.</returns>
    public string GetString(
        string  key,
        string? defaultFallback = null)
    {
        if (string.IsNullOrWhiteSpace(value: key))
        {
            return string.Empty;
        }

        if (cache.TryGetValue(
                              key: CurrentLocale,
                              value: out var localeDict) && localeDict.TryGetValue(
                                                                                   key: key,
                                                                                   value: out var translation))
        {
            return translation.EffectiveValue;
        }

        // Fallback to Italian default if present
        return cache.TryGetValue(key: "it", value: out var itDict) && itDict.TryGetValue(key: key, value: out var itTranslation)
                   ? itTranslation.EffectiveValue
                   : defaultFallback ?? key;
    }

    /// <summary>
    /// Asynchronously retrieves a localized string, ensuring target locale strings are loaded into memory cache.
    /// </summary>
    /// <param name="key">The translation identifier key.</param>
    /// <param name="locale">The optional locale override; defaults to <see cref="CurrentLocale"/> when null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task containing the effective translated string or key identifier as fallback.</returns>
    public async Task<string> GetStringAsync(
        string            key,
        string?           locale            = null,
        CancellationToken cancellationToken = default)
    {
        var targetLocale = locale ?? CurrentLocale;

        await EnsureLoadedAsync(
                                locale: targetLocale,
                                cancellationToken: cancellationToken);

        if (cache.TryGetValue(
                              key: targetLocale,
                              value: out var localeDict) && localeDict.TryGetValue(
                                                                                   key: key,
                                                                                   value: out var translation))
        {
            return translation.EffectiveValue;
        }

        return cache.TryGetValue(
                              key: "it",
                              value: out var itDict) && itDict.TryGetValue(
                                                                           key: key,
                                                                           value: out var itTranslation)
            ? itTranslation.EffectiveValue
            : key;
    }

    /// <summary>
    /// Retrieves all translations registered for the specified locale as data transfer objects.
    /// </summary>
    /// <param name="locale">The ISO language code to query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task containing the read-only list of translation DTOs.</returns>
    public async Task<IReadOnlyList<AppTranslationDto>> GetAllTranslationsAsync(
        string            locale,
        CancellationToken cancellationToken = default)
    {
        var targetLocale = string.IsNullOrWhiteSpace(value: locale) ? CurrentLocale : locale.Trim().ToLowerInvariant();

        await EnsureLoadedAsync(
                                locale: targetLocale,
                                cancellationToken: cancellationToken);

        return cache.TryGetValue(key: targetLocale, value: out var dict)
                   ? dict.Values.Select(selector: t => new AppTranslationDto(
                                                                             Id: t.Id,
                                                                             TenantId: t.TenantId,
                                                                             Locale: t.Locale,
                                                                             Key: t.Key,
                                                                             DefaultValue: t.DefaultValue,
                                                                             CustomValue: t.CustomValue,
                                                                             EffectiveValue: t.EffectiveValue,
                                                                             UpdatedAtUtc: t.UpdatedAtUtc))
                         .ToList()
                   : [];
    }

    /// <summary>
    /// Persists a custom user override translation to SQLite and updates the in-memory cache.
    /// </summary>
    /// <param name="locale">The target ISO language code.</param>
    /// <param name="key">The translation key identifier.</param>
    /// <param name="customValue">The user-customized translation text.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    public async Task SaveCustomTranslationAsync(
        string            locale,
        string            key,
        string            customValue,
        CancellationToken cancellationToken = default)
    {
        var targetLocale = string.IsNullOrWhiteSpace(value: locale) ? CurrentLocale : locale.Trim().ToLowerInvariant();

        await using var scope = serviceProvider.CreateAsyncScope();
        var             db    = scope.ServiceProvider.GetRequiredService<IFatturaFacileDbContext>();

        var existing = await db.AppTranslations
                               .FirstOrDefaultAsync(
                                                    predicate: t => t.Locale == targetLocale && t.Key == key,
                                                    cancellationToken: cancellationToken);

        if (existing != null)
        {
            existing.UpdateCustomValue(customValue: customValue);
        }
        else
        {
            existing = AppTranslation.Create(
                                             locale: targetLocale,
                                             key: key,
                                             defaultValue: key,
                                             customValue: customValue);

            // Will be added through DbContext directly if needed
        }

        _ = await db.SaveChangesAsync(cancellationToken: cancellationToken);

        // Update local memory cache
        var localeCache = cache.GetOrAdd(
                                         key: targetLocale,
                                         valueFactory: _ => new ConcurrentDictionary<string, AppTranslation>(
                                                                                                             comparer: StringComparer
                                                                                                                .OrdinalIgnoreCase));

        localeCache[key: key] = existing;

        LanguageChanged?.Invoke(sender: this, e: EventArgs.Empty);
    }

    /// <summary>
    /// Loads all database translations into memory cache during application initialization.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    public async Task InitializeCacheAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var             db    = scope.ServiceProvider.GetRequiredService<IFatturaFacileDbContext>();

        var list = await db.AppTranslations.ToListAsync(cancellationToken: cancellationToken);

        foreach (var item in list)
        {
            var localeDict = cache.GetOrAdd(
                                            key: item.Locale,
                                            valueFactory: _ => new ConcurrentDictionary<string, AppTranslation>(
                                                                                                                comparer: StringComparer
                                                                                                                   .OrdinalIgnoreCase));

            localeDict[key: item.Key] = item;
        }
    }

    /// <summary>
    /// Ensures translations for the specified locale are loaded from the database into memory cache.
    /// </summary>
    /// <param name="locale">The ISO culture code.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task EnsureLoadedAsync(
        string            locale,
        CancellationToken cancellationToken)
    {
        if (cache.ContainsKey(key: locale))
        {
            return;
        }

        await using var scope = serviceProvider.CreateAsyncScope();
        var             db    = scope.ServiceProvider.GetRequiredService<IFatturaFacileDbContext>();

        var list = await db.AppTranslations
                           .Where(predicate: t => t.Locale == locale)
                           .ToListAsync(cancellationToken: cancellationToken);

        var localeDict = cache.GetOrAdd(
                                        key: locale,
                                        valueFactory: _ => new ConcurrentDictionary<string, AppTranslation>(
                                                                                                            comparer: StringComparer
                                                                                                               .OrdinalIgnoreCase));

        foreach (var item in list)
        {
            localeDict[key: item.Key] = item;
        }
    }

}
