// <copyright file="LocalizationManager.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.ComponentModel;
using FatturaFacile.Application.Common.Interfaces;

namespace FatturaFacile.Services;

/// <summary>
/// Represents localization manager.
/// </summary>
public sealed class LocalizationManager : INotifyPropertyChanged
{
    private ILocalizationService? localizationService;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the instance.
    /// </summary>
    public static LocalizationManager Instance => field ??= new LocalizationManager();

    /// <summary>
    /// Gets the current locale.
    /// </summary>
    public string CurrentLocale => localizationService?.CurrentLocale ?? "it";

    public string this[
        string key] => localizationService == null ? key : localizationService.GetString(key);

    /// <summary>
    /// Initialize.
    /// </summary>
    /// <param name="localizationService">The localization service.</param>
    public void Initialize(
        ILocalizationService localizationService)
    {
        this.localizationService = localizationService;
        this.localizationService.LanguageChanged += OnLanguageChanged;
    }

    /// <summary>
    /// Change Language.
    /// </summary>
    /// <param name="locale">The locale.</param>
    public void ChangeLanguage(
        string locale) => localizationService?.SetLocale(locale);

    private void OnLanguageChanged()
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(nameof(CurrentLocale)));
    }
}
