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
/// Represents localization admin view model.
/// </summary>
/// <param name="mediator">The mediator.</param>
public sealed partial class LocalizationAdminViewModel(ISender mediator) : ViewModelBase
{
    private readonly ISender mediator = mediator;

    /// <summary>
    /// Gets or sets the selected locale.
    /// </summary>
    [ObservableProperty]
    public partial string SelectedLocale { get; set; } = "it";

    /// <summary>
    /// Gets or sets the available locales.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<string> AvailableLocales { get; set; } = ["it", "de", "fr", "en"];

    /// <summary>
    /// Gets or sets the translations.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<AppTranslationDto> Translations { get; set; } = [];

    /// <summary>
    /// Gets or sets the selected translation.
    /// </summary>
    [ObservableProperty]
    public partial AppTranslationDto? SelectedTranslation { get; set; }

    /// <summary>
    /// Gets or sets the search filter.
    /// </summary>
    [ObservableProperty]
    public partial string SearchFilter { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the edit custom value.
    /// </summary>
    [ObservableProperty]
    public partial string EditCustomValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the save success message.
    /// </summary>
    [ObservableProperty]
    public partial string SaveSuccessMessage { get; set; } = string.Empty;

    /// <summary>
    /// Initialize Async.
    /// </summary>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public override Task InitializeAsync() => LoadTranslationsAsync();

    /// <summary>
    /// Load Translations Async.
    /// </summary>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
    /// Save Translation Async.
    /// </summary>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
