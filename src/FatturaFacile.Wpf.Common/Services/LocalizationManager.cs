// <copyright file="LocalizationManager.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.ComponentModel;
using FatturaFacile.Application.Common.Interfaces;

namespace FatturaFacile.Wpf.Common.Services;

/// <summary>
/// Singleton manager bridging the core localization service with WPF data-binding mechanisms.
/// Supports localized string lookup and notifications on culture/language changes.
/// </summary>
public sealed class LocalizationManager : INotifyPropertyChanged
{

    /// <summary>
    /// The underlying localization service instance.
    /// </summary>
    private ILocalizationService? localizationService;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the singleton instance of the <see cref="LocalizationManager"/>.
    /// </summary>
    public static LocalizationManager Instance => field ??= new LocalizationManager();

    /// <summary>
    /// Gets the current active culture/locale identifier (e.g., "it", "en"). Defaults to "it".
    /// </summary>
    public string CurrentLocale => localizationService?.CurrentLocale ?? "it";

    /// <summary>
    /// Indexer used by XAML data bindings to resolve localized strings by resource key.
    /// </summary>
    /// <param name="key">The localization resource key.</param>
    /// <returns>The translated string corresponding to <paramref name="key"/>, or the key itself if uninitialized or missing.</returns>
    public string this[string key] => localizationService == null ? key : localizationService.GetString(key: key);

    /// <summary>
    /// Initializes the manager with the application-wide localization service and hooks up event listeners.
    /// </summary>
    /// <param name="localizationService">The underlying localization service implementation.</param>
    public void Initialize(ILocalizationService localizationService)
    {
        this.localizationService                 =  localizationService;
        this.localizationService.LanguageChanged += OnLanguageChanged;
    }

    /// <summary>
    /// Changes the current application language/locale and propagates change notifications to bound UI controls.
    /// </summary>
    /// <param name="locale">The new culture/locale code (e.g. "it", "de", "en", "fr").</param>
    public void ChangeLanguage(string locale) => localizationService?.SetLocale(locale: locale);

    /// <summary>
    /// Handles language changed notifications and raises property changed events for UI bindings.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event data.</param>
    private void OnLanguageChanged(
        object?   sender,
        EventArgs e)
    {
        PropertyChanged?.Invoke(
                                sender: this,
                                e: new PropertyChangedEventArgs(propertyName: "Item[]"));

        PropertyChanged?.Invoke(
                                sender: this,
                                e: new PropertyChangedEventArgs(propertyName: nameof(CurrentLocale)));
    }

}
