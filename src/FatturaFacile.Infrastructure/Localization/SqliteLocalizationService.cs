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
/// Represents sqlite localization service.
/// </summary>
/// <param name="serviceProvider">The service provider.</param>
public sealed class SqliteLocalizationService(IServiceProvider serviceProvider) : ILocalizationService
{
    private readonly IServiceProvider serviceProvider = serviceProvider;

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, AppTranslation>> cache =
        new(StringComparer.OrdinalIgnoreCase);

    public event EventHandler? LanguageChanged;

    /// <summary>
    /// Gets the current locale.
    /// </summary>
    public string CurrentLocale { get; private set; } = "it";

    /// <summary>
    /// Set Locale.
    /// </summary>
    /// <param name="locale">The locale.</param>
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
    /// Get String.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="defaultFallback">The default fallback.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
    /// Get String Async.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="locale">The locale.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
    /// Get All Translations Async.
    /// </summary>
    /// <param name="locale">The locale.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
    /// Save Custom Translation Async.
    /// </summary>
    /// <param name="locale">The locale.</param>
    /// <param name="key">The key.</param>
    /// <param name="customValue">The custom value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
    /// Initialize Cache Async.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
