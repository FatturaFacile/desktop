using System.Collections.Concurrent;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Administration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Infrastructure.Localization;

public sealed class SqliteLocalizationService(IServiceProvider serviceProvider) : ILocalizationService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, AppTranslation>> _cache = new(StringComparer.OrdinalIgnoreCase);

    public string CurrentLocale { get; private set; } = "it";

    public event Action? LanguageChanged;

    public void SetLocale(string locale)
    {
        var normalized = string.IsNullOrWhiteSpace(locale) ? "it" : locale.Trim().ToLowerInvariant();
        if (!string.Equals(CurrentLocale, normalized, StringComparison.Ordinal))
        {
            CurrentLocale = normalized;
            LanguageChanged?.Invoke();
        }
    }

    public string GetString(string key, string? defaultFallback = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        if (_cache.TryGetValue(CurrentLocale, out var localeDict) && localeDict.TryGetValue(key, out var translation))
        {
            return translation.EffectiveValue;
        }

        // Fallback to Italian default if present
        return _cache.TryGetValue("it", out var itDict) && itDict.TryGetValue(key, out var itTranslation)
            ? itTranslation.EffectiveValue
            : defaultFallback ?? key;
    }

    public async Task<string> GetStringAsync(string key, string? locale = null, CancellationToken cancellationToken = default)
    {
        var targetLocale = locale ?? CurrentLocale;
        await EnsureLoadedAsync(targetLocale, cancellationToken);
        return await GetStringAsync(key);
    }

    public async Task<IReadOnlyList<AppTranslationDto>> GetAllTranslationsAsync(string locale, CancellationToken cancellationToken = default)
    {
        var targetLocale = string.IsNullOrWhiteSpace(locale) ? CurrentLocale : locale.Trim().ToLowerInvariant();
        await EnsureLoadedAsync(targetLocale, cancellationToken);

        return _cache.TryGetValue(targetLocale, out var dict)
            ? dict.Values.Select(t => new AppTranslationDto(
                t.Id,
                t.TenantId,
                t.Locale,
                t.Key,
                t.DefaultValue,
                t.CustomValue,
                t.EffectiveValue,
                t.UpdatedAtUtc)).ToList()
            : Array.Empty<AppTranslationDto>();
    }

    public async Task SaveCustomTranslationAsync(string locale, string key, string customValue, CancellationToken cancellationToken = default)
    {
        var targetLocale = string.IsNullOrWhiteSpace(locale) ? CurrentLocale : locale.Trim().ToLowerInvariant();

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IFatturaFacileDbContext>();

        var existing = await db.AppTranslations
            .FirstOrDefaultAsync(t => t.Locale == targetLocale && t.Key == key, cancellationToken);

        if (existing != null)
        {
            existing.UpdateCustomValue(customValue);
        }
        else
        {
            existing = AppTranslation.Create(targetLocale, key, key, customValue);

            // Will be added through DbContext directly if needed
        }

        _ = await db.SaveChangesAsync(cancellationToken);

        // Update local memory cache
        var localeCache = _cache.GetOrAdd(targetLocale, _ => new ConcurrentDictionary<string, AppTranslation>(StringComparer.OrdinalIgnoreCase));
        localeCache[key] = existing;

        LanguageChanged?.Invoke();
    }

    public async Task InitializeCacheAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IFatturaFacileDbContext>();

        var list = await db.AppTranslations.ToListAsync(cancellationToken);
        foreach (var item in list)
        {
            var localeDict = _cache.GetOrAdd(item.Locale, _ => new ConcurrentDictionary<string, AppTranslation>(StringComparer.OrdinalIgnoreCase));
            localeDict[item.Key] = item;
        }
    }

    private async Task EnsureLoadedAsync(string locale, CancellationToken cancellationToken)
    {
        if (_cache.ContainsKey(locale))
        {
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IFatturaFacileDbContext>();

        var list = await db.AppTranslations
            .Where(t => t.Locale == locale)
            .ToListAsync(cancellationToken);

        var localeDict = _cache.GetOrAdd(locale, _ => new ConcurrentDictionary<string, AppTranslation>(StringComparer.OrdinalIgnoreCase));
        foreach (var item in list)
        {
            localeDict[item.Key] = item;
        }
    }
}
