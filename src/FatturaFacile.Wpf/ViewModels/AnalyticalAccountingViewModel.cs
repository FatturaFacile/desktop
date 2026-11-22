// <copyright file="AnalyticalAccountingViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.AnalyticalAccounting;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.AnalyticalAccounting;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Drives the industrial and analytical cost accounting view (Mod 018): the cost and profit center hierarchy, the contribution
/// margin income statement, the direct-versus-apportioned comparison, the step-down run history and a manual analytical entry with
/// an optional two-way split, queued offline. It reads the local cache first, so it shows the last synced figures offline; the sync
/// sends the queued entries and pulls fresh figures when the server is reachable.
/// </summary>
internal sealed partial class AnalyticalAccountingViewModel : ObservableObject
{
    private const string NumberFormat = "N2";
    private const string DateTimeFormat = "g";
    private const string NoValue = "-";
    private const string IndentUnit = "    ";
    private const decimal FullPercentage = 100m;

    private static readonly Dictionary<AnalyticalSyncStatus, string> StatusKeys = new()
    {
        [AnalyticalSyncStatus.Synced] = "Analytical_Synced",
        [AnalyticalSyncStatus.Offline] = "Analytical_OfflineUsingCache",
        [AnalyticalSyncStatus.Unauthorized] = "Analytical_SignInRequired",
        [AnalyticalSyncStatus.Forbidden] = "Analytical_Forbidden",
        [AnalyticalSyncStatus.Failed] = "Analytical_SyncFailed",
    };

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;

    private int _pendingCount;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private bool _hasMargin;

    [ObservableProperty]
    private string _lastSyncedText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _pendingText = string.Empty;

    [ObservableProperty]
    private string _revenuesText = NoValue;

    [ObservableProperty]
    private string _variableCostsText = NoValue;

    [ObservableProperty]
    private string _margin1Text = NoValue;

    [ObservableProperty]
    private string _fixedCostsText = NoValue;

    [ObservableProperty]
    private string _margin2Text = NoValue;

    [ObservableProperty]
    private string _indirectCostsText = NoValue;

    [ObservableProperty]
    private string _apportionedCostsText = NoValue;

    [ObservableProperty]
    private string _operatingResultText = NoValue;

    [ObservableProperty]
    private string _entryAmountText = string.Empty;

    [ObservableProperty]
    private string _selectedCostElement = AnalyticalContract.CostElementTypes[0];

    [ObservableProperty]
    private AnalyticalCenterRow? _selectedCenter;

    [ObservableProperty]
    private AnalyticalCenterRow? _selectedSecondCenter;

    [ObservableProperty]
    private string _splitPercentText = string.Empty;

    [ObservableProperty]
    private string _entryMessage = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyticalAccountingViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    public AnalyticalAccountingViewModel(ISender sender, ILocalizationService localization)
    {
        _sender = sender;
        _localization = localization;
        LastSyncedText = _localization["Analytical_NeverSynced"];
    }

    /// <summary>
    /// Gets the cost element types of the manual entry.
    /// </summary>
    public IReadOnlyList<string> CostElements => AnalyticalContract.CostElementTypes;

    /// <summary>
    /// Gets the center hierarchy, depth first.
    /// </summary>
    internal ObservableCollection<AnalyticalCenterRow> Centers { get; } = [];

    /// <summary>
    /// Gets the direct-versus-apportioned comparison.
    /// </summary>
    internal ObservableCollection<AnalyticalComparisonRow> Comparison { get; } = [];

    /// <summary>
    /// Gets the step-down run history.
    /// </summary>
    internal ObservableCollection<AnalyticalRunRow> Runs { get; } = [];

    /// <summary>
    /// Loads the cached overview and, when nothing was ever synced, tries to pull it.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            var overview = await _sender.Send(new GetAnalyticalOverviewQuery());
            if (overview.SyncedAtUtc is null)
            {
                await PullAsync();
                overview = await _sender.Send(new GetAnalyticalOverviewQuery());
            }

