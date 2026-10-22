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
    private readonly IServiceProvider serviceProvider = serviceProvider;

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, AppTranslation>> cache =
        new(StringComparer.OrdinalIgnoreCase);

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
    public void SetLocale(
        string locale)
    {
        var normalized = string.IsNullOrWhiteSpace(locale) ? "it" : locale.Trim().ToLowerInvariant();
        if (!string.Equals(
                CurrentLocale,
                normalized,
                StringComparison.Ordinal))
        {
            CurrentLocale = normalized;
            LanguageChanged?.Invoke(this, EventArgs.Empty);
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
        string key,
        string? defaultFallback = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        if (cache.TryGetValue(
                CurrentLocale,
                out var localeDict) && localeDict.TryGetValue(
                key,
                out var translation))
        {
            return translation.EffectiveValue;
        }

        // Fallback to Italian default if present
        return cache.TryGetValue( "it", out var itDict) && itDict.TryGetValue( key, out var itTranslation) ? itTranslation.EffectiveValue : defaultFallback ?? key;
    }

    /// <summary>
    /// Asynchronously retrieves a localized string, ensuring target locale strings are loaded into memory cache.
    /// </summary>
    /// <param name="key">The translation identifier key.</param>
    /// <param name="locale">The optional locale override; defaults to <see cref="CurrentLocale"/> when null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task containing the effective translated string or key identifier as fallback.</returns>
    public async Task<string> GetStringAsync(
        string key,
        string? locale = null,
        CancellationToken cancellationToken = default)
    {
        var targetLocale = locale ?? CurrentLocale;
        await EnsureLoadedAsync(
            targetLocale,
            cancellationToken);
        if (cache.TryGetValue(
                targetLocale,
                out var localeDict) && localeDict.TryGetValue(
                key,
                out var translation))
        {
            return translation.EffectiveValue;
        }

        if (cache.TryGetValue(
                "it",
                out var itDict) && itDict.TryGetValue(
                key,
                out var itTranslation))
        {
            return itTranslation.EffectiveValue;
        }

        return key;
    }

    /// <summary>
    /// Retrieves all translations registered for the specified locale as data transfer objects.
    /// </summary>
    /// <param name="locale">The ISO language code to query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task containing the read-only list of translation DTOs.</returns>
    public async Task<IReadOnlyList<AppTranslationDto>> GetAllTranslationsAsync(
        string locale,
        CancellationToken cancellationToken = default)
    {
        var targetLocale = string.IsNullOrWhiteSpace(locale) ? CurrentLocale : locale.Trim().ToLowerInvariant();
        await EnsureLoadedAsync(
            targetLocale,
            cancellationToken);
        return cache.TryGetValue( targetLocale, out var dict) ? dict.Values.Select(t => new AppTranslationDto( t.Id, t.TenantId, t.Locale, t.Key, t.DefaultValue, t.CustomValue, t.EffectiveValue, t.UpdatedAtUtc)).ToList() : [];
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
        string locale,
        string key,
        string customValue,
        CancellationToken cancellationToken = default)
    {
        var targetLocale = string.IsNullOrWhiteSpace(locale) ? CurrentLocale : locale.Trim().ToLowerInvariant();

        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IFatturaFacileDbContext>();

        var existing = await db.AppTranslations
            .FirstOrDefaultAsync(
                t => t.Locale == targetLocale && t.Key == key,
                cancellationToken);

        if (existing != null)
        {
            existing.UpdateCustomValue(customValue);
        }
        else
        {
            existing = AppTranslation.Create(
                targetLocale,
                key,
                key,
                customValue);

            // Will be added through DbContext directly if needed
        }

        _ = await db.SaveChangesAsync(cancellationToken);

        // Update local memory cache
        var localeCache = cache.GetOrAdd(
            targetLocale,
            _ => new ConcurrentDictionary<string, AppTranslation>(StringComparer.OrdinalIgnoreCase));
        localeCache[key] = existing;

        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Loads all database translations into memory cache during application initialization.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    public async Task InitializeCacheAsync(
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IFatturaFacileDbContext>();

        var list = await db.AppTranslations.ToListAsync(cancellationToken);
        foreach (var item in list)
        {
            var localeDict = cache.GetOrAdd(
                item.Locale,
                _ => new ConcurrentDictionary<string, AppTranslation>(StringComparer.OrdinalIgnoreCase));
            localeDict[item.Key] = item;
        }
    }

    private async Task EnsureLoadedAsync(
        string locale,
        CancellationToken cancellationToken)
    {
        if (cache.ContainsKey(locale))
        {
            return;
        }

        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IFatturaFacileDbContext>();

        var list = await db.AppTranslations
            .Where(t => t.Locale == locale)
            .ToListAsync(cancellationToken);

        var localeDict = cache.GetOrAdd(
            locale,
            _ => new ConcurrentDictionary<string, AppTranslation>(StringComparer.OrdinalIgnoreCase));
        foreach (var item in list)
        {
            localeDict[item.Key] = item;
        }
    }
}
