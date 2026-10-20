using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Localization;
using MediatR;

namespace FatturaFacile.ViewModels;

public sealed partial class LocalizationAdminViewModel(ISender mediator, ILocalizationService localizationService) : ViewModelBase
{
    private readonly ISender _mediator = mediator;
    private readonly ILocalizationService _localizationService = localizationService;

    [ObservableProperty]
    public partial string SelectedLocale { get; set; } = "it";

    [ObservableProperty]
    public partial ObservableCollection<string> AvailableLocales { get; set; } = ["it", "de", "fr", "en"];

    [ObservableProperty]
    public partial ObservableCollection<AppTranslationDto> Translations { get; set; } = [];

    [ObservableProperty]
    public partial AppTranslationDto? SelectedTranslation { get; set; }

    [ObservableProperty]
    public partial string SearchFilter { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EditCustomValue { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SaveSuccessMessage { get; set; } = string.Empty;

    public override Task InitializeAsync() => LoadTranslationsAsync();

    [RelayCommand]
    public async Task LoadTranslationsAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        SaveSuccessMessage = string.Empty;
        try
        {
            var list = await _mediator.Send(new GetTranslationsQuery(SelectedLocale));
            if (!string.IsNullOrWhiteSpace(SearchFilter))
            {
                list = list.Where(t => t.Key.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase)
                                    || t.DefaultValue.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase)
                                    || (t.CustomValue?.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase) == true))
                           .ToList();
            }

            Translations = [with(list)];
            SelectedTranslation = Translations.FirstOrDefault();
            if (SelectedTranslation != null)
            {
                EditCustomValue = SelectedTranslation.CustomValue ?? SelectedTranslation.DefaultValue;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSelectedTranslationChanged(AppTranslationDto? value)
    {
        if (value != null)
        {
            EditCustomValue = value.CustomValue ?? value.DefaultValue;
        }
    }

    [RelayCommand]
    public async Task SaveTranslationAsync()
    {
        if (SelectedTranslation == null)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        SaveSuccessMessage = string.Empty;
        try
        {
            _ = await _mediator.Send(new UpdateTranslationCommand(
                SelectedTranslation.Locale,
                SelectedTranslation.Key,
                EditCustomValue));

            SaveSuccessMessage = $"Traduzione '{SelectedTranslation.Key}' salvata con successo nel database SQLite!";
            await LoadTranslationsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
