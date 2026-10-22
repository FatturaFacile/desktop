// <copyright file="LocalizationAdminViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Localization;
using MediatR;

namespace FatturaFacile.ViewModels;

/// <summary>
/// Provides administrative capabilities for inspecting, filtering, and customizing localized resource strings across supported application languages.
/// </summary>
/// <param name="mediator">The MediatR sender used to execute localization queries and commands.</param>
public sealed partial class LocalizationAdminViewModel(ISender mediator) : ViewModelBase
{
    private readonly ISender mediator = mediator;

    /// <summary>
    /// Gets or sets the language/culture code currently selected for localization administration (e.g., "it", "de", "fr", "en").
    /// </summary>
    [ObservableProperty]
    public partial string SelectedLocale { get; set; } = "it";

    /// <summary>
    /// Gets or sets the list of available culture codes supported by the localization infrastructure.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<string> AvailableLocales { get; set; } = ["it", "de", "fr", "en"];

    /// <summary>
    /// Gets or sets the collection of translation entries matching the selected locale and search criteria.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<AppTranslationDto> Translations { get; set; } = [];

    /// <summary>
    /// Gets or sets the translation entry currently selected for editing, or <see langword="null"/> if none is selected.
    /// </summary>
    [ObservableProperty]
    public partial AppTranslationDto? SelectedTranslation { get; set; }

    /// <summary>
    /// Gets or sets the search filter applied against translation keys, default texts, or custom overrides.
    /// </summary>
    [ObservableProperty]
    public partial string SearchFilter { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the custom text override being edited for the <see cref="SelectedTranslation"/>.
    /// </summary>
    [ObservableProperty]
    public partial string EditCustomValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the feedback message displayed upon successfully persisting a translation override to the database.
    /// </summary>
    [ObservableProperty]
    public partial string SaveSuccessMessage { get; set; } = string.Empty;

    /// <summary>
    /// Asynchronously initializes the ViewModel by loading translations for the default locale.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous initialization operation.</returns>
    public override Task InitializeAsync() => LoadTranslationsAsync();

    /// <summary>
    /// Queries the backend via MediatR for translations associated with the <see cref="SelectedLocale"/> and applies search filtering.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous query operation.</returns>
    [RelayCommand]
    public async Task LoadTranslationsAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        SaveSuccessMessage = string.Empty;
        try
        {
            var list = await mediator.Send(new GetTranslationsQuery(SelectedLocale));
            if (!string.IsNullOrWhiteSpace(SearchFilter))
            {
                list = list.Where(t => t.Key.Contains(
                                           SearchFilter,
                                           StringComparison.OrdinalIgnoreCase)
                                       || t.DefaultValue.Contains(
                                           SearchFilter,
                                           StringComparison.OrdinalIgnoreCase)
                                       || t.CustomValue?.Contains(
                                           SearchFilter,
                                           StringComparison.OrdinalIgnoreCase) ==
                                       true)
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

    /// <summary>
    /// Dispatches an <see cref="UpdateTranslationCommand"/> to save custom localized text for the currently selected key and locale.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous persistence operation.</returns>
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
            _ = await mediator.Send(
                new UpdateTranslationCommand(
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

    partial void OnSelectedTranslationChanged(
        AppTranslationDto? value)
    {
        if (value != null)
        {
            EditCustomValue = value.CustomValue ?? value.DefaultValue;
        }
    }
}
