// <copyright file="CyclePlansViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.InventoryCounting;

namespace FatturaFacile.Wpf.ViewModels.PhysicalInventory;

/// <summary>
/// Cycle count plans: lists them, generates the A/B/C plans of a warehouse, opens a count now for a plan and runs
/// every plan that is due.
/// </summary>
internal sealed partial class CyclePlansViewModel : ObservableObject
{
    private readonly IInventoryCountingApiClient _client;
    private readonly ILocalizationService _localization;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
    [NotifyCanExecuteChangedFor(nameof(RunSelectedCommand))]
    [NotifyCanExecuteChangedFor(nameof(RunDueCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteSelectedCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
    private string _warehouseId = string.Empty;

    [ObservableProperty]
    private string _zoneId = string.Empty;

    [ObservableProperty]
    private bool _freezesStock = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RunSelectedCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteSelectedCommand))]
    private CyclePlanRow? _selectedPlan;

    /// <summary>
    /// Initializes a new instance of the <see cref="CyclePlansViewModel"/> class.
    /// </summary>
    /// <param name="client">The physical inventory web-api client.</param>
    /// <param name="localization">The localization service.</param>
    public CyclePlansViewModel(IInventoryCountingApiClient client, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(localization);

        _client = client;
        _localization = localization;
    }

    /// <summary>
    /// Gets the cycle count plans.
    /// </summary>
    public ObservableCollection<CyclePlanRow> Plans { get; } = [];

    /// <summary>
    /// Loads the plans from the web-api.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            var result = await _client.GetPlansAsync(CancellationToken.None);
            if (result.IsSuccess && result.Value is not null)
            {
                Fill(result.Value);
                StatusMessage = Plans.Count == 0 ? _localization["Inv_Plan_Empty"] : string.Empty;
            }
            else
            {
                StatusMessage = CountingText.Describe(result, _localization);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Generates the A/B/C plans of the entered warehouse.
    /// </summary>
    /// <returns>A task representing the generation.</returns>
    [RelayCommand(CanExecute = nameof(CanGenerate))]
    public async Task GenerateAsync()
    {
        IsBusy = true;
        var generated = false;
        var message = string.Empty;

        try
        {
            var zone = string.IsNullOrWhiteSpace(ZoneId) ? null : ZoneId.Trim();
            var result = await _client.GeneratePlansAsync(WarehouseId.Trim(), zone, FreezesStock, CancellationToken.None);
            generated = result.IsSuccess && result.Value is not null;
            message = generated ? _localization["Inv_Plan_Generated"] : CountingText.Describe(result, _localization);
        }
        finally
        {
            IsBusy = false;
        }

        if (generated)
        {
            await LoadAsync();
        }

        StatusMessage = message;
    }

    /// <summary>
    /// Opens a count session for the selected plan now.
    /// </summary>
    /// <returns>A task representing the run.</returns>
    [RelayCommand(CanExecute = nameof(CanActOnSelection))]
    public async Task RunSelectedAsync()
    {
        if (SelectedPlan is null)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _client.RunPlanAsync(SelectedPlan.Id, CancellationToken.None);
            StatusMessage = result.IsSuccess ? _localization["Inv_Session_Created"] : CountingText.Describe(result, _localization);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Opens the count sessions of every plan that is due.
    /// </summary>
    /// <returns>A task representing the run.</returns>
    [RelayCommand(CanExecute = nameof(CanRunDue))]
    public async Task RunDueAsync()
    {
        IsBusy = true;
        var message = string.Empty;

        try
        {
            var result = await _client.RunDuePlansAsync(CancellationToken.None);
            message = result.IsSuccess
                ? _localization["Inv_Plan_RunDueDone"].Replace("{count}", result.Value.ToString(CultureInfo.CurrentCulture), StringComparison.Ordinal)
                : CountingText.Describe(result, _localization);
        }
        finally
        {
            IsBusy = false;
        }

        await LoadAsync();
        StatusMessage = message;
    }

    /// <summary>
    /// Deletes the selected plan.
    /// </summary>
    /// <returns>A task representing the deletion.</returns>
    [RelayCommand(CanExecute = nameof(CanActOnSelection))]
    public async Task DeleteSelectedAsync()
    {
        if (SelectedPlan is null)
        {
            return;
        }

        IsBusy = true;
        var deleted = false;
        var message = string.Empty;

        try
        {
            var result = await _client.DeletePlanAsync(SelectedPlan.Id, CancellationToken.None);
            deleted = result.IsSuccess;
            message = deleted ? _localization["Common_Deleted"] : CountingText.Describe(result, _localization);
        }
        finally
        {
            IsBusy = false;
        }

        if (deleted)
        {
            await LoadAsync();
        }

        StatusMessage = message;
    }

    private bool CanGenerate() => !IsBusy && !string.IsNullOrWhiteSpace(WarehouseId);

    private bool CanActOnSelection() => !IsBusy && SelectedPlan is not null;

    private bool CanRunDue() => !IsBusy;

    private void Fill(IReadOnlyList<CycleCountPlanDto> plans)
    {
        Plans.Clear();
        foreach (var plan in plans)
        {
            Plans.Add(new CyclePlanRow(plan, _localization));
        }
    }
}
