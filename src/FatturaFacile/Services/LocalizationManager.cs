using System.ComponentModel;
using FatturaFacile.Application.Common.Interfaces;

namespace FatturaFacile.Services;

public sealed class LocalizationManager : INotifyPropertyChanged
{
    public static LocalizationManager Instance => field ??= new LocalizationManager();

    private ILocalizationService? _localizationService;

    public event PropertyChangedEventHandler? PropertyChanged;

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

    public string CurrentLocale => _localizationService?.CurrentLocale ?? "it";

    public void ChangeLanguage(string locale) => _localizationService?.SetLocale(locale);

    private void OnLanguageChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLocale)));
    }
}
