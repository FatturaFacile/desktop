// <copyright file="FieldServiceBoardViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.FieldService;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the dispatcher board view model: loading the cache, the timeline geometry, the warnings per technician, the dispatch
/// action with the server blockers and the preventive run.
/// </summary>
public sealed class FieldServiceBoardViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly DateTime SyncedAt = new(2027, 3, 1, 6, 0, 0, DateTimeKind.Utc);

    private readonly Mock<ISender> sender = new();
    private readonly ILocalizationService loc = LocalizationService.Instance;

    private FieldServiceCached<FieldServiceBoard>? cached = new(FieldServiceBoardParser.Parse(FieldServiceTestData.Board), SyncedAt);

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldServiceBoardViewModelTests"/> class.
    /// </summary>
    public FieldServiceBoardViewModelTests()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetDispatcherGanttAndMapBoardQuery>(), Any)).ReturnsAsync(() => cached);
        _ = sender.Setup(s => s.Send(It.IsAny<SyncFieldServiceCommand>(), Any)).ReturnsAsync(new FieldServiceCallResult(FieldServiceCallStatus.Ok));
    }

    /// <summary>
    /// Loading shows the unassigned queue and a lane per technician, with the bars placed on the timeline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ShowsTheQueueTheLanesAndTheBars()
    {
        var vm = await LoadedAsync();

        Assert.True(vm.HasBoard);
        Assert.False(vm.IsOffline);
        var order = Assert.Single(vm.Unassigned);
        Assert.Equal("WO-1", order.Number);
        Assert.Equal(2, vm.Lanes.Count);

        var bar = Assert.Single(vm.Lanes[0].Bars);
        Assert.Equal("WO-2", bar.Number);
        Assert.Equal(32.0 / 168.0 * vm.TimelineWidth, bar.Left, precision: 3);
        Assert.Equal(4.0 / 168.0 * vm.TimelineWidth, bar.Width, precision: 3);
        Assert.Empty(vm.Lanes[1].Bars);
        sender.Verify(s => s.Send(It.IsAny<SyncFieldServiceCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// With an empty cache the board is pulled from the server, and an unreachable server is shown as offline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_WithEmptyCacheAndNoServer_IsOffline()
    {
        cached = null;
        _ = sender.Setup(s => s.Send(It.IsAny<SyncFieldServiceCommand>(), Any)).ReturnsAsync(new FieldServiceCallResult(FieldServiceCallStatus.Offline));

        var vm = await LoadedAsync();

        Assert.False(vm.HasBoard);
        Assert.True(vm.IsOffline);
        Assert.Equal(loc["FieldService_OfflineUsingCache"], vm.StatusMessage);
        Assert.Empty(vm.Lanes);
    }

    /// <summary>
    /// Selecting a work order and a window shows, under each technician, the overlap, the missing skills and the van warnings.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SelectingAnOrderAndAWindow_ShowsTheWarningsOfEachTechnician()
    {
        var vm = await LoadedAsync();

        vm.SelectedUnassigned = vm.Unassigned[0];
        SetWindow(vm, new DateTimeOffset(2027, 3, 2, 11, 0, 0, TimeSpan.Zero), hours: 3);

        Assert.True(vm.Lanes[0].HasWarning);
        Assert.Contains("WO-2", vm.Lanes[0].WarningText, StringComparison.Ordinal);
        Assert.True(vm.Lanes[1].HasWarning);
        Assert.Contains("electrical", vm.Lanes[1].WarningText, StringComparison.Ordinal);
        Assert.Contains(loc["FieldService_Warn_Van"], vm.Lanes[1].WarningText, StringComparison.Ordinal);

        SetWindow(vm, new DateTimeOffset(2027, 3, 2, 13, 0, 0, TimeSpan.Zero), hours: 2);

        Assert.False(vm.Lanes[0].HasWarning);
        Assert.Empty(vm.Lanes[0].WarningText);
        Assert.True(vm.Lanes[1].HasWarning);

        vm.SelectedUnassigned = null;

        Assert.All(vm.Lanes, lane => Assert.False(lane.HasWarning));
    }

    /// <summary>
    /// Dispatching sends the selected order and technician with the chosen window, then refreshes the board and tells the other tabs.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Dispatch_SendsTheSelectionAndRefreshes()
    {
        DispatchTechnicianWorkOrderCommand? sent = null;
        _ = sender.Setup(s => s.Send(It.IsAny<DispatchTechnicianWorkOrderCommand>(), Any))
            .Callback((IRequest<FieldServiceDispatchResult> command, CancellationToken _) => sent = (DispatchTechnicianWorkOrderCommand)command)
            .ReturnsAsync(new FieldServiceDispatchResult(FieldServiceCallStatus.Ok, [], Detail: null));
        var vm = await LoadedAsync();
        var raised = 0;
        vm.DataChanged += (_, _) => raised++;
        vm.SelectedUnassigned = vm.Unassigned[0];
        vm.SelectedLane = vm.Lanes[0];
        SetWindow(vm, new DateTimeOffset(2027, 3, 2, 13, 0, 0, TimeSpan.Zero), hours: 2);

        await vm.DispatchAsync();

        Assert.NotNull(sent);
        Assert.Equal(FieldServiceTestData.OrderId, sent.WorkOrderId);
        Assert.Equal(FieldServiceTestData.TechnicianId, sent.TechnicianId);
        Assert.Equal(TimeSpan.FromHours(2), sent.End - sent.Start);
        Assert.Equal(new DateTimeOffset(2027, 3, 2, 13, 0, 0, TimeSpan.Zero), sent.Start);
        Assert.Equal(1, raised);
        Assert.Empty(vm.DispatchWarnings);
        sender.Verify(s => s.Send(It.Is<SyncFieldServiceCommand>(c => c.Scope == FieldServiceSyncScope.Board), Any), Times.Once);
        Assert.False(vm.IsBusy);
    }

    /// <summary>
    /// A dispatch the server blocks lists every blocker and changes nothing else.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Dispatch_Blocked_ListsTheBlockers()
    {
        IReadOnlyList<FieldServiceWarning> blockers =
        [
            new FieldServiceWarning(FieldServiceWarningKind.Schedule, "WO-2"),
            new FieldServiceWarning(FieldServiceWarningKind.Skills, "electrical"),
            new FieldServiceWarning(FieldServiceWarningKind.VanStock, string.Empty),
        ];
        _ = sender.Setup(s => s.Send(It.IsAny<DispatchTechnicianWorkOrderCommand>(), Any)).ReturnsAsync(new FieldServiceDispatchResult(FieldServiceCallStatus.Rejected, blockers, "422"));
        var vm = await LoadedAsync();
        var raised = 0;
        vm.DataChanged += (_, _) => raised++;
        vm.SelectedUnassigned = vm.Unassigned[0];
        vm.SelectedLane = vm.Lanes[1];

        await vm.DispatchAsync();

        Assert.Equal(3, vm.DispatchWarnings.Count);
        Assert.Contains("electrical", vm.DispatchWarnings[1], StringComparison.Ordinal);
        Assert.Equal(loc["FieldService_DispatchBlocked"], vm.StatusMessage);
        Assert.Equal(0, raised);
        sender.Verify(s => s.Send(It.IsAny<SyncFieldServiceCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// Dispatching without a selection asks for one and sends nothing.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Dispatch_WithoutSelection_AsksForOne()
    {
        var vm = await LoadedAsync();

        await vm.DispatchAsync();

        Assert.Equal(loc["FieldService_SelectToDispatch"], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<DispatchTechnicianWorkOrderCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// A window that is not a time or has no length is refused before anything is sent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Dispatch_WithAnInvalidWindow_IsRefusedLocally()
    {
        var vm = await LoadedAsync();
        vm.SelectedUnassigned = vm.Unassigned[0];
        vm.SelectedLane = vm.Lanes[0];
        vm.StartTimeText = "late";

        await vm.DispatchAsync();

        Assert.Equal(loc["FieldService_Err_Window"], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<DispatchTechnicianWorkOrderCommand>(), Any), Times.Never);

        vm.StartTimeText = "09:00";
        vm.DurationText = "0";
        await vm.DispatchAsync();

        sender.Verify(s => s.Send(It.IsAny<DispatchTechnicianWorkOrderCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// A validation failure of the pipeline is shown with its localized message.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Dispatch_ValidationFailure_ShowsTheLocalizedMessage()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<DispatchTechnicianWorkOrderCommand>(), Any))
            .ThrowsAsync(new ValidationException([new ValidationFailure("End", "FieldService_Err_Window")]));
        var vm = await LoadedAsync();
        vm.SelectedUnassigned = vm.Unassigned[0];
        vm.SelectedLane = vm.Lanes[0];

        await vm.DispatchAsync();

        Assert.Equal(loc["FieldService_Err_Window"], vm.StatusMessage);
        Assert.False(vm.IsBusy);
    }

    /// <summary>
    /// The preventive run shows what the engine did and refreshes the board.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task RunPreventive_ShowsTheCountsAndRefreshes()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GeneratePreventiveMaintenanceWorkOrdersCommand>(), Any))
            .ReturnsAsync(new FieldServicePreventiveResult(FieldServiceCallStatus.Ok, 7, 2, 3, Detail: null));
        var vm = await LoadedAsync();
        var raised = 0;
        vm.DataChanged += (_, _) => raised++;

        await vm.RunPreventiveAsync();

        Assert.Equal(string.Format(loc.CurrentCulture, loc["FieldService_PreventiveDone"], "7", "2", "3"), vm.PreventiveText);
        Assert.Equal(1, raised);
        sender.Verify(s => s.Send(It.Is<SyncFieldServiceCommand>(c => c.Scope == FieldServiceSyncScope.Board), Any), Times.Once);
    }

    /// <summary>
    /// A refused preventive run is shown and does not refresh anything.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task RunPreventive_Forbidden_IsShown()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GeneratePreventiveMaintenanceWorkOrdersCommand>(), Any))
            .ReturnsAsync(new FieldServicePreventiveResult(FieldServiceCallStatus.Forbidden, 0, 0, 0, Detail: null));
        var vm = await LoadedAsync();

        await vm.RunPreventiveAsync();

        Assert.Equal(loc["FieldService_Forbidden"], vm.PreventiveText);
        sender.Verify(s => s.Send(It.IsAny<SyncFieldServiceCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// Moving to the next week pulls the following seven days.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task NextWeek_PullsTheFollowingWindow()
    {
        var vm = await LoadedAsync();

        await vm.NextWeekAsync();

        var from = new DateTimeOffset(2027, 3, 8, 0, 0, 0, TimeSpan.Zero);
        sender.Verify(s => s.Send(It.Is<SyncFieldServiceCommand>(c => c.Scope == FieldServiceSyncScope.Board && c.From == from && c.To == from.AddDays(7)), Any), Times.Once);
    }

    private static void SetWindow(FieldServiceBoardViewModel vm, DateTimeOffset startUtc, double hours)
    {
        var local = TimeZoneInfo.ConvertTime(startUtc, TimeZoneInfo.Local);
        vm.DispatchDate = local.Date;
        vm.StartTimeText = local.ToString("HH:mm", CultureInfo.InvariantCulture);
        vm.DurationText = hours.ToString(CultureInfo.InvariantCulture);
    }

    private async Task<FieldServiceBoardViewModel> LoadedAsync()
    {
        FieldServiceBoardViewModel vm = new(sender.Object, loc);
        await vm.LoadAsync();

        return vm;
    }
}
