// <copyright file="HelpdeskKnowledgeLocale.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// Maps the culture of the application to the locale codes of the knowledge base endpoint.
/// </summary>
public static class HelpdeskKnowledgeLocale
{
    /// <summary>
    /// The locale codes the knowledge base accepts.
    /// </summary>
    public static readonly IReadOnlyList<string> Supported = ["it-CH", "de-CH", "fr-CH", "en", "it-IT"];

    private const string Default = "it-IT";

    /// <summary>
    /// Gets the knowledge base locale of a culture. German and French map to their Swiss variants, English to en, Italian
    /// keeps the Swiss or Italian region and anything else falls back to it-IT.
    /// </summary>
    /// <param name="culture">The culture.</param>
    /// <returns>One of <see cref="Supported"/>.</returns>
    public static string From(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);

        return culture.TwoLetterISOLanguageName switch
        {
            "de" => "de-CH",
            "fr" => "fr-CH",
            "en" => "en",
            "it" => culture.Name.EndsWith("-CH", StringComparison.OrdinalIgnoreCase) ? "it-CH" : "it-IT",
            _ => Default,
        };
    }
}
