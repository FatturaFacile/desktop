// <copyright file="ExecutiveDssViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using System.Net.Http;
using FatturaFacile.Application.Dss;
using FatturaFacile.Application.Features.Dss;
using FatturaFacile.Domain.Dss;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.Services;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the Executive DSS cockpit view model: cached display, sync and offline behavior, filters, simulator, detail and export.
/// </summary>
public sealed class ExecutiveDssViewModelTests : IDisposable
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly DateTime SyncedAt = new(2026, 6, 30, 10, 0, 0, DateTimeKind.Utc);

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private readonly FakeFileDialog dialog = new();
    private readonly List<string> temporaryFiles = [];
    private DssSnapshot? cached;
    private DssSyncResult syncResult = new(DssSyncStatus.Synced, SyncedAt);

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutiveDssViewModelTests"/> class.
    /// </summary>
    public ExecutiveDssViewModelTests()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetDssSnapshotQuery>(), Any)).ReturnsAsync((GetDssSnapshotQuery query, CancellationToken _) => cached is null ? null : cached with { Filter = query.Filter });
        _ = sender.Setup(s => s.Send(It.IsAny<SyncDssSnapshotCommand>(), Any)).ReturnsAsync(() => syncResult);
    }

    /// <summary>
    /// With a cached snapshot the cockpit shows the scorecard groups, the alerts, the matrix, the Whale Curve and the waterfall.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_Cached_PopulatesTheCockpit()
    {
        cached = Snapshot();
        var vm = Create();

        await vm.LoadAsync();

        Assert.True(vm.HasSnapshot);
        Assert.Equal(5, vm.FinancialKpis.Count);
        Assert.Equal(2, vm.CustomerKpis.Count);
        _ = Assert.Single(vm.EfficiencyKpis);
        DssTestData.SameStrings(["CRITICAL", "WARNING"], vm.Alerts.Select(alert => alert.Severity));
        Assert.Equal(2, vm.MatrixRows.Count);
        Assert.Equal(2, vm.WhalePoints.Count);
        Assert.Equal(7, vm.BridgeBars.Count);
        Assert.Contains(SyncedAt.ToLocalTime().ToString("g", loc.CurrentCulture), vm.LastSyncedText, StringComparison.Ordinal);
        sender.Verify(s => s.Send(It.IsAny<SyncDssSnapshotCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// The indicator cards carry the formatted value, the colour state of the change, the trend and the target gauge.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_Cards_CarryValueDeltaTrendAndTarget()
    {
        cached = Snapshot();
        var vm = Create();

        await vm.LoadAsync();

        var culture = loc.CurrentCulture;
        var revenue = vm.FinancialKpis.Single(card => string.Equals(card.Key, "revenue", StringComparison.Ordinal));
        Assert.Equal(1000.ToString("N0", culture) + " CHF", revenue.ValueText);
        Assert.Equal("+100%", revenue.DeltaText);
        Assert.Equal("Good", revenue.DeltaState);
        Assert.True(revenue.HasSparkline);
        Assert.Equal(loc["Dss_KPI_REVENUE"], revenue.Title);

        var margin = vm.FinancialKpis.Single(card => string.Equals(card.Key, "gross_margin_pct", StringComparison.Ordinal));
        Assert.Equal("60%", margin.ValueText);
        Assert.Equal("Bad", margin.DeltaState);

        var ebitda = vm.FinancialKpis.Single(card => string.Equals(card.Key, "ebitda", StringComparison.Ordinal));
        Assert.False(ebitda.HasDelta);
        Assert.Equal("Neutral", ebitda.DeltaState);

        var unavailable = vm.EfficiencyKpis.Single();
        Assert.False(unavailable.IsAvailable);
        Assert.Equal("–", unavailable.ValueText);

        // A negative cash conversion cycle against a target of 30 reads as 0% of the target, still with a gauge.
        var cycle = vm.FinancialKpis.Single(card => string.Equals(card.Key, "cash_conversion_cycle", StringComparison.Ordinal));
        Assert.True(cycle.HasTarget);
        Assert.Equal(0, cycle.TargetProgress);
    }

    /// <summary>
    /// Alerts are listed with the localized severity and title, and the customer they name.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_Alerts_AreLocalizedAndNameTheCustomer()
    {
        cached = Snapshot();
        var vm = Create();

        await vm.LoadAsync();

        Assert.Equal(loc["Dss_SEVERITY_CRITICAL"], vm.Alerts[0].SeverityText);
        Assert.Equal(loc["Dss_ALERT_CUSTOMER_CONCENTRATION"], vm.Alerts[0].Title);
        Assert.StartsWith("Alpenbau AG · ", vm.Alerts[0].Detail, StringComparison.Ordinal);
        Assert.DoesNotContain("·", vm.Alerts[1].Detail, StringComparison.Ordinal);
    }

    /// <summary>
    /// Nothing cached for the filter: the cockpit pulls it from the server once and then shows it.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_NothingCached_PullsFromTheServerThenShows()
    {
        var vm = Create();
        _ = sender.Setup(s => s.Send(It.IsAny<SyncDssSnapshotCommand>(), Any)).Callback(() => cached = Snapshot()).ReturnsAsync(syncResult);

        await vm.LoadAsync();

        sender.Verify(s => s.Send(It.IsAny<SyncDssSnapshotCommand>(), Any), Times.Once);
        Assert.True(vm.HasSnapshot);
        Assert.False(vm.IsOffline);
        Assert.Equal(loc["Dss_Synced"], vm.StatusMessage);
        Assert.Equal(5, vm.FinancialKpis.Count);
    }

    /// <summary>
    /// Offline with nothing cached the cockpit says so instead of showing an empty page.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_OfflineWithoutCache_ShowsTheOfflineBadgeAndMessage()
    {
        syncResult = new DssSyncResult(DssSyncStatus.Offline, Detail: "no route");
        var vm = Create();

        await vm.LoadAsync();

        Assert.True(vm.IsOffline);
        Assert.False(vm.HasSnapshot);
        Assert.Equal(loc["Dss_OfflineNoCache"], vm.StatusMessage);
        Assert.Empty(vm.FinancialKpis);
        Assert.Equal(loc["Dss_NeverSynced"], vm.LastSyncedText);
    }

    /// <summary>
    /// A failed sync while a snapshot is on screen keeps it, with its timestamp, and flags the cockpit offline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_Offline_KeepsTheCachedSnapshotOnScreen()
    {
        cached = Snapshot();
        var vm = Create();
        await vm.LoadAsync();
        syncResult = new DssSyncResult(DssSyncStatus.Offline, Detail: "no route");

        await vm.SyncAsync();

        Assert.True(vm.IsOffline);
        Assert.True(vm.HasSnapshot);
        Assert.Equal(5, vm.FinancialKpis.Count);
        Assert.Equal(loc["Dss_OfflineUsingCache"], vm.StatusMessage);
        Assert.Contains(SyncedAt.ToLocalTime().ToString("g", loc.CurrentCulture), vm.LastSyncedText, StringComparison.Ordinal);
        Assert.False(vm.IsBusy);
    }

    /// <summary>
    /// The message follows the outcome of the sync: sign in, forbidden, failed with the reason.
    /// </summary>
    /// <param name="status">The outcome.</param>
    /// <param name="expectedKey">The resource key of the expected message.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData(DssSyncStatus.Unauthorized, "Dss_SignInRequired")]
    [InlineData(DssSyncStatus.Forbidden, "Dss_Forbidden")]
    [InlineData(DssSyncStatus.Synced, "Dss_Synced")]
    public async Task Sync_Outcome_ShowsTheMatchingMessage(DssSyncStatus status, string expectedKey)
    {
        syncResult = new DssSyncResult(status, SyncedAt);
        var vm = Create();

        await vm.SyncAsync();

        Assert.Equal(loc[expectedKey], vm.StatusMessage);
        Assert.Equal(status != DssSyncStatus.Synced, vm.IsOffline);
    }

    /// <summary>
    /// A server error shows the reason.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_Failed_ShowsTheReason()
    {
        syncResult = new DssSyncResult(DssSyncStatus.Failed, Detail: "HTTP 500");
        var vm = Create();

        await vm.SyncAsync();

        Assert.Contains("HTTP 500", vm.StatusMessage, StringComparison.Ordinal);
    }

    /// <summary>
    /// A user without the permission gets the explanation instead of an exception, and the busy flag is released.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_PermissionDenied_ShowsTheMessage()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetDssSnapshotQuery>(), Any)).ThrowsAsync(new UnauthorizedAccessException("no permission"));
        var vm = Create();

        await vm.LoadAsync();

        Assert.Equal("no permission", vm.StatusMessage);
        Assert.False(vm.IsBusy);
    }

    /// <summary>
    /// Changing a part of the filter reads the snapshot of the new filter.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task FilterChange_ReadsTheSnapshotOfTheNewFilter()
    {
        cached = Snapshot();
        var vm = Create();
        await vm.LoadAsync();

        vm.SelectedCurrency = vm.Currencies.Single(choice => string.Equals(choice.Value, "EUR", StringComparison.Ordinal));
        vm.SelectedPeriod = vm.Periods.Single(choice => string.Equals(choice.Value, "MTD", StringComparison.Ordinal));
        vm.SelectedBaseline = vm.Baselines.Single(choice => string.Equals(choice.Value, "BUDGET", StringComparison.Ordinal));
        vm.SelectedEntity = vm.Entities.Single(choice => string.Equals(choice.Value, "ent-2", StringComparison.Ordinal));

        sender.Verify(s => s.Send(It.Is<GetDssSnapshotQuery>(q => q.Filter == new DssFilter("ent-2", "EUR", "MTD", "BUDGET")), Any), Times.Once);
        Assert.Equal(new DssFilter("ent-2", "EUR", "MTD", "BUDGET"), vm.CurrentFilter);
    }

    /// <summary>
    /// The first entry of the entity filter means every legal entity, and the server entities follow it.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_Entities_StartWithAllAndKeepTheSelection()
    {
        cached = Snapshot();
        var vm = Create();

        await vm.LoadAsync();

        DssTestData.SameStrings([string.Empty, "ent-1", "ent-2"], vm.Entities.Select(choice => choice.Value));
        Assert.Equal(string.Empty, vm.SelectedEntity!.Value);
    }

    /// <summary>
    /// The variance waterfall switches between the EBITDA and the revenue bridge.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ShowRevenueBridge_SwitchesTheWaterfall()
    {
        cached = Snapshot();
        var vm = Create();
        await vm.LoadAsync();

        vm.ShowRevenueBridge = true;

        Assert.Equal(5, vm.BridgeBars.Count);

        vm.ShowRevenueBridge = false;

        Assert.Equal(7, vm.BridgeBars.Count);
    }

    /// <summary>
    /// Moving a slider runs the local calculator on the cached actuals: the same figures as the server formulas, without any request.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Simulator_Slider_UsesTheLocalCalculator()
    {
        cached = Snapshot();
        var vm = Create();
        await vm.LoadAsync();
        sender.Invocations.Clear();

        vm.PricePct = 10;

        var expected = WhatIfScenarioCalculator.Simulate(cached.Baseline, new WhatIfScenario(PricePct: 10));
        var culture = loc.CurrentCulture;
        Assert.True(vm.HasSimulation);
        Assert.Equal(3, vm.SimulationRows.Count);
        Assert.Equal(expected.Simulated.Ebitda.ToString("N0", culture) + " CHF", vm.SimulationRows[0].Simulated);
        Assert.Equal(expected.Baseline.Ebitda.ToString("N0", culture) + " CHF", vm.SimulationRows[0].Baseline);
        Assert.Equal("Good", vm.SimulationRows[0].DeltaState);
        Assert.Equal(loc["Dss_SIM_BREAK_EVEN"], vm.SimulationRows[2].Title);
        Assert.Empty(sender.Invocations);
    }

    /// <summary>
    /// A price drop is shown as a deterioration and a higher break-even as bad.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Simulator_PriceDrop_IsMarkedBad()
    {
        cached = Snapshot();
        var vm = Create();
        await vm.LoadAsync();

        vm.PricePct = -10;

        Assert.Equal("Bad", vm.SimulationRows[0].DeltaState);
        Assert.Equal("Bad", vm.SimulationRows[2].DeltaState);
    }

    /// <summary>
    /// The simulator keeps working offline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Simulator_WorksOffline()
    {
        cached = Snapshot();
        var vm = Create();
        await vm.LoadAsync();
        syncResult = new DssSyncResult(DssSyncStatus.Offline, Detail: "no route");
        await vm.SyncAsync();

        vm.CogsPct = 5;
        vm.FxShiftPct = 3;

        Assert.True(vm.IsOffline);
        Assert.True(vm.HasSimulation);
        Assert.Equal(3, vm.SimulationRows.Count);
    }

    /// <summary>
    /// A neutral scenario shows no result, and the reset puts every slider back to zero.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Simulator_Reset_ClearsTheScenario()
    {
        cached = Snapshot();
        var vm = Create();
        await vm.LoadAsync();
        Assert.False(vm.HasSimulation);
        Assert.Empty(vm.SimulationRows);

        vm.PricePct = 5;
        vm.OpexPct = -3;
        vm.DsoDaysDelta = 7;
        vm.FteDelta = 2;
        vm.ResetScenario();

        Assert.False(vm.HasSimulation);
        Assert.Empty(vm.SimulationRows);
        Assert.Equal(0, vm.PricePct + vm.CogsPct + vm.OpexPct + vm.DsoDaysDelta + vm.FxShiftPct + vm.FteDelta);
    }

    /// <summary>
    /// Without a snapshot the sliders have nothing to simulate on.
    /// </summary>
    [Fact]
    public void Simulator_WithoutSnapshot_ShowsNothing()
    {
        var vm = Create();

        vm.PricePct = 10;

        Assert.False(vm.HasSimulation);
        Assert.Empty(vm.SimulationRows);
    }

    /// <summary>
    /// Opening a profitability row shows its figures in the detail dialog until it is closed.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task MatrixRow_Selected_OpensTheDetailDialog()
    {
        cached = Snapshot();
        var vm = Create();
        await vm.LoadAsync();

        vm.SelectedMatrixRow = vm.MatrixRows[1];

        Assert.True(vm.IsDetailOpen);
        Assert.Equal(8, vm.DetailLines.Count);
        Assert.Equal("Verlust GmbH", vm.DetailLines[0].Value);

        vm.CloseDetail();

        Assert.False(vm.IsDetailOpen);
        Assert.Null(vm.SelectedMatrixRow);
        Assert.Empty(vm.DetailLines);
    }

    /// <summary>
    /// The board pack needs the server: offline the user is told so and no dialog opens.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Export_Offline_IsRefused()
    {
        cached = Snapshot();
        syncResult = new DssSyncResult(DssSyncStatus.Offline, Detail: "no route");
        var vm = Create();
        await vm.LoadAsync();
        await vm.SyncAsync();

        await vm.ExportBoardPackAsync("pdf");

        Assert.Equal(loc["Dss_ExportOnlineOnly"], vm.StatusMessage);
        Assert.Equal(0, dialog.Calls);
    }

    /// <summary>
    /// Online, the board pack is requested in the chosen format and saved where the user picked.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Export_Online_SavesTheFile()
    {
        cached = Snapshot();
        var vm = Create();
        await vm.LoadAsync();
        dialog.Path = TempPath();
        _ = sender.Setup(s => s.Send(It.IsAny<ExportDssBoardPackQuery>(), Any)).ReturnsAsync(new DssBoardPack(new byte[] { 1, 2, 3 }, "x.xlsx"));

        await vm.ExportBoardPackAsync("xlsx");

        Assert.Equal(new byte[] { 1, 2, 3 }, await File.ReadAllBytesAsync(dialog.Path, CancellationToken.None));
        Assert.Contains(dialog.Path, vm.StatusMessage, StringComparison.Ordinal);
        Assert.EndsWith(".xlsx", dialog.LastSuggestedName, StringComparison.Ordinal);
        sender.Verify(s => s.Send(It.Is<ExportDssBoardPackQuery>(q => string.Equals(q.Format, "xlsx", StringComparison.Ordinal) && q.Filter == DssFilter.Default), Any), Times.Once);
        Assert.False(vm.IsBusy);
    }

    /// <summary>
    /// An unknown format falls back to PDF, and a cancelled dialog requests nothing.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Export_CancelledDialog_RequestsNothing()
    {
        cached = Snapshot();
        var vm = Create();
        await vm.LoadAsync();
        dialog.Path = null;

        await vm.ExportBoardPackAsync("docx");

        sender.Verify(s => s.Send(It.IsAny<ExportDssBoardPackQuery>(), Any), Times.Never);
        Assert.EndsWith(".pdf", dialog.LastSuggestedName, StringComparison.Ordinal);
    }

    /// <summary>
    /// A network failure during the export is reported and flags the cockpit offline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Export_ServerUnreachable_ReportsAndGoesOffline()
    {
        cached = Snapshot();
        var vm = Create();
        await vm.LoadAsync();
        dialog.Path = TempPath();
        _ = sender.Setup(s => s.Send(It.IsAny<ExportDssBoardPackQuery>(), Any)).ThrowsAsync(new HttpRequestException("no route"));

        await vm.ExportBoardPackAsync("pdf");

        Assert.True(vm.IsOffline);
        Assert.Contains("no route", vm.StatusMessage, StringComparison.Ordinal);
        Assert.False(File.Exists(dialog.Path));
        Assert.False(vm.IsBusy);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var file in temporaryFiles.Where(File.Exists))
        {
            File.Delete(file);
        }
    }

    private static DssSnapshot Snapshot()
    {
        var overview = DssSnapshotParser.ParseOverview(DssTestData.Overview);

        return new DssSnapshot(
            DssFilter.Default,
            overview,
            DssSnapshotParser.ParseProfitability(DssTestData.Profitability),
            DssSnapshotParser.ParseVarianceBridge(DssTestData.VarianceBridge),
            DssSnapshotParser.ParseBaseline(overview, DssTestData.SimulationInputs),
            DssSnapshotParser.ParseLegalEntities(DssTestData.LegalEntities),
            SyncedAt);
    }

    private ExecutiveDssViewModel Create() => new(sender.Object, loc, dialog);

    private string TempPath()
    {
        var path = Path.Combine(Path.GetTempPath(), "dss-" + Guid.NewGuid().ToString("N") + ".bin");
        temporaryFiles.Add(path);

        return path;
    }

    private sealed class FakeFileDialog : IFileDialogService
    {
        public string? Path { get; set; }

        public int Calls { get; private set; }

        public string LastSuggestedName { get; private set; } = string.Empty;

        public string? ShowSaveFile(string title, string filter, string defaultFileName)
        {
            Calls++;
            LastSuggestedName = defaultFileName;

            return Path;
        }

        public string? ShowOpenFile(string title, string filter) => null;
    }
}
