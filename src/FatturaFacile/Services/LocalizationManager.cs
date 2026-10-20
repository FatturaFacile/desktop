using System.ComponentModel;
using FatturaFacile.Application.Common.Interfaces;

namespace FatturaFacile.Services;

/// <summary>
    /// Represents localization manager.
    /// </summary>
    public sealed class LocalizationManager : INotifyPropertyChanged
    {
    /// <summary>
    /// Gets the instance.
    /// </summary>
public static LocalizationManager Instance => field ??= new LocalizationManager();

private ILocalizationService? _localizationService;

public event PropertyChangedEventHandler? PropertyChanged;

/// <summary>
/// Initialize.
/// </summary>
/// <param name="localizationService">The localization service.</param>
    public void Initialize(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
        _localizationService.LanguageChanged += OnLanguageChanged;
    }

public string this[string key]
    {
        get
        {
            return _localizationService == null ? key : _localizationService.GetString(key);
        }
    }

/// <summary>
/// Gets the current locale.
/// </summary>
    public string CurrentLocale => _localizationService?.CurrentLocale ?? "it";

/// <summary>
/// Change Language.
/// </summary>
/// <param name="locale">The locale.</param>
    public void ChangeLanguage(string locale) => _localizationService?.SetLocale(locale);

private void OnLanguageChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLocale)));
    }
}
