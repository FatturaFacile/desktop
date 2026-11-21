// <copyright file="AiCopilotPanelViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.AiCopilot;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the AI copilot panel view model (suggestions, confidence bands, decisions and the offline indicator).
/// </summary>
public sealed class AiCopilotPanelViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();

    private readonly Mock<IAiCopilot> copilot = new();
    private readonly LocalizationService loc = LocalizationService.Instance;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiCopilotPanelViewModelTests"/> class.
    /// </summary>
    public AiCopilotPanelViewModelTests()
    {
        _ = copilot.SetupGet(c => c.IsConfigured).Returns(value: true);
        _ = copilot.Setup(c => c.LoadAsync(Any)).ReturnsAsync(Snapshot());
        _ = copilot.Setup(c => c.RefreshAsync(Any)).ReturnsAsync(new AiRefreshResult(AiRefreshStatus.Ok, AiFlushResult.Empty, Snapshot()));
    }

    /// <summary>
    /// Loading shows the cached rows with their confidence band and queued decision, and the queued count.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ShowsRowsBandsAndQueuedCount()
    {
        var high = Suggestion(0.95);
        var medium = Suggestion(0.75);
        var low = Suggestion(0.30);
        low.QueuedDecision = AiQueuedDecision.Reject;
        var snapshot = Snapshot(2, high, medium, low);
        _ = copilot.Setup(c => c.LoadAsync(Any)).ReturnsAsync(snapshot);
        _ = copilot.Setup(c => c.RefreshAsync(Any)).ReturnsAsync(new AiRefreshResult(AiRefreshStatus.Ok, AiFlushResult.Empty, snapshot));
        var vm = Create();

        await vm.LoadAsync();

        Assert.Equal([AiConfidenceBand.High, AiConfidenceBand.Medium, AiConfidenceBand.Low], vm.Suggestions.Select(r => r.Band).ToArray());
        Assert.True(vm.Suggestions[2].IsQueued);
        Assert.Equal(loc["AiCopilot_StatusQueuedReject"], vm.Suggestions[2].StatusLabel);
        Assert.Equal(2, vm.QueuedCount);
        Assert.True(vm.HasQueuedDecisions);
        Assert.False(vm.IsOffline);
    }

    /// <summary>
    /// The insights show the cash flow summary, the anomalies and the recommendations.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Refresh_ShowsInsights()
    {
        var anomaly = Dto("anomaly");
        var advice = Dto("reorder");
        var insights = new AiInsightsDto(new AiCashFlowDto("CHF", 30, ["2026-11-21"], [10m], [20m], [30m]), 3, [advice], [anomaly]);
        var snapshot = new AiCopilotSnapshot([], insights, DateTime.UtcNow, 0);
        _ = copilot.Setup(c => c.RefreshAsync(Any)).ReturnsAsync(new AiRefreshResult(AiRefreshStatus.Ok, AiFlushResult.Empty, snapshot));
        var vm = Create();

        await vm.RefreshAsync();

        Assert.Equal(2, vm.Insights.Count);
        Assert.Equal(loc["AiCopilot_Anomaly"], vm.Insights[0].Heading);
        Assert.Equal(loc["AiCopilot_Recommendation"], vm.Insights[1].Heading);
        Assert.Contains("CHF", vm.CashFlowSummary, StringComparison.Ordinal);
        Assert.Contains('3', vm.PaymentRiskSummary);
        Assert.NotEmpty(vm.InsightsUpdated);
    }

    /// <summary>
    /// A refresh that cannot reach the server raises the offline indicator and keeps the cached rows.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Refresh_Offline_RaisesIndicator()
    {
        var snapshot = Snapshot(0, Suggestion(0.8));
        _ = copilot.Setup(c => c.RefreshAsync(Any)).ReturnsAsync(new AiRefreshResult(AiRefreshStatus.Offline, AiFlushResult.Empty, snapshot));
        var vm = Create();

        await vm.RefreshAsync();

        Assert.True(vm.IsOffline);
        _ = Assert.Single(vm.Suggestions);
        Assert.Equal(loc["AiCopilot_Offline"], vm.StatusMessage);
        Assert.False(vm.IsBusy);
    }

    /// <summary>
    /// A missing ai_review permission shows a friendly state instead of failing.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Refresh_Forbidden_ShowsFriendlyState()
    {
        _ = copilot.Setup(c => c.RefreshAsync(Any)).ReturnsAsync(new AiRefreshResult(AiRefreshStatus.Forbidden, AiFlushResult.Empty, Snapshot()));
        var vm = Create();

        await vm.RefreshAsync();

        Assert.True(vm.IsForbidden);
        Assert.Equal(loc["AiCopilot_Forbidden"], vm.StatusMessage);
    }

    /// <summary>
    /// Approving sends the typed feedback and reports the result.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Approve_SendsFeedbackAndClearsInputs()
    {
        var suggestion = Suggestion(0.9);
        var vm = await LoadedAsync(suggestion);
        _ = copilot
            .Setup(c => c.DecideAsync(suggestion.Id, It.Is<AiDecisionRequest>(r => r.Approve && r.Text == "go"), Any))
            .ReturnsAsync(new AiDecisionResult(AiDecisionStatus.Sent, Detail: null, Snapshot()));
        vm.SelectedSuggestion = vm.Suggestions[0];
        vm.Feedback = "go";

        await vm.ApproveCommand.ExecuteAsync(parameter: null);

        copilot.Verify(c => c.DecideAsync(suggestion.Id, It.Is<AiDecisionRequest>(r => r.Approve && r.Text == "go"), Any), Times.Once);
        Assert.Equal(loc["AiCopilot_Approved"], vm.StatusMessage);
        Assert.Empty(vm.Feedback);
        Assert.Empty(vm.Suggestions);
    }

    /// <summary>
    /// A rejection needs a reason of at least three characters and sends nothing without it.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Reject_ShortReason_IsNotSent()
    {
        var suggestion = Suggestion(0.9);
        var vm = await LoadedAsync(suggestion);
        vm.SelectedSuggestion = vm.Suggestions[0];
        vm.RejectReason = " ab ";

        await vm.RejectCommand.ExecuteAsync(parameter: null);

        Assert.Equal(loc["AiCopilot_ReasonTooShort"], vm.StatusMessage);
        copilot.Verify(c => c.DecideAsync(It.IsAny<Guid>(), It.IsAny<AiDecisionRequest>(), Any), Times.Never);
    }

    /// <summary>
    /// An offline rejection is queued: the indicator and the queued count show it and the reason is cleared.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Reject_Offline_ShowsQueuedState()
    {
        var suggestion = Suggestion(0.9);
        var vm = await LoadedAsync(suggestion);
        suggestion.QueuedDecision = AiQueuedDecision.Reject;
        _ = copilot
            .Setup(c => c.DecideAsync(suggestion.Id, It.Is<AiDecisionRequest>(r => !r.Approve && r.Text == "wrong"), Any))
            .ReturnsAsync(new AiDecisionResult(AiDecisionStatus.Queued, Detail: null, Snapshot(1, suggestion)));
        vm.SelectedSuggestion = vm.Suggestions[0];
        vm.RejectReason = "wrong";

        await vm.RejectCommand.ExecuteAsync(parameter: null);

        Assert.True(vm.IsOffline);
        Assert.Equal(1, vm.QueuedCount);
        Assert.Equal(loc["AiCopilot_Queued"], vm.StatusMessage);
        Assert.Empty(vm.RejectReason);
        Assert.True(vm.Suggestions[0].IsQueued);
    }

    /// <summary>
    /// An incomplete proposal (422) keeps the inputs and shows the server explanation.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Approve_Incomplete_ShowsServerDetail()
    {
        var suggestion = Suggestion(0.9);
        var vm = await LoadedAsync(suggestion);
        _ = copilot
            .Setup(c => c.DecideAsync(suggestion.Id, It.IsAny<AiDecisionRequest>(), Any))
            .ReturnsAsync(new AiDecisionResult(AiDecisionStatus.Incomplete, "Customer is missing.", Snapshot(0, suggestion)));
        vm.SelectedSuggestion = vm.Suggestions[0];
        vm.Feedback = "try";

        await vm.ApproveCommand.ExecuteAsync(parameter: null);

        Assert.Contains("Customer is missing.", vm.StatusMessage, StringComparison.Ordinal);
        Assert.Equal("try", vm.Feedback);
        _ = Assert.Single(vm.Suggestions);
    }

    /// <summary>
    /// An already decided suggestion (409) is reported and leaves the list.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Approve_AlreadyDecided_ReportsAndRefreshes()
    {
        var suggestion = Suggestion(0.9);
        var vm = await LoadedAsync(suggestion);
        _ = copilot
            .Setup(c => c.DecideAsync(suggestion.Id, It.IsAny<AiDecisionRequest>(), Any))
            .ReturnsAsync(new AiDecisionResult(AiDecisionStatus.AlreadyDecided, Detail: null, Snapshot()));
        vm.SelectedSuggestion = vm.Suggestions[0];

        await vm.ApproveCommand.ExecuteAsync(parameter: null);

        Assert.Equal(loc["AiCopilot_AlreadyDecided"], vm.StatusMessage);
        Assert.Empty(vm.Suggestions);
    }

    /// <summary>
    /// The decision commands need a selected suggestion.
    /// </summary>
    [Fact]
    public void Decide_WithoutSelection_IsDisabled()
    {
        var vm = Create();

        Assert.False(vm.ApproveCommand.CanExecute(parameter: null));
        Assert.False(vm.RejectCommand.CanExecute(parameter: null));
    }

    /// <summary>
    /// Without a configured server the panel says so and works on the local data only.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Refresh_NotConfigured_ShowsLocalOnlyMessage()
    {
        _ = copilot.SetupGet(c => c.IsConfigured).Returns(value: false);
        _ = copilot.Setup(c => c.RefreshAsync(Any)).ReturnsAsync(new AiRefreshResult(AiRefreshStatus.AuthRequired, AiFlushResult.Empty, Snapshot()));
        var vm = Create();

        await vm.RefreshAsync();

        Assert.False(vm.IsAvailable);
        Assert.True(vm.IsOffline);
        Assert.Equal(loc["AiCopilot_NotConfigured"], vm.StatusMessage);
    }

    /// <summary>
    /// A background refresh reloads the cache into the panel.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task BackgroundChange_ReloadsCache()
    {
        var suggestion = Suggestion(0.8);
        var source = new ManualCopilot(Snapshot(0, suggestion));
        var vm = new AiCopilotPanelViewModel(source, loc);

        source.RaiseChanged();
        for (var attempt = 0; attempt < 100 && vm.Suggestions.Count == 0; attempt++)
        {
            await Task.Delay(20, CancellationToken.None);
        }

        _ = Assert.Single(vm.Suggestions);
    }

    private static AiCopilotSnapshot Snapshot(int queued = 0, params AiSuggestion[] suggestions) => new(suggestions, Insights: null, InsightsFetchedAt: null, queued);

    private static AiSuggestion Suggestion(double confidence) => new()
    {
        Id = Guid.NewGuid(),
        Kind = "sales_quote",
        SourceReference = "mail-1",
        Reasoning = "Clear request.",
        Confidence = confidence,
        Amount = 99m,
        Status = "pending",
        Priority = "normal",
    };

    private static AiSuggestionDto Dto(string kind) => new(
        Guid.NewGuid(),
        kind,
        "workflow",
        "invoice",
        "inv-1",
        SourceText: null,
        ProposedPayloadJson: "{}",
        FieldConfidencesJson: "{}",
        Confidence: 0.5,
        Amount: null,
        Reasoning: "Unusual amount",
        Status: "pending",
        Priority: "normal",
        ProviderUsed: null,
        ModelUsed: null,
        ExpiresAt: null,
        CreatedAt: null);

    private AiCopilotPanelViewModel Create() => new(copilot.Object, loc);

    private async Task<AiCopilotPanelViewModel> LoadedAsync(AiSuggestion suggestion)
    {
        var snapshot = Snapshot(0, suggestion);
        _ = copilot.Setup(c => c.LoadAsync(Any)).ReturnsAsync(snapshot);
        _ = copilot.Setup(c => c.RefreshAsync(Any)).ReturnsAsync(new AiRefreshResult(AiRefreshStatus.Ok, AiFlushResult.Empty, snapshot));
        var vm = Create();
        await vm.LoadAsync();
        return vm;
    }

    private sealed class ManualCopilot(AiCopilotSnapshot snapshot) : IAiCopilot
    {
        public event EventHandler? Changed;

        public bool IsConfigured => true;

        public Task<AiCopilotSnapshot> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(snapshot);

        public Task<AiRefreshResult> RefreshAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new AiRefreshResult(AiRefreshStatus.Ok, AiFlushResult.Empty, snapshot));

        public Task<AiDecisionResult> DecideAsync(Guid id, AiDecisionRequest request, CancellationToken cancellationToken) =>
            Task.FromResult(new AiDecisionResult(AiDecisionStatus.Sent, Detail: null, snapshot));

        public void RaiseChanged() => Changed?.Invoke(this, EventArgs.Empty);
    }
}
