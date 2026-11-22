// <copyright file="SlaMonitorViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using FatturaFacile.Application.Features.Helpdesk;
using FatturaFacile.Application.Helpdesk;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the SLA monitoring view model: the default period, the compliance colours, the formatted durations, the offline
/// notice and the grouping.
/// </summary>
public sealed class SlaMonitorViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly DateTime SyncedAt = new(2027, 1, 10, 9, 0, 0, DateTimeKind.Utc);

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private readonly ManualTimeProvider time = new();
    private readonly List<GetSlaMetricsQuery> queries = [];
    private HelpdeskLoad<HelpdeskMetrics>? load;

    /// <summary>
    /// Initializes a new instance of the <see cref="SlaMonitorViewModelTests"/> class.
    /// </summary>
    public SlaMonitorViewModelTests()
    {
        load = new HelpdeskLoad<HelpdeskMetrics>(
            new HelpdeskMetrics(
                "queue",
                [
                    new HelpdeskMetricGroup("L1", 4, 12.5, 185, 97.5, 70, new HelpdeskCsat(3, 4.33)),
                    new HelpdeskMetricGroup("L2", 1, null, null, 85, null, new HelpdeskCsat(0, null)),
                    new HelpdeskMetricGroup(string.Empty, 2, 59.4, 60, 95, 80, new HelpdeskCsat(1, 5)),
                ]),
            FromCache: false,
            SyncedAt);
        _ = sender.Setup(s => s.Send(It.IsAny<GetSlaMetricsQuery>(), Any))
            .Callback<GetSlaMetricsQuery, CancellationToken>((query, _) => queries.Add(query))
            .ReturnsAsync(() => load);
    }

    /// <summary>
    /// The view starts on the last thirty days grouped by queue.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_UsesTheLastThirtyDaysGroupedByQueue()
    {
        var vm = Create();

        await vm.LoadAsync();

        var today = DateOnly.FromDateTime(time.GetLocalNow().Date);
        var query = Assert.Single(queries);
        Assert.Equal(today, query.To);
        Assert.Equal(today.AddDays(-30), query.From);
        Assert.Equal("queue", query.GroupBy);
    }

    /// <summary>
    /// The table shows the compliance of each group with its colour state, the durations and the satisfaction.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_FormatsTheGroupsWithComplianceStates()
    {
        var vm = Create();

        await vm.LoadAsync();

        Assert.True(vm.HasData);
        Assert.False(vm.IsOffline);
        Assert.Equal(3, vm.Rows.Count);
        var culture = loc.CurrentCulture;

        var level1 = vm.Rows[0];
        Assert.Equal("L1", level1.Key);
        Assert.Equal("Good", level1.FirstResponseState);
        Assert.Equal("Bad", level1.ResolutionState);
        Assert.Equal(string.Format(culture, loc["SlaMonitor_Duration_Minutes"], 13.ToString(culture)), level1.MttfrText);
        Assert.Equal(string.Format(culture, loc["SlaMonitor_Duration_Hours"], 3.ToString(culture), 5.ToString(culture)), level1.MttrText);
        Assert.Equal(string.Format(culture, loc["SlaMonitor_Csat"], 4.3.ToString("0.0", culture), 3.ToString(culture)), level1.CsatText);

        var level2 = vm.Rows[1];
        Assert.Equal("Warn", level2.FirstResponseState);
        Assert.Equal("-", level2.MttfrText);
        Assert.Equal("-", level2.ResolutionText);
        Assert.Equal(string.Empty, level2.ResolutionState);
        Assert.Equal("-", level2.CsatText);

        var unnamed = vm.Rows[2];
        Assert.Equal("-", unnamed.Key);
        Assert.Equal("Warn", unnamed.ResolutionState);
        Assert.Equal(string.Format(culture, loc["SlaMonitor_Duration_Hours"], 1.ToString(culture), 0.ToString(culture)), unnamed.MttrText);
        Assert.Equal(string.Format(culture, loc["SlaMonitor_Total"], 7.ToString(culture)), vm.TotalText);
    }

    /// <summary>
    /// Offline, the cached figures are shown with the notice; with nothing cached the table is empty.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_Offline_ShowsTheCacheNoticeOrNothing()
    {
        load = load! with { FromCache = true };
        var vm = Create();

        await vm.LoadAsync();
        Assert.True(vm.IsOffline);
        Assert.Equal(loc["Helpdesk_OfflineCache"], vm.StatusMessage);
        Assert.Equal(3, vm.Rows.Count);

        load = null;
        await vm.LoadAsync();
        Assert.False(vm.HasData);
        Assert.Empty(vm.Rows);
        Assert.Equal(loc["Helpdesk_NeverSynced"], vm.LastSyncedText);
    }

    /// <summary>
    /// A period without tickets says so.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_EmptyPeriod_SaysThereIsNoData()
    {
        load = new HelpdeskLoad<HelpdeskMetrics>(new HelpdeskMetrics("queue", []), FromCache: false, SyncedAt);
        var vm = Create();

        await vm.LoadAsync();

        Assert.Equal(loc["SlaMonitor_NoData"], vm.TotalText);
    }

    /// <summary>
    /// Choosing another grouping reloads the metrics with it.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ChangingTheGrouping_ReloadsWithIt()
    {
        var vm = Create();
        await vm.LoadAsync();

        vm.SelectedGroup = vm.GroupChoices.Single(choice => string.Equals(choice.Key, "contract", StringComparison.Ordinal));

        DssTestData.SameStrings(["queue", "contract"], queries.Select(query => query.GroupBy));
    }

    /// <summary>
    /// A user without the permission sees the reason.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_WithoutPermission_ShowsTheReason()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetSlaMetricsQuery>(), Any)).ThrowsAsync(new UnauthorizedAccessException("No access"));
        var vm = Create();

        await vm.LoadAsync();

        Assert.Equal("No access", vm.StatusMessage);
        Assert.False(vm.IsBusy);
    }

    private SlaMonitorViewModel Create() => new(sender.Object, loc, time);
}
