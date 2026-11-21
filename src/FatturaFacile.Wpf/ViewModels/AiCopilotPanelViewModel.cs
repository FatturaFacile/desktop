// <copyright file="AiCopilotPanelViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.AiCopilot;
using FatturaFacile.Application.Common.Interfaces;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Drives the AI copilot panel: pending Human-in-the-Loop suggestions, predictive insights, approve or reject
/// decisions and the offline indicator with the count of decisions queued for the next sync.
/// </summary>
internal sealed partial class AiCopilotPanelViewModel : ObservableObject
{
    private const int MinimumReasonLength = 3;

    private readonly IAiCopilot _copilot;
    private readonly ILocalizationService _localization;
    private readonly TaskScheduler _uiScheduler = SynchronizationContext.Current is null ? TaskScheduler.Default : TaskScheduler.FromCurrentSynchronizationContext();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApproveCommand))]
    [NotifyCanExecuteChangedFor(nameof(RejectCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private bool _isForbidden;

    [ObservableProperty]
    private int _queuedCount;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _cashFlowSummary = string.Empty;

    [ObservableProperty]
    private string _paymentRiskSummary = string.Empty;

    [ObservableProperty]
    private string _insightsUpdated = string.Empty;

    [ObservableProperty]
    private string _feedback = string.Empty;

    [ObservableProperty]
    private string _rejectReason = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApproveCommand))]
    [NotifyCanExecuteChangedFor(nameof(RejectCommand))]
    private AiSuggestionRow? _selectedSuggestion;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiCopilotPanelViewModel"/> class.
    /// </summary>
    /// <param name="copilot">The AI copilot.</param>
    /// <param name="localization">The localization service.</param>
    public AiCopilotPanelViewModel(IAiCopilot copilot, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(copilot);
        ArgumentNullException.ThrowIfNull(localization);

        _copilot = copilot;
        _localization = localization;
        _copilot.Changed += (_, _) => _ = ReloadAsync();
    }

    /// <summary>
    /// Gets the pending suggestions, most urgent first.
    /// </summary>
    public ObservableCollection<AiSuggestionRow> Suggestions { get; } = [];

    /// <summary>
    /// Gets the anomaly alerts and recommendations.
    /// </summary>
    public ObservableCollection<AiInsightRow> Insights { get; } = [];

    /// <summary>
    /// Gets a value indicating whether the web-api integration is configured.
    /// </summary>
    public bool IsAvailable => _copilot.IsConfigured;

    /// <summary>
    /// Gets a value indicating whether decisions are waiting to be pushed.
    /// </summary>
    public bool HasQueuedDecisions => QueuedCount > 0;

    /// <summary>
    /// Gets the localized text with the number of queued decisions.
    /// </summary>
    public string QueuedText => string.Format(CultureInfo.CurrentCulture, _localization["AiCopilot_QueuedCount"], QueuedCount);

    /// <summary>
    /// Shows the cached data immediately, then refreshes from the server.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        Apply(await _copilot.LoadAsync(CancellationToken.None));
        await RefreshAsync();
    }

    /// <summary>
    /// Pushes the queued decisions and refreshes suggestions and insights from the server.
    /// </summary>
    /// <returns>A task representing the refresh.</returns>
    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsBusy = true;

        try
        {
            var result = await _copilot.RefreshAsync(CancellationToken.None);
            Apply(result.Snapshot);
            DescribeRefresh(result);
        }
        catch (OperationCanceledException)
        {
            StatusMessage = string.Empty;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Approves the selected suggestion (queued locally while offline).
    /// </summary>
    /// <returns>A task representing the decision.</returns>
    [RelayCommand(CanExecute = nameof(CanDecide))]
    public Task ApproveAsync() => DecideAsync(approve: true);

    /// <summary>
    /// Rejects the selected suggestion with the typed reason (queued locally while offline).
    /// </summary>
    /// <returns>A task representing the decision.</returns>
    [RelayCommand(CanExecute = nameof(CanDecide))]
    public Task RejectAsync() => DecideAsync(approve: false);

    partial void OnQueuedCountChanged(int value)
    {
        OnPropertyChanged(nameof(HasQueuedDecisions));
        OnPropertyChanged(nameof(QueuedText));
    }

    private bool CanDecide() => SelectedSuggestion is not null && !IsBusy;

    private async Task DecideAsync(bool approve)
    {
        if (SelectedSuggestion is not { } row)
        {
            StatusMessage = _localization["AiCopilot_NoSelection"];
            return;
        }

        var text = approve ? Feedback : RejectReason;
        if (!approve && text.Trim().Length < MinimumReasonLength)
        {
            StatusMessage = _localization["AiCopilot_ReasonTooShort"];
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _copilot.DecideAsync(row.Id, new AiDecisionRequest(approve, text), CancellationToken.None);
            Apply(result.Snapshot);
            DescribeDecision(result, approve);
        }
        catch (OperationCanceledException)
        {
            StatusMessage = string.Empty;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ReloadAsync()
    {
        try
        {
            var snapshot = await _copilot.LoadAsync(CancellationToken.None).ConfigureAwait(false);
            await RunOnUiAsync(() => Apply(snapshot)).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // The application is shutting down: nothing left to show.
        }
    }

    private Task RunOnUiAsync(Action action) =>
        Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.DenyChildAttach, _uiScheduler);

    private void Apply(AiCopilotSnapshot snapshot)
    {
        var selectedId = SelectedSuggestion?.Id;

        Suggestions.Clear();
        foreach (var suggestion in snapshot.Suggestions)
        {
            Suggestions.Add(new AiSuggestionRow(suggestion, _localization));
        }

        SelectedSuggestion = selectedId is { } id ? Suggestions.FirstOrDefault(row => row.Id == id) : null;
        QueuedCount = snapshot.QueuedCount;
        ApplyInsights(snapshot);
    }

    private void ApplyInsights(AiCopilotSnapshot snapshot)
    {
        Insights.Clear();
        var insights = snapshot.Insights;
        if (insights is null)
        {
            CashFlowSummary = string.Empty;
            PaymentRiskSummary = string.Empty;
            InsightsUpdated = string.Empty;
            return;
        }

        foreach (var anomaly in insights.Anomalies)
        {
            Insights.Add(ToRow("AiCopilot_Anomaly", anomaly));
        }

        foreach (var recommendation in insights.Recommendations)
        {
            Insights.Add(ToRow("AiCopilot_Recommendation", recommendation));
        }

        CashFlowSummary = DescribeCashFlow(insights.CashFlow);
        PaymentRiskSummary = Format("AiCopilot_PaymentRisk", insights.PaymentRiskCount.ToString(CultureInfo.CurrentCulture));
        InsightsUpdated = snapshot.InsightsFetchedAt is { } fetched
            ? Format("AiCopilot_UpdatedAt", DateTime.SpecifyKind(fetched, DateTimeKind.Utc).ToLocalTime().ToString("g", CultureInfo.CurrentCulture))
            : string.Empty;
    }

    private AiInsightRow ToRow(string headingKey, AiSuggestionDto dto)
    {
        var kindKey = "AiCopilot_Kind_" + dto.Kind;
        var kind = _localization[kindKey];
        var title = string.Equals(kind, kindKey, StringComparison.Ordinal) ? dto.Kind : kind;
        var band = _localization["AiCopilot_Band_" + AiConfidence.BandOf(dto.Confidence)];
        return new AiInsightRow(_localization[headingKey], $"{title} - {dto.SourceReference}", dto.Reasoning ?? string.Empty, band);
    }

    private string DescribeCashFlow(AiCashFlowDto? cashFlow) => cashFlow is null
        ? _localization["AiCopilot_CashFlowNone"]
        : string.Format(
            CultureInfo.CurrentCulture,
            _localization["AiCopilot_CashFlow"],
            cashFlow.Currency ?? string.Empty,
            cashFlow.HorizonDays,
            cashFlow.P50.LastOrDefault().ToString("N2", CultureInfo.CurrentCulture),
            cashFlow.P10.LastOrDefault().ToString("N2", CultureInfo.CurrentCulture),
            cashFlow.P90.LastOrDefault().ToString("N2", CultureInfo.CurrentCulture));

    private void DescribeRefresh(AiRefreshResult result)
    {
        IsForbidden = result.Status == AiRefreshStatus.Forbidden;
        IsOffline = result.Status is AiRefreshStatus.Offline or AiRefreshStatus.AuthRequired || !IsAvailable;
        StatusMessage = IsAvailable ? RefreshMessage(result) : _localization["AiCopilot_NotConfigured"];
    }

    private string RefreshMessage(AiRefreshResult result) => result.Status switch
    {
        AiRefreshStatus.Ok => Format("AiCopilot_Loaded", result.Snapshot.Suggestions.Count.ToString(CultureInfo.CurrentCulture)),
        AiRefreshStatus.Offline => _localization["AiCopilot_Offline"],
        AiRefreshStatus.Forbidden => _localization["AiCopilot_Forbidden"],
        AiRefreshStatus.AuthRequired => _localization["AiCopilot_AuthRequired"],
        _ => string.Empty,
    };

    private void DescribeDecision(AiDecisionResult result, bool approve)
    {
        var decided = result.Status is AiDecisionStatus.Sent or AiDecisionStatus.Queued or AiDecisionStatus.AlreadyDecided or AiDecisionStatus.NotFound;
        if (decided)
        {
            Feedback = string.Empty;
            RejectReason = string.Empty;
        }

        if (result.Status == AiDecisionStatus.Queued)
        {
            IsOffline = true;
        }

        StatusMessage = result.Status switch
        {
            AiDecisionStatus.Sent => _localization[approve ? "AiCopilot_Approved" : "AiCopilot_Rejected"],
            AiDecisionStatus.Queued => _localization["AiCopilot_Queued"],
            AiDecisionStatus.AlreadyDecided => _localization["AiCopilot_AlreadyDecided"],
            AiDecisionStatus.NotFound => _localization["AiCopilot_NotFound"],
            AiDecisionStatus.Incomplete => Format("AiCopilot_Incomplete", result.Detail ?? string.Empty),
            AiDecisionStatus.Refused => Format("AiCopilot_Refused", result.Detail ?? string.Empty),
            _ => string.Empty,
        };
    }

    private string Format(string key, string argument) =>
        string.Format(CultureInfo.CurrentCulture, _localization[key], argument);
}
