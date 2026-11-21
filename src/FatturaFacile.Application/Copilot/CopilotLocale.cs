// <copyright file="CopilotLocale.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;

namespace FatturaFacile.Application.Copilot;

/// <summary>
/// Maps the UI culture of the desktop application to a locale accepted by the Copilot API
/// (it, de, fr, en, it-CH, de-CH, fr-CH, it-IT).
/// </summary>
public static class CopilotLocale
{
    private static readonly HashSet<string> Accepted = new(["it", "de", "fr", "en", "it-CH", "de-CH", "fr-CH", "it-IT"], StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Maps a culture to an API locale, falling back to the two letter language and then to "en".
    /// </summary>
    /// <param name="culture">The UI culture.</param>
    /// <returns>The API locale.</returns>
    public static string From(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);

        if (Accepted.Contains(culture.Name))
        {
            return Canonical(culture.Name);
        }

        var language = culture.TwoLetterISOLanguageName;
        return Accepted.Contains(language) ? language.ToLowerInvariant() : "en";
    }

    private static string Canonical(string name)
    {
        var parts = name.Split('-');
        return parts.Length == 2 ? parts[0].ToLowerInvariant() + "-" + parts[1].ToUpperInvariant() : name.ToLowerInvariant();
    }
}
