// <copyright file="ILocalizationService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Service contract providing dynamic UI and message localization with runtime language switching.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Occurs when the active UI culture changes.
    /// </summary>
    public event EventHandler? CultureChanged;

    /// <summary>
    /// Gets the currently active UI culture.
    /// </summary>
    public CultureInfo CurrentCulture { get; }

    /// <summary>
    /// Gets the list of supported application cultures.
    /// </summary>
    public IReadOnlyList<CultureInfo> SupportedCultures { get; }

    /// <summary>
    /// Gets the localized string resource via indexer.
    /// </summary>
    /// <param name="key">The resource string identifier key.</param>
    /// <returns>The localized text.</returns>
    public string this[string key] { get; }

    /// <summary>
    /// Sets the active UI culture at runtime.
    /// </summary>
    /// <param name="culture">The target culture to activate.</param>
    public void SetCulture(CultureInfo culture);

    /// <summary>
    /// Gets the localized string resource for the specified key.
    /// </summary>
    /// <param name="key">The resource string identifier key.</param>
    /// <returns>The localized text, or key placeholder if not found.</returns>
    public string GetString(string key);
}
