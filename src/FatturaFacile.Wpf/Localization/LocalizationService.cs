// <copyright file="LocalizationService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Text.Json;
using FatturaFacile.Application.Common.Interfaces;

namespace FatturaFacile.Wpf.Localization;

/// <summary>
/// Dynamic runtime localization service providing real-time language switching, OS auto-detection, and persistence.
/// </summary>
internal sealed class LocalizationService : ILocalizationService, INotifyPropertyChanged
{
    private static readonly Lazy<LocalizationService> _instance = new(() => new LocalizationService());
    private static readonly string _settingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FatturaFacile",
        "language.json");

    private readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>?> _localizedDictionaries = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationService"/> class.
    /// Evaluates stored user preference, falling back to host OS UI language or default Italian.
    /// </summary>
    public LocalizationService()
    {
        SupportedCultures =
        [
            new CultureInfo("it-IT"),
            new CultureInfo("en-US"),
            new CultureInfo("de-CH"),
            new CultureInfo("fr-CH"),
        ];

        CurrentCulture = ResolveInitialCulture();
        ApplyCulture(CurrentCulture, persist: false);
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event EventHandler? CultureChanged;

    /// <summary>
    /// Gets the singleton instance of the localization service.
    /// </summary>
    public static LocalizationService Instance => _instance.Value;

    /// <inheritdoc/>
    public CultureInfo CurrentCulture { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyList<CultureInfo> SupportedCultures { get; }

    /// <inheritdoc/>
    public string this[string key] => GetString(key);

    /// <inheritdoc/>
    public void SetCulture(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);

        var resolved = SupportedCultures.FirstOrDefault(c =>
            string.Equals(c.Name, culture.Name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.TwoLetterISOLanguageName, culture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase))
            ?? SupportedCultures[0];

        if (Equals(CurrentCulture, resolved))
        {
            return;
        }

        ApplyCulture(resolved, persist: true);
    }

    /// <inheritdoc/>
    public string GetString(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        try
        {
            var dict = GetLocalizedDictionary(CurrentCulture);
            if (dict != null && dict.TryGetValue(key, out var val) && !string.IsNullOrEmpty(val))
            {
                return val;
            }

            var fallback = Strings.ResourceManager.GetString(key, CurrentCulture);
            return string.IsNullOrEmpty(fallback) ? key : fallback;
        }
        catch
        {
            return key;
        }
    }

    private static string? LoadSavedCultureCode()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("Language", out var langProp))
                {
                    return langProp.GetString();
                }
            }
        }
        catch
        {
            // Ignore corrupted preferences and fall back gracefully
        }

        return null;
    }

    private static void SaveCulturePreference(string cultureName)
    {
        try
        {
            var directory = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                _ = Directory.CreateDirectory(directory);
            }

            var content = JsonSerializer.Serialize(new { Language = cultureName });
            File.WriteAllText(_settingsFilePath, content);
        }
        catch
        {
            // Non-critical local persistence failure
        }
    }

    private static IReadOnlyDictionary<string, string>? LoadDictionary(CultureInfo culture)
    {
        return LoadDictionaryFromSatellite(culture)
            ?? (!string.IsNullOrEmpty(culture.Parent.Name)
                ? LoadDictionaryFromSatellite(culture.Parent)
                : null);
    }

    private static IReadOnlyDictionary<string, string>? LoadDictionaryFromSatellite(CultureInfo culture)
    {
        try
        {
            var satellite = typeof(Strings).Assembly.GetSatelliteAssembly(culture);
            using var stream = satellite.GetManifestResourceStream("FatturaFacile.Wpf.Localization.Strings.resources");
            if (stream == null)
            {
                return null;
            }

            using var reader = new ResourceReader(stream);
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (DictionaryEntry entry in reader)
            {
                if (entry.Key is string k && entry.Value is string v)
                {
                    result[k] = v;
                }
            }

            return result;
        }
        catch
        {
            return null;
        }
    }

    private void ApplyCulture(CultureInfo culture, bool persist)
    {
        CurrentCulture = culture;
        Strings.Culture = culture;

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        if (persist)
        {
            SaveCulturePreference(culture.Name);
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        CultureChanged?.Invoke(this, EventArgs.Empty);
    }

    private IReadOnlyDictionary<string, string>? GetLocalizedDictionary(CultureInfo culture) =>
        _localizedDictionaries.GetOrAdd(culture.Name, static (_, cult) => LoadDictionary(cult), culture);

    private CultureInfo ResolveInitialCulture()
    {
        // 1. Check user persisted setting
        var savedCode = LoadSavedCultureCode();
        if (!string.IsNullOrWhiteSpace(savedCode))
        {
            var match = SupportedCultures.FirstOrDefault(c => string.Equals(c.Name, savedCode, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                return match;
            }
        }

        // 2. Fall back to host OS UI language
        var osCulture = CultureInfo.CurrentUICulture;
        var osMatch = SupportedCultures.FirstOrDefault(c =>
            string.Equals(c.Name, osCulture.Name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.TwoLetterISOLanguageName, osCulture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase));

        if (osMatch != null)
        {
            return osMatch;
        }

        // 3. Fall back to default neutral language (Italian)
        return SupportedCultures[0];
    }
}
