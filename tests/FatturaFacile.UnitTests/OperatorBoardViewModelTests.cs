// <copyright file="OperatorBoardViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Mes;
using FatturaFacile.Application.Mes;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.Services;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the MES operator board view model: loading, the live counters, the action flags, the declaration, lot and suspension
/// panels and the way refused or queued commands are shown.
/// </summary>
public sealed class OperatorBoardViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly DateTime Start = new(2026, 6, 30, 8, 0, 0, DateTimeKind.Utc);

    private readonly Mock<ISender> sender = new();
    private readonly Mock<ICurrentUserService> user = new();
    private readonly FakeNetwork network = new();
    private readonly ILocalizationService loc = LocalizationService.Instance;
    private readonly MutableTime clock = new(Start);
    private readonly MesWorkCenter center = new(Guid.NewGuid(), "WC-01", "Milling");
    private readonly Guid itemId = Guid.NewGuid();

    private MesBoardView board;

    /// <summary>
    /// Initializes a new instance of the <see cref="OperatorBoardViewModelTests"/> class.
    /// </summary>
    public OperatorBoardViewModelTests()
    {
        var run = new MesOpenInterval(MesTimeCategory.Run, Start, MachineAttended: true, Reason: null);
        MesComponent[] components = [new(itemId, "STEEL", "Steel bar", 10m, 4m, LotTracked: true)];
        var running = MesTestRows.Row(MesPhaseStatus.Running, Guid.NewGuid(), open: run, components: components);
        var available = MesTestRows.Row(MesPhaseStatus.Available);
        board = new MesBoardView(center.Id, center.Name, [running, available], FromCache: false, Start, PendingCount: 0, []);

        _ = sender.Setup(s => s.Send(It.IsAny<GetMesWorkCentersQuery>(), Any)).ReturnsAsync([center]);
        _ = sender.Setup(s => s.Send(It.IsAny<GetMesBoardQuery>(), Any)).ReturnsAsync(() => board);
        _ = sender.Setup(s => s.Send(It.IsAny<ExecuteMesCommandCommand>(), Any)).ReturnsAsync(new MesExecuteResult(MesExecuteStatus.Sent));
        _ = sender.Setup(s => s.Send(It.IsAny<FlushMesOutboxCommand>(), Any)).ReturnsAsync(new MesFlushResult(0, 0, 0, Offline: false));
    }

    /// <summary>
    /// Loading selects the first work centre and shows its phases with the actions their state allows.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ShowsPhasesAndTheActionsOfTheirState()
    {
        var vm = await LoadedAsync();

        Assert.Equal(center, vm.SelectedWorkCenter);
        Assert.Equal(2, vm.Phases.Count);
        Assert.False(vm.IsOffline);

        var running = vm.Phases[0];
        Assert.True(running.CanSuspend);
        Assert.True(running.CanDeclare);
        Assert.True(running.CanConsume);
        Assert.True(running.CanClose);
        Assert.False(running.CanTakeOver);
        Assert.False(running.CanStartSetup);
        Assert.False(running.CanResume);

        var available = vm.Phases[1];
        Assert.True(available.CanTakeOver);
        Assert.False(available.CanSuspend);
        Assert.False(available.CanClose);
    }

    /// <summary>
    /// A board that comes from the cache is shown as offline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_FromCache_IsOffline()
    {
        board = board with { FromCache = true, PendingCount = 3 };

        var vm = await LoadedAsync();

        Assert.True(vm.IsOffline);
        Assert.Equal(3, vm.PendingCount);
    }

    /// <summary>
    /// Without any cached board the operator is told, and no phases are shown.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_NoBoardAtAll_ShowsAnErrorAndNoPhases()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetMesBoardQuery>(), Any)).ReturnsAsync((MesBoardView?)null);

        var vm = await LoadedAsync();

        Assert.Empty(vm.Phases);
        Assert.True(vm.IsError);
        Assert.True(vm.IsOffline);
    }

    /// <summary>
    /// The timer advances the counters of the open interval without any call to the server.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Tick_AdvancesTheLiveCounters()
    {
        var vm = await LoadedAsync();
        var card = vm.Phases[0];
        Assert.Equal("00:00:00", card.ManText);

        clock.Now = Start.AddSeconds(90);
        vm.Tick();

        Assert.Equal("00:01:30", card.ManText);
        Assert.Equal("00:01:30", card.MachineText);
        Assert.Equal("00:00:00", card.DowntimeText);
    }

    /// <summary>
    /// Typing good and scrap pieces on the keypad and choosing a defect reason sends one declaration with those quantities.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Declaration_KeypadAndDefectReason_SendTheQuantities()
    {
        var vm = await LoadedAsync();
        vm.OpenDeclarationCommand.Execute(vm.Phases[0]);
        Assert.True(vm.IsDeclarePanelOpen);

        vm.KeypadCommand.Execute("1");
        vm.KeypadCommand.Execute("2");
        vm.SelectScrapFieldCommand.Execute(parameter: null);
        vm.KeypadCommand.Execute("3");
        vm.ChooseDefectReasonCommand.Execute(vm.DefectReasons.First(reason => reason.Value == MesDefectReason.Surface));
        await vm.ConfirmDeclarationCommand.ExecuteAsync(parameter: null);

        sender.Verify(
            s => s.Send(
                It.Is<ExecuteMesCommandCommand>(c => c.Command.Action == MesAction.DeclareProduction
                    && c.Command.Amount(MesCommand.GoodField) == 12m
                    && c.Command.Amount(MesCommand.ScrapField) == 3m
                    && string.Equals(c.Command.Text(MesCommand.DefectField), "surface", StringComparison.Ordinal)),
                Any),
            Times.Once);
        Assert.False(vm.IsAnyPanelOpen);
    }

    /// <summary>
    /// A command the service rejects keeps the panel open and shows an error, so the operator can fix the entry.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Declaration_Rejected_KeepsThePanelOpenWithAnError()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<ExecuteMesCommandCommand>(), Any))
            .ReturnsAsync(new MesExecuteResult(MesExecuteStatus.Rejected, MesOperatorService.DefectReasonKey));
        var vm = await LoadedAsync();
        vm.OpenDeclarationCommand.Execute(vm.Phases[0]);
        vm.SelectScrapFieldCommand.Execute(parameter: null);
        vm.KeypadCommand.Execute("2");

        await vm.ConfirmDeclarationCommand.ExecuteAsync(parameter: null);

        Assert.True(vm.IsDeclarePanelOpen);
        Assert.True(vm.IsError);
        Assert.Equal(loc[MesOperatorService.DefectReasonKey], vm.StatusMessage);
    }

    /// <summary>
    /// A queued command (offline) closes the panel and tells the operator it was saved.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Execute_Queued_ClosesThePanelAndShowsTheQueuedMessage()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<ExecuteMesCommandCommand>(), Any)).ReturnsAsync(new MesExecuteResult(MesExecuteStatus.Queued));
        var vm = await LoadedAsync();
        vm.OpenCloseCommand.Execute(vm.Phases[0]);

        await vm.ConfirmCloseCommand.ExecuteAsync(parameter: null);

        Assert.False(vm.IsAnyPanelOpen);
        Assert.False(vm.IsError);
        Assert.Equal(loc["Mes_QueuedOffline"], vm.StatusMessage);
    }

    /// <summary>
    /// A lot above the requirement needs the operator's confirmation; with it, the command is sent with the lot in upper case.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Lot_OverConsumption_NeedsConfirmationThenSends()
    {
        var vm = await LoadedAsync();
        vm.OpenLotCommand.Execute(vm.Phases[0]);
        Assert.Equal(itemId, vm.SelectedComponent?.ItemId);
        vm.LotCode = " lot-77 ";
        vm.KeypadCommand.Execute("7");

        await vm.ConfirmLotCommand.ExecuteAsync(parameter: null);

        sender.Verify(s => s.Send(It.IsAny<ExecuteMesCommandCommand>(), Any), Times.Never);
        Assert.True(vm.NeedsOverConsumptionConfirm);
        Assert.True(vm.IsLotPanelOpen);

        vm.OverConsumptionConfirmed = true;
        await vm.ConfirmLotCommand.ExecuteAsync(parameter: null);

        sender.Verify(
            s => s.Send(
                It.Is<ExecuteMesCommandCommand>(c => c.Command.Action == MesAction.ConsumeMaterial
                    && string.Equals(c.Command.Text(MesCommand.LotField), "LOT-77", StringComparison.Ordinal)
                    && c.Command.Amount(MesCommand.QuantityField) == 7m
                    && string.Equals(c.Command.Text("over_consumption_confirmed"), "True", StringComparison.Ordinal)),
                Any),
            Times.Once);
        Assert.False(vm.IsAnyPanelOpen);
    }

    /// <summary>
    /// Choosing a reason tile suspends the phase with that reason.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Suspend_ReasonTile_SendsTheSuspension()
    {
        var vm = await LoadedAsync();
        vm.OpenSuspendCommand.Execute(vm.Phases[0]);
        Assert.True(vm.IsSuspendPanelOpen);
        Assert.Equal(Enum.GetValues<MesSuspensionReason>().Length, vm.SuspensionReasons.Count);

        await vm.ConfirmSuspendCommand.ExecuteAsync(vm.SuspensionReasons.First(reason => reason.Value == MesSuspensionReason.QualityHold));

        sender.Verify(
            s => s.Send(
                It.Is<ExecuteMesCommandCommand>(c => c.Command.Action == MesAction.Suspend
                    && string.Equals(c.Command.Text(MesCommand.ReasonField), "quality_hold", StringComparison.Ordinal)),
                Any),
            Times.Once);
        Assert.False(vm.IsAnyPanelOpen);
    }

    /// <summary>
    /// Without a signed-in user a take-over is not attempted.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task TakeOver_NotSignedIn_ShowsAnErrorAndSendsNothing()
    {
        var vm = await LoadedAsync();

        await vm.TakeOverCommand.ExecuteAsync(vm.Phases[1]);

        Assert.True(vm.IsError);
        sender.Verify(s => s.Send(It.IsAny<ExecuteMesCommandCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// A server answer that disappears between actions (a refused command) lists it as a conflict the operator can dismiss.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Conflicts_FromTheBoard_AreListedAndCanBeDismissed()
    {
        var conflict = new MesConflict(Guid.NewGuid(), MesAction.Suspend, Start, "Phase already closed.");
        board = board with { Conflicts = [conflict] };
        _ = sender.Setup(s => s.Send(It.IsAny<DismissMesConflictCommand>(), Any)).Callback(() => board = board with { Conflicts = [] });
        var vm = await LoadedAsync();
        var item = Assert.Single(vm.Conflicts);
        Assert.True(vm.HasConflicts);
        Assert.Equal("Phase already closed.", item.Error);

        await vm.DismissConflictCommand.ExecuteAsync(item);

        Assert.Empty(vm.Conflicts);
        Assert.False(vm.HasConflicts);
    }

    private async Task<OperatorBoardViewModel> LoadedAsync()
    {
        OperatorBoardViewModel vm = new(sender.Object, loc, user.Object, clock, network);
        await vm.LoadCommand.ExecuteAsync(parameter: null);

        return vm;
    }

    private sealed class FakeNetwork : INetworkStatus
    {
#pragma warning disable CS0067 // The fake never changes its status.
        public event EventHandler? Changed;
#pragma warning restore CS0067

        public bool IsOnline => true;
    }

    private sealed class MutableTime(DateTime start) : TimeProvider
    {
        public DateTime Now { get; set; } = start;

        public override DateTimeOffset GetUtcNow() => new(DateTime.SpecifyKind(Now, DateTimeKind.Utc));
    }
}
