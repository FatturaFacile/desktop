// <copyright file="PhysicalInventoryViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0002, MA0003, MA0006 // Test data uses positional literals and plain string comparisons by design.
using System.Globalization;
using FatturaFacile.Application.InventoryCounting;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels.PhysicalInventory;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the physical inventory view models: variance display, the approval step state machine, command
/// availability and the request each command sends.
/// </summary>
public sealed class PhysicalInventoryViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();

    private readonly Mock<IInventoryCountingApiClient> client = new();
    private readonly LocalizationService loc = LocalizationService.Instance;

    /// <summary>
    /// A line with a defined percentage shows it signed, an undefined percentage shows the localized text and an
    /// uncounted line shows nothing.
    /// </summary>
    [Fact]
    public void VarianceLine_PercentDisplay_HandlesUndefinedAndUncounted()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        var loss = new VarianceLineRow(Line("l1", system: 10m, counted: 8m, percent: -20m), loc);
        var undefined = new VarianceLineRow(Line("l2", system: 0m, counted: 3m, percent: null, undefined: true), loc);
        var uncounted = new VarianceLineRow(new CountVarianceLineDto { Id = "l3", IsCounted = false, SystemQuantity = 5m }, loc);

        Assert.Equal("-20.00 %", loss.VariancePercentText);
        Assert.Equal("-2", loss.VarianceQuantityText);
        Assert.True(loss.IsLoss);
        Assert.False(loss.IsGain);
        Assert.Equal(loc["Inv_Recon_Undefined"], undefined.VariancePercentText);
        Assert.Equal("+3", undefined.VarianceQuantityText);
        Assert.True(undefined.IsGain);
        Assert.Equal(string.Empty, uncounted.VariancePercentText);
        Assert.Equal(string.Empty, uncounted.VarianceQuantityText);
        Assert.False(uncounted.HasVariance);
        Assert.False(uncounted.CanRecount);
    }

    /// <summary>
    /// A line waiting for a recount shows its round and cannot be recounted again.
    /// </summary>
    [Fact]
    public void VarianceLine_RecountPending_ShowsRoundAndBlocksNewRequest()
    {
        var line = Line("l1", system: 10m, counted: 7m, percent: -30m);
        line = line with { RecountRequired = true, RecountRound = 2 };

        var row = new VarianceLineRow(line, loc);

        Assert.Contains(loc["Inv_Recon_RecountPending"], row.RecountText, StringComparison.Ordinal);
        Assert.Contains("2", row.RecountText, StringComparison.Ordinal);
        Assert.False(row.CanRecount);
    }

    /// <summary>
    /// Level 1 can be decided as soon as the session waits for approval, level 2 only after level 1 was approved.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Approval_Level2_IsEnabledOnlyAfterLevel1IsApproved()
    {
        _ = client.SetupSequence(c => c.GetVarianceAsync("s1", Any))
            .ReturnsAsync(Ok(Report("pending_approval", Approval(1, "pending"), Approval(2, "pending"))))
            .ReturnsAsync(Ok(Report("pending_approval", Approval(1, "approved"), Approval(2, "pending"))));
        var vm = new ReconciliationViewModel(client.Object, loc);

        await vm.OpenAsync("s1", "CNT-1");

        Assert.True(vm.Level1.IsActionable);
        Assert.True(vm.Level1.ApproveCommand.CanExecute(null));
        Assert.False(vm.Level2.IsActionable);
        Assert.False(vm.Level2.ApproveCommand.CanExecute(null));
        Assert.True(vm.Level2.IsWaitingForPrevious);

        await vm.ReloadAsync();

        Assert.False(vm.Level1.IsActionable);
        Assert.True(vm.Level1.IsApproved);
        Assert.True(vm.Level2.IsActionable);
        Assert.True(vm.Level2.ApproveCommand.CanExecute(null));
        Assert.False(vm.Level2.IsWaitingForPrevious);
    }

    /// <summary>
    /// A session that is not waiting for approval, or a level the policy does not require, cannot be decided.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Approval_IsDisabled_WhenSessionIsNotPendingOrLevelIsNotRequired()
    {
        _ = client.SetupSequence(c => c.GetVarianceAsync("s1", Any))
            .ReturnsAsync(Ok(Report("in_progress", Approval(1, "pending"))))
            .ReturnsAsync(Ok(Report("pending_approval", Approval(1, "pending"))));
        var vm = new ReconciliationViewModel(client.Object, loc);

        await vm.OpenAsync("s1", "CNT-1");

        Assert.False(vm.Level1.IsActionable);
        Assert.False(vm.Level1.ApproveCommand.CanExecute(null));

        await vm.ReloadAsync();

        Assert.True(vm.Level1.IsActionable);
        Assert.False(vm.Level2.IsPresent);
        Assert.False(vm.Level2.IsActionable);
    }

    /// <summary>
    /// A rejection needs a reason; the reason and the level are sent and the report is reloaded.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Reject_RequiresReason_AndSendsLevelAndReason()
    {
        _ = client.Setup(c => c.GetVarianceAsync("s1", Any)).ReturnsAsync(Ok(Report("pending_approval", Approval(1, "pending"))));
        _ = client.Setup(c => c.RejectAsync("s1", 1, "bin was not counted", Any)).ReturnsAsync(Ok(true));
        var vm = new ReconciliationViewModel(client.Object, loc);
        await vm.OpenAsync("s1", "CNT-1");

        Assert.False(vm.Level1.RejectCommand.CanExecute(null));

        vm.Level1.RejectReason = "   ";
        Assert.False(vm.Level1.RejectCommand.CanExecute(null));

        vm.Level1.RejectReason = "bin was not counted";
        Assert.True(vm.Level1.RejectCommand.CanExecute(null));

        await vm.Level1.RejectCommand.ExecuteAsync(null);

        client.Verify(c => c.RejectAsync("s1", 1, "bin was not counted", Any), Times.Once);
        client.Verify(c => c.GetVarianceAsync("s1", Any), Times.Exactly(2));
        Assert.Equal(loc["Inv_Recon_DecisionDone"], vm.StatusMessage);
    }

    /// <summary>
    /// Approve sends the notes (null when blank).
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Approve_SendsNotes()
    {
        _ = client.Setup(c => c.GetVarianceAsync("s1", Any)).ReturnsAsync(Ok(Report("pending_approval", Approval(1, "pending"))));
        _ = client.Setup(c => c.ApproveAsync("s1", 1, It.IsAny<string?>(), Any)).ReturnsAsync(Ok(true));
        var vm = new ReconciliationViewModel(client.Object, loc);
        await vm.OpenAsync("s1", "CNT-1");

        vm.Level1.Notes = "  checked with the counter  ";
        await vm.Level1.ApproveCommand.ExecuteAsync(null);

        client.Verify(c => c.ApproveAsync("s1", 1, "checked with the counter", Any), Times.Once);
    }

    /// <summary>
    /// A failed decision shows the server message and keeps the report as it was.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Approve_Failure_ShowsErrors()
    {
        _ = client.Setup(c => c.GetVarianceAsync("s1", Any)).ReturnsAsync(Ok(Report("pending_approval", Approval(1, "pending"))));
        _ = client.Setup(c => c.ApproveAsync("s1", 1, null, Any))
            .ReturnsAsync(InventoryCountingResults.Fail<bool>(422, "invalid", ["Segregation of duties violated."]));
        var vm = new ReconciliationViewModel(client.Object, loc);
        await vm.OpenAsync("s1", "CNT-1");

        await vm.Level1.ApproveCommand.ExecuteAsync(null);

        Assert.Equal("Segregation of duties violated.", vm.StatusMessage);
        client.Verify(c => c.GetVarianceAsync("s1", Any), Times.Once);
    }

    /// <summary>
    /// The summary, the required level, the adjustment and the journal entry are shown after loading.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Open_ShowsSummaryAdjustmentAndJournalEntry()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        var report = Report("reconciled", Approval(1, "approved"), Approval(2, "approved")) with
        {
            Summary = new CountVarianceSummaryDto { Lines = 4, CountedLines = 3, UncountedLines = 1, DifferingLines = 2, GrossVarianceValue = 150.5m, NetVarianceValue = -50.25m, SuggestedApprovalLevel = 2 },
            Adjustment = new CountAdjustmentDto { Id = "adj1", AdjustmentNumber = "ADJ-7", NetValuationChange = -50.25m, JournalEntryId = "je-1" },
        };
        _ = client.Setup(c => c.GetVarianceAsync("s1", Any)).ReturnsAsync(Ok(report));
        var vm = new ReconciliationViewModel(client.Object, loc);

        await vm.OpenAsync("s1", "CNT-1");

        Assert.Equal("4", vm.LinesText);
        Assert.Equal("150.50", vm.GrossVarianceText);
        Assert.Equal("-50.25", vm.NetVarianceText);
        Assert.Equal(string.Format(CultureInfo.CurrentCulture, loc["Inv_Level"], 2), vm.RequiredLevelText);
        Assert.True(vm.HasAdjustment);
        Assert.Equal("ADJ-7 (-50.25)", vm.AdjustmentText);
        Assert.Equal("je-1", vm.JournalEntryText);
        Assert.Equal(2, vm.Lines.Count);
        Assert.False(vm.Level1.IsActionable);
        Assert.True(vm.Level2.IsApproved);
    }

    /// <summary>
    /// Within tolerance, the summary shows the tolerance text instead of an approval level.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Open_WithinTolerance_ShowsToleranceText()
    {
        var report = Report("pending_approval") with { Summary = new CountVarianceSummaryDto { WithinTolerance = true } };
        _ = client.Setup(c => c.GetVarianceAsync("s1", Any)).ReturnsAsync(Ok(report));
        var vm = new ReconciliationViewModel(client.Object, loc);

        await vm.OpenAsync("s1", "CNT-1");

        Assert.Equal(loc["Inv_Recon_WithinTolerance"], vm.RequiredLevelText);
    }

    /// <summary>
    /// The recount command targets the marked lines (or the selected line) and is disabled without a target.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Recount_SendsTheMarkedLines()
    {
        _ = client.Setup(c => c.GetVarianceAsync("s1", Any)).ReturnsAsync(Ok(Report("in_progress")));
        _ = client.Setup(c => c.RecountAsync("s1", It.IsAny<IReadOnlyList<string>>(), Any)).ReturnsAsync(Ok(true));
        var vm = new ReconciliationViewModel(client.Object, loc);
        await vm.OpenAsync("s1", "CNT-1");

        Assert.False(vm.RecountCommand.CanExecute(null));

        vm.Lines[1].IsMarked = true;
        Assert.True(vm.RecountCommand.CanExecute(null));

        await vm.RecountCommand.ExecuteAsync(null);

        client.Verify(c => c.RecountAsync("s1", It.Is<IReadOnlyList<string>>(ids => ids.Count == 1 && ids[0] == "l2"), Any), Times.Once);
    }

    /// <summary>
    /// Freeze, unfreeze, assign, cancel and submit follow the state of the selected session; releasing the freeze needs a reason.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sessions_CommandAvailability_FollowsSessionState()
    {
        _ = client.Setup(c => c.GetSessionsAsync(Any)).ReturnsAsync(Ok<IReadOnlyList<CountSessionDto>>(
        [
            Session("open", "in_progress", frozen: false),
            Session("frozen", "in_progress", frozen: true),
            Session("done", "reconciled", frozen: false),
        ]));
        _ = client.Setup(c => c.UnfreezeAsync("frozen", "count finished early", Any)).ReturnsAsync(Ok(true));
        var vm = new CountSessionsViewModel(client.Object, loc);
        await vm.LoadAsync();

        Assert.False(vm.FreezeCommand.CanExecute(null));

        vm.SelectedSession = vm.Sessions[0];
        Assert.True(vm.FreezeCommand.CanExecute(null));
        Assert.False(vm.BeginUnfreezeCommand.CanExecute(null));
        Assert.True(vm.SubmitDifferencesCommand.CanExecute(null));
        Assert.True(vm.CancelSessionCommand.CanExecute(null));
        Assert.True(vm.AssignCommand.CanExecute(null));

        vm.SelectedSession = vm.Sessions[1];
        Assert.False(vm.FreezeCommand.CanExecute(null));
        Assert.True(vm.BeginUnfreezeCommand.CanExecute(null));
        vm.BeginUnfreezeCommand.Execute(null);
        Assert.True(vm.IsUnfreezeOpen);
        Assert.False(vm.ConfirmUnfreezeCommand.CanExecute(null));
        vm.UnfreezeReason = "count finished early";
        Assert.True(vm.ConfirmUnfreezeCommand.CanExecute(null));

        await vm.ConfirmUnfreezeCommand.ExecuteAsync(null);

        client.Verify(c => c.UnfreezeAsync("frozen", "count finished early", Any), Times.Once);
        Assert.False(vm.IsUnfreezeOpen);

        vm.SelectedSession = vm.Sessions[2];
        Assert.False(vm.FreezeCommand.CanExecute(null));
        Assert.False(vm.SubmitDifferencesCommand.CanExecute(null));
        Assert.False(vm.CancelSessionCommand.CanExecute(null));
        Assert.True(vm.OpenReconciliationCommand.CanExecute(null));
        Assert.True(vm.Sessions[1].IsFrozen);
        Assert.Equal(loc["Inv_Session_StatusReconciled"], vm.Sessions[2].StatusLabel);
    }

    /// <summary>
    /// The create form needs a warehouse and sends the entered values, with the bins split into a list.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Create_RequiresWarehouse_AndBuildsTheRequest()
    {
        CreateCountSessionRequest? sent = null;
        _ = client.Setup(c => c.CreateSessionAsync(It.IsAny<CreateCountSessionRequest>(), Any))
            .Callback<CreateCountSessionRequest, CancellationToken>((request, _) => sent = request)
            .ReturnsAsync(Ok(new CountSessionDto { Id = "n1", SessionNumber = "CNT-5", Status = "in_progress" }));
        _ = client.Setup(c => c.GetSessionsAsync(Any)).ReturnsAsync(Ok<IReadOnlyList<CountSessionDto>>([]));
        var vm = new CountSessionsViewModel(client.Object, loc);
        vm.BeginCreateCommand.Execute(null);

        Assert.True(vm.IsCreateOpen);
        Assert.False(vm.CreateCommand.CanExecute(null));

        vm.NewWarehouseId = " w1 ";
        vm.NewCountType = vm.CountTypes.Single(t => t.Value == "zone_sweep");
        vm.NewScheduledDate = new DateTime(2026, 12, 1);
        vm.NewBinIds = "b1, b2;b3";
        vm.NewFreezesStock = true;
        Assert.True(vm.CreateCommand.CanExecute(null));

        await vm.CreateCommand.ExecuteAsync(null);

        Assert.NotNull(sent);
        Assert.Equal("w1", sent.WarehouseId);
        Assert.Equal("zone_sweep", sent.CountType);
        Assert.Equal("2026-12-01", sent.ScheduledDate);
        Assert.Equal(new[] { "b1", "b2", "b3" }, sent.BinIds!.ToArray());
        Assert.True(sent.FreezesStock);
        Assert.True(sent.IsBlind);
        Assert.Null(sent.Notes);
        Assert.False(vm.IsCreateOpen);
    }

    /// <summary>
    /// The reconciliation request of a session switches the shell to the reconciliation tab and loads the report.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task OpenReconciliation_SwitchesTabAndLoads()
    {
        _ = client.Setup(c => c.GetVarianceAsync("s1", Any)).ReturnsAsync(Ok(Report("pending_approval", Approval(1, "pending"))));
        var vm = new PhysicalInventoryViewModel(client.Object, loc);

        await vm.ShowReconciliationAsync(new CountSessionRow(Session("s1", "pending_approval", frozen: false), loc));

        Assert.Equal(PhysicalInventoryViewModel.ReconciliationTab, vm.SelectedTabIndex);
        Assert.True(vm.Reconciliation.HasSession);
        Assert.Equal("CNT-s1", vm.Reconciliation.SessionTitle);
        Assert.True(vm.Reconciliation.Level1.IsActionable);
    }

    /// <summary>
    /// The override can be confirmed only with a reason and a valid class; clearing needs only the reason.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task AbcOverride_RequiresReason()
    {
        _ = client.Setup(c => c.GetAbcRunsAsync(Any)).ReturnsAsync(Ok<IReadOnlyList<AbcRunDto>>([new AbcRunDto { Id = "r1", ItemCount = 2 }]));
        _ = client.Setup(c => c.GetAbcRunAsync("r1", Any)).ReturnsAsync(Ok(Run()));
        _ = client.Setup(c => c.OverrideAbcClassAsync("i1", "A", "strategic item", Any)).ReturnsAsync(Ok(true));
        var vm = new AbcAnalysisViewModel(client.Object, loc);
        await vm.LoadAsync();

        Assert.Equal(2, vm.Items.Count);
        Assert.Equal(new[] { "S1", "S2" }, vm.Items.Select(i => i.Sku).ToArray());
        Assert.False(vm.BeginOverrideCommand.CanExecute(null));

        vm.SelectedItem = vm.Items[0];
        Assert.True(vm.BeginOverrideCommand.CanExecute(null));
        vm.BeginOverrideCommand.Execute(null);

        Assert.True(vm.IsOverrideOpen);
        Assert.False(vm.ConfirmOverrideCommand.CanExecute(null));

        vm.OverrideClass = "A";
        vm.OverrideReason = "  ";
        Assert.False(vm.ConfirmOverrideCommand.CanExecute(null));

        vm.OverrideReason = "strategic item";
        Assert.True(vm.ConfirmOverrideCommand.CanExecute(null));

        await vm.ConfirmOverrideCommand.ExecuteAsync(null);

        client.Verify(c => c.OverrideAbcClassAsync("i1", "A", "strategic item", Any), Times.Once);
        Assert.False(vm.IsOverrideOpen);
        client.Verify(c => c.GetAbcRunAsync("r1", Any), Times.Exactly(2));
    }

    /// <summary>
    /// Clearing the override sends a null class.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task AbcOverride_Clear_SendsNullClass()
    {
        _ = client.Setup(c => c.GetAbcRunsAsync(Any)).ReturnsAsync(Ok<IReadOnlyList<AbcRunDto>>([new AbcRunDto { Id = "r1" }]));
        _ = client.Setup(c => c.GetAbcRunAsync("r1", Any)).ReturnsAsync(Ok(Run()));
        _ = client.Setup(c => c.OverrideAbcClassAsync("i1", null, "automatic again", Any)).ReturnsAsync(Ok(true));
        var vm = new AbcAnalysisViewModel(client.Object, loc);
        await vm.LoadAsync();
        vm.SelectedItem = vm.Items[0];
        vm.BeginOverrideCommand.Execute(null);

        vm.ClearOverride = true;
        vm.OverrideReason = "automatic again";
        await vm.ConfirmOverrideCommand.ExecuteAsync(null);

        client.Verify(c => c.OverrideAbcClassAsync("i1", null, "automatic again", Any), Times.Once);
    }

    /// <summary>
    /// The ranked items show the class, the previous class and the cumulative percentage.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task AbcRun_ShowsRankedItems()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        _ = client.Setup(c => c.RunAbcAsync(It.IsAny<AbcRunRequest>(), Any)).ReturnsAsync(Ok(Run()));
        var vm = new AbcAnalysisViewModel(client.Object, loc);

        await vm.RunAnalysisCommand.ExecuteAsync(null);

        Assert.Equal(new[] { "S1", "S2" }, vm.Items.Select(i => i.Sku).ToArray());
        Assert.Equal("A", vm.Items[0].AbcClass);
        Assert.Equal("B", vm.Items[0].PreviousClass);
        Assert.Equal("81.00 %", vm.Items[0].CumulativeText);
        Assert.Equal(loc["Inv_Abc_Manual"] + ", " + loc["Inv_Abc_NewItem"], vm.Items[1].Flags);
        Assert.Single(vm.Runs);
        Assert.Equal(loc["Inv_Abc_RunDone"], vm.StatusMessage);
    }

    /// <summary>
    /// Shares that do not satisfy 0 &lt; A &lt; B &lt;= 100 are refused before any call; valid values are sent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task AbcRun_ValidatesTheShares()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        AbcRunRequest? sent = null;
        _ = client.Setup(c => c.RunAbcAsync(It.IsAny<AbcRunRequest>(), Any))
            .Callback<AbcRunRequest, CancellationToken>((request, _) => sent = request)
            .ReturnsAsync(Ok(Run()));
        var vm = new AbcAnalysisViewModel(client.Object, loc) { AShare = "95", BShare = "80" };

        await vm.RunAnalysisCommand.ExecuteAsync(null);

        client.Verify(c => c.RunAbcAsync(It.IsAny<AbcRunRequest>(), Any), Times.Never);
        Assert.Equal(loc["Inv_Required"], vm.StatusMessage);

        vm.AShare = "70";
        vm.BShare = "90";
        vm.PeriodDays = "60";
        vm.WarehouseId = " w1 ";
        await vm.RunAnalysisCommand.ExecuteAsync(null);

        Assert.NotNull(sent);
        Assert.Equal(60, sent.PeriodDays);
        Assert.Equal(70m, sent.AShare);
        Assert.Equal(90m, sent.BShare);
        Assert.Equal("w1", sent.WarehouseId);
        Assert.True(sent.Apply);
    }

    /// <summary>
    /// Generating plans needs a warehouse; the run-due message carries the number of opened counts.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Plans_GenerateNeedsWarehouse_AndRunDueReportsCount()
    {
        _ = client.Setup(c => c.GetPlansAsync(Any)).ReturnsAsync(Ok<IReadOnlyList<CycleCountPlanDto>>(
            [new CycleCountPlanDto { Id = "p1", Name = "Class A", WarehouseId = "w1", AbcClass = "A", Cadence = "monthly", IntervalDays = 30, IsActive = true }]));
        _ = client.Setup(c => c.RunDuePlansAsync(Any)).ReturnsAsync(Ok(2));
        _ = client.Setup(c => c.GeneratePlansAsync("w1", null, true, Any)).ReturnsAsync(Ok<IReadOnlyList<CycleCountPlanDto>>([]));
        var vm = new CyclePlansViewModel(client.Object, loc);

        Assert.False(vm.GenerateCommand.CanExecute(null));
        Assert.False(vm.RunSelectedCommand.CanExecute(null));

        vm.WarehouseId = "w1";
        Assert.True(vm.GenerateCommand.CanExecute(null));

        await vm.RunDueCommand.ExecuteAsync(null);

        Assert.Contains("2", vm.StatusMessage, StringComparison.Ordinal);
        Assert.Equal(loc["Inv_Plan_CadenceMonthly"], vm.Plans[0].CadenceLabel);

        vm.SelectedPlan = vm.Plans[0];
        Assert.True(vm.RunSelectedCommand.CanExecute(null));

        await vm.GenerateCommand.ExecuteAsync(null);

        client.Verify(c => c.GeneratePlansAsync("w1", null, true, Any), Times.Once);
    }

    /// <summary>
    /// A server that cannot be reached is shown as the offline message.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Offline_ShowsOfflineMessage()
    {
        _ = client.Setup(c => c.GetSessionsAsync(Any)).ReturnsAsync(InventoryCountingResults.Fail<IReadOnlyList<CountSessionDto>>(0, "no route"));
        var vm = new CountSessionsViewModel(client.Object, loc);

        await vm.LoadAsync();

        Assert.Equal(loc["Inv_Offline"], vm.StatusMessage);
    }

    private static InventoryCountingResult<T> Ok<T>(T value) => InventoryCountingResults.Ok(value);

    private static CountVarianceLineDto Line(string id, decimal system, decimal counted, decimal? percent, bool undefined = false) => new()
    {
        Id = id,
        Sku = "SKU-" + id,
        ItemName = "Item " + id,
        IsCounted = true,
        SystemQuantity = system,
        CountedQuantity = counted,
        FinalQuantity = counted,
        VarianceQuantity = counted - system,
        VarianceValue = counted - system,
        VariancePercentage = percent,
        VariancePercentageUndefined = undefined,
    };

    private static CountApprovalDto Approval(int level, string status) => new() { Id = "a" + level, Level = level, Status = status, RequiredRole = level == 1 ? "warehouse_manager" : "cfo" };

    private static CountVarianceReportDto Report(string status, params CountApprovalDto[] approvals) => new()
    {
        Session = new CountSessionDto { Id = "s1", SessionNumber = "CNT-1", Status = status },
        Lines = [Line("l1", 10m, 8m, -20m), Line("l2", 0m, 3m, null, undefined: true)],
        Approvals = approvals,
    };

    private static CountSessionDto Session(string id, string status, bool frozen) => new()
    {
        Id = id,
        SessionNumber = "CNT-" + id,
        Status = status,
        CountType = "full_physical",
        FrozenAt = frozen ? "2026-11-22T08:00:00Z" : null,
    };

    private static AbcRunDto Run() => new()
    {
        Id = "r1",
        ItemCount = 2,
        ClassACount = 1,
        ClassCCount = 1,
        Items =
        [
            new AbcRunItemDto { StockItemId = "i2", Sku = "S2", Name = "Two", Rank = 2, ConsumptionValue = 234.5m, CumulativeShare = 100m, AbcClass = "C", IsManualOverride = true, IsNewItem = true },
            new AbcRunItemDto { StockItemId = "i1", Sku = "S1", Name = "One", Rank = 1, ConsumptionValue = 1000m, CumulativeShare = 81m, AbcClass = "A", PreviousClass = "B" },
        ],
    };
}
