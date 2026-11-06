// <copyright file="ReportCulture.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;

namespace FatturaFacile.Application.Common.Reporting;

/// <summary>
/// Derives the formatting culture of a template from the language suffix of its name
/// ("Fattura_IT", "Fattura_EN", "Rechnung_DE", "Facture_FR") and the company country.
/// </summary>
public static class ReportCulture
{
    /// <summary>
    /// Returns the culture for a template name.
    /// </summary>
    /// <param name="templateName">The template name without extension.</param>
    /// <param name="companyCountry">The ISO country of the company ("CH" or "IT").</param>
    /// <returns>The culture used to format dates and amounts.</returns>
    public static CultureInfo FromTemplateName(string templateName, string companyCountry)
    {
        ArgumentNullException.ThrowIfNull(templateName);

        var separator = templateName.AsSpan().LastIndexOf('_');
        var language = separator >= 0 ? templateName[(separator + 1)..].ToUpperInvariant() : "IT";
        var swiss = string.Equals(companyCountry, "CH", StringComparison.OrdinalIgnoreCase);

        var name = language switch
        {
            "DE" => swiss ? "de-CH" : "de-DE",
            "FR" => swiss ? "fr-CH" : "fr-FR",
            "EN" => "en-GB",
            _ => swiss ? "it-CH" : "it-IT",
        };

        return CultureInfo.GetCultureInfo(name);
    }

    /// <summary>
    /// Returns the two-letter language of a culture in upper case ("IT", "DE", "FR", "EN").
    /// </summary>
    /// <param name="culture">The culture.</param>
    /// <returns>The language code.</returns>
    public static string Language(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);
        return culture.TwoLetterISOLanguageName.ToUpperInvariant();
    }
}
