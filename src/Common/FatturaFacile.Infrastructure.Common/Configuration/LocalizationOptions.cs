// <copyright file="LocalizationOptions.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Infrastructure.Common.Configuration;

/// <summary>
/// Configuration options for application localization and supported cultures.
/// </summary>
public sealed class LocalizationOptions
{
    /// <summary>
    /// Gets or sets the fallback culture code when a requested culture is unavailable (default: "en").
    /// </summary>
    public string DefaultCulture { get; set; } = "en";

    /// <summary>
    /// Gets or sets the list of enabled two-letter language culture codes (e.g., ["en", "it", "de", "fr"]).
    /// </summary>
    public List<string> SupportedCultures { get; set; } = ["en", "it", "de", "fr"];
}
