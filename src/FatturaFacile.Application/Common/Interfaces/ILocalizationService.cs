using FatturaFacile.Application.DTOs;

namespace FatturaFacile.Application.Common.Interfaces;

public interface ILocalizationService
{
    public string CurrentLocale { get; }
    public void SetLocale(string locale);
    public string GetString(string key, string? defaultFallback = null);
    public Task<string> GetStringAsync(string key, string? locale = null, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<AppTranslationDto>> GetAllTranslationsAsync(string locale, CancellationToken cancellationToken = default);
    public Task SaveCustomTranslationAsync(string locale, string key, string customValue, CancellationToken cancellationToken = default);
    public event Action? LanguageChanged;
}
