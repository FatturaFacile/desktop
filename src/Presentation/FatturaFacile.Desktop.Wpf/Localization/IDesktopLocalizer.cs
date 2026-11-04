// <copyright file="IDesktopLocalizer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.ComponentModel;

namespace FatturaFacile.Desktop.Wpf.Localization;

/// <summary>
/// Service providing localized strings for the desktop interface across English, Italian, German, and French.
/// </summary>
public interface IDesktopLocalizer : INotifyPropertyChanged
{
    /// <summary>
    /// Gets the current active UI culture code (e.g., 'en', 'it', 'de', 'fr').
    /// </summary>
    public string CurrentCulture { get; }

    /// <summary>
    /// Gets the localized string associated with the specified resource key.
    /// </summary>
    /// <param name="key">The translation key.</param>
    public string this[string key] { get; }

    /// <summary>
    /// Switches the active culture and triggers UI property notification.
    /// </summary>
    /// <param name="culture">The new culture code.</param>
    public void SetCulture(string culture);

    /// <summary>
    /// Resolves the localized string for a specified translation key.
    /// </summary>
    /// <param name="key">The translation key.</param>
    /// <returns>The localized text or fallback value.</returns>
    public string GetString(string key);
}
