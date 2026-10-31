// <copyright file="LocalizationManager.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Globalization;
using System.Resources;
using FatturaFacile.Wpf.Resources;

namespace FatturaFacile.Wpf.Localization;

/// <summary>
/// Singleton manager managing runtime language and UI culture switching, providing dynamic indexer resolution
/// for localized resources to support dynamic XAML markup updates without app restart.
/// </summary>
public class LocalizationManager : INotifyPropertyChanged
{
    private static readonly Lazy<LocalizationManager> _lazyInstance = new(() => new LocalizationManager());

    /// <summary>
    /// Gets the singleton instance of <see cref="LocalizationManager"/>.
    /// </summary>
    public static LocalizationManager Instance => _lazyInstance.Value;

    private readonly ResourceManager _resourceManager;
    private CultureInfo _currentCulture;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationManager"/> class with default "it-CH" culture.
    /// </summary>
    public LocalizationManager()
    {
        _resourceManager = new ResourceManager(typeof(Strings));
        _currentCulture = new CultureInfo("it-CH");
    }

    /// <summary>
    /// Gets or sets the actively applied UI culture and language.
    /// </summary>
    public CultureInfo CurrentCulture
    {
        get => _currentCulture;
        set
        {
            if (!Equals(_currentCulture, value))
            {
                _currentCulture = value;
                CultureInfo.CurrentCulture = value;
                CultureInfo.CurrentUICulture = value;
                Thread.CurrentThread.CurrentCulture = value;
                Thread.CurrentThread.CurrentUICulture = value;

                // Raising with empty string or "Item[]" notifies WPF that all indexer bindings should re-query
                OnPropertyChanged("Item[]");
                OnPropertyChanged(string.Empty);
            }
        }
    }

    /// <summary>
    /// Indexer returning the localized string for a specific resource key.
    /// </summary>
    /// <param name="key">The resource key name.</param>
    /// <returns>The localized text, or a fallback placeholder if not found.</returns>
    public string this[string key]
    {
        get
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            try
            {
                var val = _resourceManager.GetString(key, _currentCulture);
                return val ?? $"#{key}#";
            }
            catch
            {
                return $"#{key}#";
            }
        }
    }

    /// <summary>
    /// Changes the active UI culture by RFC language tag (e.g. "it-CH", "en-US", "de-CH", "fr-CH").
    /// </summary>
    /// <param name="cultureCode">The culture identifier code.</param>
    public void SetCulture(string cultureCode)
    {
        try
        {
            CurrentCulture = new CultureInfo(cultureCode);
        }
        catch
        {
            CurrentCulture = new CultureInfo("it-CH");
        }
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for binding notifications.
    /// </summary>
    /// <param name="propertyName">The name of the changed property.</param>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
