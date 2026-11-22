// <copyright file="GelatoWeighingViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Gelato;
using FatturaFacile.Domain.Gelato;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Drives the guided laboratory weighing station (Mod 398): choose a recipe and a batch weight, then weigh the ingredients one
/// at a time against their scaled targets, with the weight typed from the scale and the raw material lot.
/// </summary>
internal sealed partial class GelatoWeighingViewModel : ObservableObject
{
    private readonly IGelatoApiClient _api;
    private readonly ILocalizationService _localization;
    private WeighingPlan? _plan;

    [ObservableProperty]
    private GelatoRecipeSummary? _selectedRecipe;

    [ObservableProperty]
    private decimal _batchWeightGrams = 3500m;

    [ObservableProperty]
    private string _measuredText = string.Empty;

    [ObservableProperty]
    private string _lotCode = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isComplete;

    [ObservableProperty]
    private WeighingStep? _currentStep;

    /// <summary>
    /// Initializes a new instance of the <see cref="GelatoWeighingViewModel"/> class.
    /// </summary>
    /// <param name="api">The gelato API client.</param>
    /// <param name="localization">The localization service.</param>
    public GelatoWeighingViewModel(IGelatoApiClient api, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(api);
        ArgumentNullException.ThrowIfNull(localization);

        _api = api;
        _localization = localization;
    }

    /// <summary>
    /// Gets the recipes to choose from.
    /// </summary>
    public ObservableCollection<GelatoRecipeSummary> Recipes { get; } = [];

    /// <summary>
    /// Gets the steps of the running weighing.
    /// </summary>
    public ObservableCollection<WeighingStep> Steps { get; } = [];

    /// <summary>
    /// Loads the recipes of the tenant.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    public async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            var recipes = await _api.GetRecipesAsync(Locale()).ConfigureAwait(true);
            Recipes.Clear();

            foreach (var recipe in recipes)
            {
                Recipes.Add(recipe);
            }

            StatusMessage = string.Empty;
        }
        catch (HttpRequestException)
        {
            StatusMessage = _localization["Gelato_Offline"];
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Starts the weighing of the selected recipe at the batch weight.
    /// </summary>
    /// <returns>A task representing the start.</returns>
    [RelayCommand]
    public async Task StartAsync()
    {
        if (SelectedRecipe is null || BatchWeightGrams <= 0m)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var lines = await _api.GetRecipeLinesAsync(SelectedRecipe.Id, Locale()).ConfigureAwait(true);
            _plan = WeighingPlan.Create([.. lines.Select(static l => (l.IngredientId, l.Name, l.Grams))], BatchWeightGrams);
            Refresh();
            StatusMessage = string.Empty;
        }
        catch (HttpRequestException)
        {
            StatusMessage = _localization["Gelato_Offline"];
        }
        catch (ArgumentException)
        {
            StatusMessage = _localization["Gelato_EmptyRecipe"];
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Confirms the weight typed from the scale for the current ingredient and moves to the next one.
    /// </summary>
    /// <returns>A task representing the confirmation.</returns>
    [RelayCommand]
    public async Task ConfirmWeightAsync()
    {
        if (_plan is null || CurrentStep is null)
        {
            return;
        }

        if (!decimal.TryParse(MeasuredText, NumberStyles.Number, CultureInfo.CurrentCulture, out var grams) || grams < 0m)
        {
            StatusMessage = _localization["Gelato_InvalidWeight"];

            return;
        }

        var index = _plan.CurrentIndex;
        var status = _plan.Record(index, grams, string.IsNullOrWhiteSpace(LotCode) ? null : LotCode.Trim());
        var ingredientId = _plan.Steps[index].IngredientId;

        try
        {
            await _api.RecordScaleReadingAsync(ingredientId, grams).ConfigureAwait(true);
        }
        catch (HttpRequestException)
        {
            // The weighing goes on without a connection; the reading stays in the plan.
            StatusMessage = _localization["Gelato_Offline"];
        }

        MeasuredText = string.Empty;
        Refresh();

        if (status != WeighingStatus.InTolerance)
        {
            StatusMessage = _localization[status == WeighingStatus.Under ? "Gelato_Under" : "Gelato_Over"];
        }
    }

    private string Locale() => _localization.CurrentCulture.TwoLetterISOLanguageName;

    private void Refresh()
    {
        Steps.Clear();

        if (_plan is null)
        {
            return;
        }

        foreach (var step in _plan.Steps)
        {
            Steps.Add(step);
        }

        IsComplete = _plan.IsComplete;
        CurrentStep = _plan.CurrentIndex >= 0 ? _plan.Steps[_plan.CurrentIndex] : null;
    }
}