            Show(overview);
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Sends the queued entries and refreshes the cache.
    /// </summary>
    /// <returns>A task representing the sync.</returns>
    [RelayCommand]
    public async Task SyncAsync()
    {
        IsBusy = true;

        try
        {
            await PullAsync();
            Show(await _sender.Send(new GetAnalyticalOverviewQuery()));
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Queues the manual analytical entry typed in the form; with a second center and a percentage the line is split in two.
    /// </summary>
    /// <returns>A task representing the queuing.</returns>
    [RelayCommand]
    public async Task QueueEntryAsync()
    {
        if (!decimal.TryParse(EntryAmountText, NumberStyles.Number, CultureInfo.CurrentCulture, out var amount) || SelectedCenter is null)
        {
            EntryMessage = _localization["Analytical_InvalidAmount"];

            return;
        }

        var line = BuildLine(amount);
        if (line is null)
        {
            EntryMessage = _localization["Analytical_InvalidAmount"];

            return;
        }

        var draft = new AnalyticalEntryDraft(DateOnly.FromDateTime(DateTime.Today), Description: null, [line]);
        var result = await _sender.Send(new QueueAnalyticalEntryCommand(draft));
        if (result.IsQueued)
        {
            EntryAmountText = string.Empty;
            SplitPercentText = string.Empty;
            SelectedSecondCenter = null;
            EntryMessage = _localization["Analytical_EntryQueued"];
            SetPending(_pendingCount + 1);
        }
        else
        {
            EntryMessage = string.Join(' ', result.Errors.Select(e => _localization[e]));
        }
    }

    private static string Indent(int depth) => string.Concat(Enumerable.Repeat(IndentUnit, depth));

    private static string Format(decimal value) => value.ToString(NumberFormat, CultureInfo.CurrentCulture);

    private static AllocationVector Vector(AnalyticalCenterRow center, decimal percentage) =>
        center.IsCostCenter
            ? new AllocationVector(CostCenterId: center.Id, Percentage: percentage)
            : new AllocationVector(ProfitCenterId: center.Id, Percentage: percentage);

    private AnalyticalEntryLineDraft? BuildLine(decimal amount)
    {
        var first = SelectedCenter!;
        if (SelectedSecondCenter is null || string.IsNullOrWhiteSpace(SplitPercentText))
        {
            return first.IsCostCenter
                ? new AnalyticalEntryLineDraft(amount, SelectedCostElement, CostCenterId: first.Id)
                : new AnalyticalEntryLineDraft(amount, SelectedCostElement, ProfitCenterId: first.Id);
        }

        if (!decimal.TryParse(SplitPercentText, NumberStyles.Number, CultureInfo.CurrentCulture, out var percent) || percent <= 0m || percent >= FullPercentage)
        {
            return null;
        }

        return new AnalyticalEntryLineDraft(amount, SelectedCostElement, Vectors: [Vector(first, percent), Vector(SelectedSecondCenter, FullPercentage - percent)]);
    }

    private void SetPending(int count)
    {
        _pendingCount = count;
        PendingText = count == 0 ? string.Empty : string.Format(CultureInfo.CurrentCulture, _localization["Analytical_Pending"], count);
    }

    private async Task PullAsync()
    {
        var result = await _sender.Send(new SyncAnalyticalAccountingCommand());
        IsOffline = result.Status == AnalyticalSyncStatus.Offline;
        StatusMessage = _localization[StatusKeys[result.Status]];
    }

    private void Show(AnalyticalOverview overview)
    {
        LastSyncedText = overview.SyncedAtUtc is { } at
            ? string.Format(CultureInfo.CurrentCulture, _localization["Analytical_LastSynced"], at.ToLocalTime().ToString(DateTimeFormat, CultureInfo.CurrentCulture))
            : _localization["Analytical_NeverSynced"];
        SetPending(overview.PendingOperations);

        Centers.Clear();
        foreach (var node in AnalyticalParser.Flatten(overview.Centers))
        {
            Centers.Add(new AnalyticalCenterRow(
                node.Id,
                Indent(node.Depth) + node.Code + " " + node.Name,
                _localization["Analytical_Type_" + node.CenterType],
                _localization["Analytical_Category_" + node.Category],
                node.ManagerName ?? NoValue,
                _localization[node.IsActive ? "Analytical_Active" : "Analytical_Inactive"],
                string.Equals(node.CenterType, "COST_CENTER", StringComparison.Ordinal)));
        }

        Comparison.Clear();
        foreach (var row in overview.Comparison)
        {
            Comparison.Add(new AnalyticalComparisonRow(
                Indent(row.Depth) + row.Code + " " + row.Name,
                Format(row.DirectCost),
                Format(row.ApportionedIn),
                Format(row.ApportionedOut),
                Format(row.NetCost),
                Format(row.Variance),
                Format(row.SharePercent) + " %"));
        }

        Runs.Clear();
        foreach (var run in overview.Runs)
        {
            Runs.Add(new AnalyticalRunRow(run.PeriodKey, _localization["Analytical_Run_" + run.Status], Format(run.TotalPool), Format(run.TotalAllocated)));
        }

        ShowMargin(overview.Margin);
    }

    private void ShowMargin(ContributionMarginStatement? margin)
    {
        HasMargin = margin is not null;
        RevenuesText = margin is null ? NoValue : Format(margin.OperatingRevenues);
        VariableCostsText = margin is null ? NoValue : Format(margin.DirectVariableCosts);
        Margin1Text = margin is null ? NoValue : Format(margin.ContributionMargin1) + " (" + Format(margin.ContributionMargin1Percent) + " %)";
        FixedCostsText = margin is null ? NoValue : Format(margin.DirectFixedCosts);
        Margin2Text = margin is null ? NoValue : Format(margin.ContributionMargin2) + " (" + Format(margin.ContributionMargin2Percent) + " %)";
        IndirectCostsText = margin is null ? NoValue : Format(margin.IndirectCosts);
        ApportionedCostsText = margin is null ? NoValue : Format(margin.ApportionedCosts);
        OperatingResultText = margin is null ? NoValue : Format(margin.OperatingResult) + " (" + Format(margin.OperatingResultPercent) + " %)";
    }
}
