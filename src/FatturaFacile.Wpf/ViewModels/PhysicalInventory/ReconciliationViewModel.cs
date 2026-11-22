// <copyright file="ReconciliationViewModel.cs" company="Fattura Facile">
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
/// Reconciliation of a count session: system, counted and final quantities with the variance of every line, recount
/// requests, the summary (gross and net variance, required approval level) and the two approval levels.
/// </summary>
internal sealed partial class ReconciliationViewModel : ObservableObject
{
    private const string PendingApproval = "pending_approval";

    private readonly IInventoryCountingApiClient _client;
    private readonly ILocalizationService _localization;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ReloadCommand))]
    [NotifyCanExecuteChangedFor(nameof(RecountCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSession))]
    [NotifyCanExecuteChangedFor(nameof(ReloadCommand))]
    [NotifyCanExecuteChangedFor(nameof(RecountCommand))]
    private string _sessionId = string.Empty;

    [ObservableProperty]
    private string _sessionTitle = string.Empty;

    [ObservableProperty]
    private string _sessionStatus = string.Empty;

    [ObservableProperty]
    private VarianceLineRow? _selectedLine;

    [ObservableProperty]
    private string _linesText = string.Empty;

    [ObservableProperty]
    private string _countedLinesText = string.Empty;

    [ObservableProperty]
    private string _uncountedLinesText = string.Empty;

    [ObservableProperty]
    private string _differingLinesText = string.Empty;

    [ObservableProperty]
    private string _recountPendingText = string.Empty;

    [ObservableProperty]
    private string _systemValueText = string.Empty;

    [ObservableProperty]
    private string _grossVarianceText = string.Empty;

    [ObservableProperty]
    private string _netVarianceText = string.Empty;

    [ObservableProperty]
    private string _requiredLevelText = string.Empty;

    [ObservableProperty]
    private bool _hasAdjustment;

    [ObservableProperty]
    private string _adjustmentText = string.Empty;

    [ObservableProperty]
    private string _journalEntryText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReconciliationViewModel"/> class.
    /// </summary>
    /// <param name="client">The physical inventory web-api client.</param>
    /// <param name="localization">The localization service.</param>
    public ReconciliationViewModel(IInventoryCountingApiClient client, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(localization);

        _client = client;
        _localization = localization;
        Level1 = new ApprovalStepViewModel(1, localization, step => DecideAsync(step, approve: true), step => DecideAsync(step, approve: false));
        Level2 = new ApprovalStepViewModel(2, localization, step => DecideAsync(step, approve: true), step => DecideAsync(step, approve: false))
        {
            Previous = Level1,
        };
    }

    /// <summary>
    /// Gets the lines of the session with their variance.
    /// </summary>
    public ObservableCollection<VarianceLineRow> Lines { get; } = [];

    /// <summary>
    /// Gets the first approval level (warehouse manager).
    /// </summary>
    public ApprovalStepViewModel Level1 { get; }

    /// <summary>
    /// Gets the second approval level (finance); it can be decided only after level 1 was approved.
    /// </summary>
    public ApprovalStepViewModel Level2 { get; }

    /// <summary>
    /// Gets a value indicating whether a session is loaded.
    /// </summary>
    public bool HasSession => !string.IsNullOrEmpty(SessionId);

    /// <summary>
    /// Shows the reconciliation of a session.
    /// </summary>
    /// <param name="sessionId">The session id.</param>
    /// <param name="title">The session number shown in the heading.</param>
    /// <returns>A task representing the load.</returns>
    public Task OpenAsync(string sessionId, string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);

        SessionId = sessionId;
        SessionTitle = title;
        return ReloadAsync();
    }

    /// <summary>
    /// Reloads the variance report of the open session.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand(CanExecute = nameof(CanReload))]
    public async Task ReloadAsync()
    {
        IsBusy = true;
        Level1.IsBusy = true;
        Level2.IsBusy = true;

        try
        {
            var result = await _client.GetVarianceAsync(SessionId, CancellationToken.None);
            if (result.IsSuccess && result.Value is not null)
            {
                Apply(result.Value);
            }
            else
            {
                StatusMessage = result.IsOffline || result.IsForbidden
                    ? CountingText.Describe(result, _localization)
                    : _localization["Inv_Recon_LoadError"] + " " + CountingText.Describe(result, _localization);
            }
        }
        finally
        {
            Level1.IsBusy = false;
            Level2.IsBusy = false;
            IsBusy = false;
        }
    }

    /// <summary>
    /// Requests a recount of the lines marked in the grid (or of the selected line when none is marked).
    /// </summary>
    /// <returns>A task representing the request.</returns>
    [RelayCommand(CanExecute = nameof(CanRecount))]
    public async Task RecountAsync()
    {
        var ids = RecountTargets().Select(line => line.Id).ToList();
        if (ids.Count == 0)
        {
            return;
        }

        IsBusy = true;
        var done = false;
        var message = string.Empty;

        try
        {
            var result = await _client.RecountAsync(SessionId, ids, CancellationToken.None);
            done = result.IsSuccess;
            message = done ? _localization["Inv_Session_ActionDone"] : CountingText.Describe(result, _localization);
        }
        finally
        {
            IsBusy = false;
        }

        if (done)
        {
            await ReloadAsync();
        }

        StatusMessage = message;
    }

    /// <summary>
    /// Gets the lines a recount request would target: the marked lines, otherwise the selected line when it can be recounted.
    /// </summary>
    /// <returns>The target lines.</returns>
    public IReadOnlyList<VarianceLineRow> RecountTargets()
    {
        var marked = Lines.Where(line => line.IsMarked && line.CanRecount).ToList();

        return marked.Count > 0 ? marked : SelectedTarget();
    }

    /// <summary>
    /// Re-evaluates the recount command after a line was marked or selected.
    /// </summary>
    public void RefreshRecount() => RecountCommand.NotifyCanExecuteChanged();

    partial void OnSelectedLineChanged(VarianceLineRow? value) => RefreshRecount();

    private bool CanReload() => !IsBusy && HasSession;

    private IReadOnlyList<VarianceLineRow> SelectedTarget() => SelectedLine is { CanRecount: true } selected ? [selected] : [];

    private bool CanRecount() => !IsBusy && HasSession && RecountTargets().Count > 0;

    private async Task DecideAsync(ApprovalStepViewModel step, bool approve)
    {
        IsBusy = true;
        Level1.IsBusy = true;
        Level2.IsBusy = true;
        var done = false;
        var message = string.Empty;

        try
        {
            var notes = string.IsNullOrWhiteSpace(step.Notes) ? null : step.Notes.Trim();
            var result = approve
                ? await _client.ApproveAsync(SessionId, step.Level, notes, CancellationToken.None)
                : await _client.RejectAsync(SessionId, step.Level, step.RejectReason.Trim(), CancellationToken.None);
            done = result.IsSuccess;
            message = done ? _localization["Inv_Recon_DecisionDone"] : CountingText.Describe(result, _localization);
        }
        finally
        {
            Level1.IsBusy = false;
            Level2.IsBusy = false;
            IsBusy = false;
        }

        if (done)
        {
            await ReloadAsync();
        }

        StatusMessage = message;
    }

    private void Apply(CountVarianceReportDto report)
    {
        var session = report.Session;
        SessionStatus = CountingText.StatusLabel(session.Status, _localization);
        if (string.IsNullOrEmpty(SessionTitle))
        {
            SessionTitle = session.SessionNumber;
        }

        Lines.Clear();
        foreach (var line in report.Lines)
        {
            VarianceLineRow row = new(line, _localization);
            row.PropertyChanged += (_, args) =>
            {
                if (string.Equals(args.PropertyName, nameof(VarianceLineRow.IsMarked), StringComparison.Ordinal))
                {
                    RefreshRecount();
                }
            };
            Lines.Add(row);
        }

        var summary = report.Summary;
        LinesText = summary.Lines.ToString(CultureInfo.CurrentCulture);
        CountedLinesText = summary.CountedLines.ToString(CultureInfo.CurrentCulture);
        UncountedLinesText = summary.UncountedLines.ToString(CultureInfo.CurrentCulture);
        DifferingLinesText = summary.DifferingLines.ToString(CultureInfo.CurrentCulture);
        RecountPendingText = summary.RecountPendingLines.ToString(CultureInfo.CurrentCulture);
        SystemValueText = CountingText.Amount(summary.SystemValueTotal);
        GrossVarianceText = CountingText.Amount(summary.GrossVarianceValue);
        NetVarianceText = CountingText.SignedAmount(summary.NetVarianceValue);
        RequiredLevelText = DescribeRequiredLevel(summary);

        var awaiting = string.Equals(session.Status, PendingApproval, StringComparison.Ordinal);
        Level1.Apply(report.Approvals.FirstOrDefault(a => a.Level == 1), awaiting);
        Level2.Apply(report.Approvals.FirstOrDefault(a => a.Level == 2), awaiting);
        Level2.Refresh();

        var adjustment = report.Adjustment;
        HasAdjustment = adjustment is not null;
        AdjustmentText = adjustment is null
            ? string.Empty
            : (adjustment.AdjustmentNumber ?? adjustment.Id) + " (" + CountingText.SignedAmount(adjustment.NetValuationChange) + ")";
        JournalEntryText = adjustment?.JournalEntryId ?? string.Empty;
        StatusMessage = string.Empty;
        RefreshRecount();
    }

    private string DescribeRequiredLevel(CountVarianceSummaryDto summary)
    {
        var suggested = summary.SuggestedApprovalLevel is { } level
            ? string.Format(CultureInfo.CurrentCulture, _localization["Inv_Level"], level)
            : string.Empty;

        return summary.WithinTolerance ? _localization["Inv_Recon_WithinTolerance"] : suggested;
    }
}
