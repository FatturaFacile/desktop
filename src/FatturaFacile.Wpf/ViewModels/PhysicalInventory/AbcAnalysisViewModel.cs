// <copyright file="AbcAnalysisViewModel.cs" company="Fattura Facile">
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
/// ABC analysis: runs the classification, shows the ranked items with their class and cumulative percentage, keeps the
/// run history and lets the manager override the class of an item (a reason is mandatory).
/// </summary>
internal sealed partial class AbcAnalysisViewModel : ObservableObject
{
    private const decimal MaximumShare = 100m;

    private readonly IInventoryCountingApiClient _client;
    private readonly ILocalizationService _localization;

    [ObservableProperty]
    private string _periodDays = "90";

    [ObservableProperty]
    private string _aShare = "80";

    [ObservableProperty]
    private string _bShare = "95";

    [ObservableProperty]
    private string _warehouseId = string.Empty;

    [ObservableProperty]
    private bool _applyClasses = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RunAnalysisCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowRunCommand))]
    [NotifyCanExecuteChangedFor(nameof(ConfirmOverrideCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _runSummary = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ShowRunCommand))]
    private AbcRunRow? _selectedRun;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BeginOverrideCommand))]
    private AbcItemRow? _selectedItem;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmOverrideCommand))]
    private bool _isOverrideOpen;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmOverrideCommand))]
    private string _overrideClass = "A";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmOverrideCommand))]
    private bool _clearOverride;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmOverrideCommand))]
    private string _overrideReason = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="AbcAnalysisViewModel"/> class.
    /// </summary>
    /// <param name="client">The physical inventory web-api client.</param>
    /// <param name="localization">The localization service.</param>
    public AbcAnalysisViewModel(IInventoryCountingApiClient client, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(localization);

        _client = client;
        _localization = localization;
    }

    /// <summary>
    /// Gets the classes an item can be forced to.
    /// </summary>
    public IReadOnlyList<string> ClassChoices { get; } = ["A", "B", "C"];

    /// <summary>
    /// Gets the ranked items of the shown run.
    /// </summary>
    public ObservableCollection<AbcItemRow> Items { get; } = [];

    /// <summary>
    /// Gets the run history, newest first.
    /// </summary>
    public ObservableCollection<AbcRunRow> Runs { get; } = [];

    /// <summary>
    /// Gets a value indicating whether the manual override can be confirmed: an item is selected, the editor is open,
    /// the reason is filled in and a class is chosen (or the override is cleared).
    /// </summary>
    public bool CanConfirmOverride =>
        IsOverrideOpen
        && !IsBusy
        && SelectedItem is not null
        && !string.IsNullOrWhiteSpace(OverrideReason)
        && (ClearOverride || ClassChoices.Contains(OverrideClass, StringComparer.Ordinal));

    /// <summary>
    /// Loads the run history and shows the latest run.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            var result = await _client.GetAbcRunsAsync(CancellationToken.None);
            if (!result.IsSuccess || result.Value is null)
            {
                StatusMessage = CountingText.Describe(result, _localization);
                return;
            }

            Runs.Clear();
            foreach (var run in result.Value)
            {
                Runs.Add(new AbcRunRow(run));
            }

            StatusMessage = Runs.Count == 0 ? _localization["Inv_Abc_Empty"] : string.Empty;
            SelectedRun = Runs.FirstOrDefault();
        }
        finally
        {
            IsBusy = false;
        }

        if (SelectedRun is not null)
        {
            await ShowRunAsync();
        }
    }

    /// <summary>
    /// Runs a new classification with the entered parameters and shows its ranking.
    /// </summary>
    /// <returns>A task representing the run.</returns>
    [RelayCommand(CanExecute = nameof(CanRun))]
    public async Task RunAnalysisAsync()
    {
        if (!TryBuildRequest(out var request))
        {
            StatusMessage = _localization["Inv_Required"];
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _client.RunAbcAsync(request, CancellationToken.None);
            if (!result.IsSuccess || result.Value is null)
            {
                StatusMessage = CountingText.Describe(result, _localization);
                return;
            }

            var row = new AbcRunRow(result.Value);
            Runs.Insert(0, row);
            SelectedRun = row;
            Show(result.Value);
            StatusMessage = _localization["Inv_Abc_RunDone"];
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Loads the ranking of the selected run from the web-api.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand(CanExecute = nameof(CanShowRun))]
    public async Task ShowRunAsync()
    {
        if (SelectedRun is null)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _client.GetAbcRunAsync(SelectedRun.Id, CancellationToken.None);
            if (result.IsSuccess && result.Value is not null)
            {
                Show(result.Value);
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
    /// Opens the manual override editor for the selected item.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanBeginOverride))]
    public void BeginOverride()
    {
        if (SelectedItem is null)
        {
            return;
        }

        OverrideClass = ClassChoices.Contains(SelectedItem.AbcClass, StringComparer.Ordinal) ? SelectedItem.AbcClass : "A";
        ClearOverride = false;
        OverrideReason = string.Empty;
        IsOverrideOpen = true;
    }

    /// <summary>
    /// Closes the manual override editor without saving.
    /// </summary>
    [RelayCommand]
    public void CancelOverride()
    {
        IsOverrideOpen = false;
        OverrideReason = string.Empty;
    }

    /// <summary>
    /// Saves the manual class override (or clears it) with its mandatory reason and refreshes the ranking.
    /// </summary>
    /// <returns>A task representing the save.</returns>
    [RelayCommand(CanExecute = nameof(CanConfirmOverride))]
    public async Task ConfirmOverrideAsync()
    {
        if (SelectedItem is null)
        {
            return;
        }

        IsBusy = true;
        var saved = false;

        try
        {
            var result = await _client.OverrideAbcClassAsync(SelectedItem.StockItemId, ClearOverride ? null : OverrideClass, OverrideReason.Trim(), CancellationToken.None);
            if (result.IsSuccess)
            {
                saved = true;
                IsOverrideOpen = false;
                OverrideReason = string.Empty;
                StatusMessage = _localization["Common_Saved"];
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

        if (saved && SelectedRun is not null)
        {
            await ShowRunAsync();
        }
    }

    private bool CanRun() => !IsBusy;

    private bool CanShowRun() => !IsBusy && SelectedRun is not null;

    private bool CanBeginOverride() => SelectedItem is not null;

    private bool TryBuildRequest(out AbcRunRequest request)
    {
        request = new AbcRunRequest { Apply = ApplyClasses, WarehouseId = string.IsNullOrWhiteSpace(WarehouseId) ? null : WarehouseId.Trim() };

        if (!string.IsNullOrWhiteSpace(PeriodDays))
        {
            if (!int.TryParse(PeriodDays, NumberStyles.Integer, CultureInfo.CurrentCulture, out var days) || days <= 0)
            {
                return false;
            }

            request.PeriodDays = days;
        }

        // The shares must satisfy 0 < A < B <= 100; a single share is validated by the server against the policy.
        decimal? lower = null;
        decimal? upper = null;

        if (!string.IsNullOrWhiteSpace(AShare))
        {
            if (!CountingText.TryParseDecimal(AShare, out var a) || a <= 0m || a >= MaximumShare)
            {
                return false;
            }

            lower = a;
        }

        if (!string.IsNullOrWhiteSpace(BShare))
        {
            if (!CountingText.TryParseDecimal(BShare, out var b) || b <= 0m || b > MaximumShare)
            {
                return false;
            }

            upper = b;
        }

        if (lower >= upper)
        {
            return false;
        }

        request.AShare = lower;
        request.BShare = upper;
        return true;
    }

    private void Show(AbcRunDto run)
    {
        Items.Clear();
        foreach (var item in run.Items.OrderBy(i => i.Rank))
        {
            Items.Add(new AbcItemRow(item, _localization));
        }

        RunSummary = string.Format(
            CultureInfo.CurrentCulture,
            "{0}: {1} - A {2}, B {3}, C {4} - {5}: {6}",
            _localization["Inv_Col_Items"],
            run.ItemCount,
            run.ClassACount,
            run.ClassBCount,
            run.ClassCCount,
            _localization["Inv_Abc_TotalConsumption"],
            CountingText.Amount(run.TotalConsumptionValue));
    }
}
