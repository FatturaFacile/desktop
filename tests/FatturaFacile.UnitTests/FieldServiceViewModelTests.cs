// <copyright file="FieldServiceViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.FieldService;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the Field Service hub: loading every tab, the sync of all of them after the outbox, the outbox summary with its
/// conflicts and the refresh of the work orders after a dispatch.
/// </summary>
public sealed class FieldServiceViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly DateTime SyncedAt = new(2027, 3, 2, 7, 0, 0, DateTimeKind.Utc);

    private readonly Mock<ISender> sender = new();
    private readonly ILocalizationService loc = LocalizationService.Instance;

    private FieldServiceOutboxSummary summary = new(0, []);

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldServiceViewModelTests"/> class.
    /// </summary>
    public FieldServiceViewModelTests()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetDispatcherGanttAndMapBoardQuery>(), Any)).ReturnsAsync(new FieldServiceCached<FieldServiceBoard>(FieldServiceBoardParser.Parse(FieldServiceTestData.Board), SyncedAt));
        _ = sender.Setup(s => s.Send(It.IsAny<GetMaintenanceContractsQuery>(), Any)).ReturnsAsync(new FieldServiceCached<IReadOnlyList<MaintenanceContract>>(FieldServiceContractParser.ParseContracts(FieldServiceTestData.Contracts), SyncedAt));
        _ = sender.Setup(s => s.Send(It.IsAny<GetFieldServiceEquipmentQuery>(), Any)).ReturnsAsync(new FieldServiceCached<IReadOnlyList<FieldServiceEquipment>>(FieldServiceContractParser.ParseEquipment(FieldServiceTestData.Equipment), SyncedAt));
        _ = sender.Setup(s => s.Send(It.IsAny<GetFieldWorkOrdersQuery>(), Any)).ReturnsAsync(new FieldServiceCached<IReadOnlyList<FieldServiceWorkOrder>>(FieldServiceWorkOrderParser.ParseList(FieldServiceTestData.WorkOrderList), SyncedAt));
        _ = sender.Setup(s => s.Send(It.IsAny<GetFieldWorkOrderQuery>(), Any)).ReturnsAsync(new FieldServiceCached<FieldServiceWorkOrder>(FieldServiceWorkOrderParser.ParseDetail(FieldServiceTestData.WorkOrder), SyncedAt));
        _ = sender.Setup(s => s.Send(It.IsAny<GetCustomerEquipmentServiceHistoryQuery>(), Any)).ReturnsAsync(new FieldServiceCached<EquipmentHistory>(FieldServiceContractParser.ParseHistory(FieldServiceTestData.History), SyncedAt));
        _ = sender.Setup(s => s.Send(It.IsAny<GetTechnicianOfflineWorkQueueQuery>(), Any)).ReturnsAsync(new FieldServiceCached<FieldServiceTechnicianBundle>(FieldServiceBundleParser.Parse(FieldServiceTestData.Bundle), SyncedAt));
        _ = sender.Setup(s => s.Send(It.IsAny<GetFieldServiceOutboxQuery>(), Any)).ReturnsAsync(() => summary);
        _ = sender.Setup(s => s.Send(It.IsAny<SyncFieldServiceCommand>(), Any)).ReturnsAsync(new FieldServiceCallResult(FieldServiceCallStatus.Ok));
        _ = sender.Setup(s => s.Send(It.IsAny<FlushFieldServiceOutboxCommand>(), Any)).ReturnsAsync(new FieldServiceFlushResult(0, 0, 0, Offline: false));
    }

    /// <summary>
    /// Loading fills every tab from the cache and shows the pending count of the outbox.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_FillsEveryTabAndTheOutboxCount()
    {
        summary = new FieldServiceOutboxSummary(3, []);
        var vm = Create();

        await vm.LoadAsync();

        Assert.True(vm.Board.HasBoard);
        Assert.Equal(2, vm.Contracts.Contracts.Count);
        Assert.Equal(2, vm.WorkOrders.WorkOrders.Count);
        Assert.Single(vm.Equipment.Equipment);
        Assert.Equal(string.Format(loc.CurrentCulture, loc["FieldService_Pending"], "3"), vm.PendingText);
        Assert.True(vm.HasPending);
        Assert.False(vm.HasConflicts);
        Assert.False(vm.IsBusy);
    }

    /// <summary>
    /// The rejected operations are listed with their kind and their error, and can be dismissed.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Conflicts_AreListedAndDismissed()
    {
        var id = Guid.NewGuid();
        summary = new FieldServiceOutboxSummary(0, [new FieldServiceConflict(id, FieldServiceOperationType.PartConsumption, Guid.NewGuid(), SyncedAt, "quantity: Not enough stock")]);
        var vm = Create();
        await vm.LoadAsync();

        var row = Assert.Single(vm.Conflicts);
        Assert.True(vm.HasConflicts);
        Assert.Equal(id, row.Id);
        Assert.Contains("Not enough stock", row.Text, StringComparison.Ordinal);
        Assert.Contains(loc["FieldService_Op_PartConsumption"], row.Text, StringComparison.Ordinal);

        summary = new FieldServiceOutboxSummary(0, []);
        await vm.DismissConflictAsync(row);

        sender.Verify(s => s.Send(It.Is<DismissFieldServiceConflictCommand>(c => c.EntryId == id), Any), Times.Once);
        Assert.False(vm.HasConflicts);
        Assert.Empty(vm.Conflicts);
    }

    /// <summary>
    /// Syncing everything sends the outbox before it pulls any tab, so the server has the field operations first.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SyncAll_SendsTheOutboxBeforePulling()
    {
        List<string> order = [];
        _ = sender.Setup(s => s.Send(It.IsAny<FlushFieldServiceOutboxCommand>(), Any))
            .Callback((IRequest<FieldServiceFlushResult> _, CancellationToken _) => order.Add("flush"))
            .ReturnsAsync(new FieldServiceFlushResult(2, 0, 0, Offline: false));
        _ = sender.Setup(s => s.Send(It.IsAny<SyncFieldServiceCommand>(), Any))
            .Callback((IRequest<FieldServiceCallResult> command, CancellationToken _) => order.Add(((SyncFieldServiceCommand)command).Scope.ToString()))
            .ReturnsAsync(new FieldServiceCallResult(FieldServiceCallStatus.Ok));
        var vm = Create();
        await vm.LoadAsync();
        order.Clear();

        await vm.SyncAllAsync();

        Assert.Equal("flush", order[0]);
        Assert.Contains(nameof(FieldServiceSyncScope.Board), order);
        Assert.Contains(nameof(FieldServiceSyncScope.Contracts), order);
        Assert.Contains(nameof(FieldServiceSyncScope.WorkOrders), order);
        Assert.Contains(nameof(FieldServiceSyncScope.Equipment), order);
        Assert.Equal(string.Format(loc.CurrentCulture, loc["FieldService_FlushDone"], "2", "0", "0"), vm.StatusMessage);
    }

    /// <summary>
    /// A flush that cannot reach the server says how many operations stay queued.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_Offline_SaysHowManyStayQueued()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<FlushFieldServiceOutboxCommand>(), Any)).ReturnsAsync(new FieldServiceFlushResult(0, 0, 4, Offline: true));
        var vm = Create();

        await vm.FlushAsync();

        Assert.Equal(string.Format(loc.CurrentCulture, loc["FieldService_FlushOffline"], "4"), vm.StatusMessage);
    }

    /// <summary>
    /// A dispatch on the board refreshes the work orders tab.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ADispatchOnTheBoard_RefreshesTheWorkOrders()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<DispatchTechnicianWorkOrderCommand>(), Any)).ReturnsAsync(new FieldServiceDispatchResult(FieldServiceCallStatus.Ok, [], Detail: null));
        var vm = Create();
        await vm.LoadAsync();
        sender.Invocations.Clear();
        vm.Board.SelectedUnassigned = vm.Board.Unassigned[0];
        vm.Board.SelectedLane = vm.Board.Lanes[0];

        await vm.Board.DispatchAsync();

        sender.Verify(s => s.Send(It.IsAny<GetFieldWorkOrdersQuery>(), Any), Times.AtLeastOnce);
    }

    private FieldServiceViewModel Create() => new(
        sender.Object,
        loc,
        new FieldServiceBoardViewModel(sender.Object, loc),
        new MaintenanceContractsViewModel(sender.Object, loc),
        new FieldWorkOrderViewModel(sender.Object, loc),
        new EquipmentHistoryViewModel(sender.Object, loc));
}
