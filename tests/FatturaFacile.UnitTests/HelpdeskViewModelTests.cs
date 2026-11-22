// <copyright file="HelpdeskViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using FatturaFacile.Application.Features.Helpdesk;
using FatturaFacile.Application.Helpdesk;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the ticket workbench view model: SLA-sorted list with breach indicators, filters, offline and queue display, the
/// customer 360 panel, the knowledge base lookup and the actions it opens.
/// </summary>
public sealed class HelpdeskViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly DateTime SyncedAt = new(2027, 1, 10, 9, 0, 0, DateTimeKind.Utc);
    private static readonly Guid Breached = new(0x22222222, 0x2222, 0x2222, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22);
    private static readonly Guid Risky = new(0x33333333, 0x3333, 0x3333, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33);
    private static readonly Guid Quiet = new(0x11111111, 0x1111, 0x1111, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11);

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private readonly ManualTimeProvider time = new();
    private HelpdeskWorkbench workbench;
    private HelpdeskLoad<HelpdeskCustomerContext>? context;
    private HelpdeskLoad<IReadOnlyList<HelpdeskEscalation>>? escalations;
    private HelpdeskLoad<IReadOnlyList<HelpdeskKbArticle>>? articles;

    /// <summary>
    /// Initializes a new instance of the <see cref="HelpdeskViewModelTests"/> class.
    /// </summary>
    public HelpdeskViewModelTests()
    {
        workbench = new HelpdeskWorkbench([Ticket(Breached, "TCK-2", "Server room offline", "high", 120, breached: true), Ticket(Risky, "TCK-3", "Slow VPN", "high", 95, breached: false), Ticket(Quiet, "TCK-1", "Printer toner", "low", 80, breached: false)], FromCache: false, SyncedAt, [], []);
        _ = sender.Setup(s => s.Send(It.IsAny<GetHelpdeskWorkbenchQuery>(), Any)).ReturnsAsync(() => workbench);
        _ = sender.Setup(s => s.Send(It.IsAny<GetHelpdeskCustomerContextQuery>(), Any)).ReturnsAsync(() => context);
        _ = sender.Setup(s => s.Send(It.IsAny<GetHelpdeskEscalationsQuery>(), Any)).ReturnsAsync(() => escalations);
        _ = sender.Setup(s => s.Send(It.IsAny<SearchHelpdeskKnowledgeQuery>(), Any)).ReturnsAsync(() => articles);
        _ = sender.Setup(s => s.Send(It.IsAny<DismissHelpdeskConflictCommand>(), Any)).Returns(Task.CompletedTask);
        _ = sender.Setup(s => s.Send(It.IsAny<FlushHelpdeskOutboxCommand>(), Any)).ReturnsAsync(new HelpdeskFlushResult(1, 0, 0, Offline: false));
    }

    /// <summary>
    /// The list shows the tickets in the order of the workbench with the breach state, the risk and the targets at risk.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ShowsTicketsWithTheirSlaState()
    {
        var vm = Create();

        await vm.LoadAsync();

        Assert.Equal(3, vm.Rows.Count);
        var first = vm.Rows[0];
        Assert.Equal("TCK-2", first.Number);
        Assert.Equal("Breached", first.State);
        Assert.Equal("120%", first.RiskText);
        Assert.Equal(loc["Helpdesk_Target_resolution"], first.TargetsText);
        Assert.Equal("AtRisk", vm.Rows[1].State);
        Assert.False(vm.IsOffline);
        Assert.Contains(SyncedAt.ToLocalTime().ToString("g", loc.CurrentCulture), vm.LastSyncedText, StringComparison.Ordinal);
        sender.Verify(s => s.Send(It.Is<GetHelpdeskWorkbenchQuery>(q => q.RiskPercent == 75), Any), Times.Once);
    }

    /// <summary>
    /// Offline, the cached list stays on screen with a notice, and the queued actions are listed and marked on their ticket.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_Offline_ShowsTheNoticeTheQueueAndTheQueuedMark()
    {
        var pending = new HelpdeskPendingAction(Guid.NewGuid(), HelpdeskAction.LogTime, Risky, "TCK-3", SyncedAt, null);
        workbench = workbench with { FromCache = true, Pending = [pending] };
        var vm = Create();

        await vm.LoadAsync();

        Assert.True(vm.IsOffline);
        Assert.Equal(loc["Helpdesk_OfflineCache"], vm.StatusMessage);
        Assert.True(vm.HasPending);
        Assert.False(vm.HasConflicts);
        var row = Assert.Single(vm.Queue);
        Assert.Equal(loc["Helpdesk_Action_LogTime"], row.ActionText);
        Assert.Equal("TCK-3", row.TicketNumber);
        Assert.True(vm.Rows.Single(r => r.Ticket.TicketId == Risky).HasPending);
        Assert.False(vm.Rows.Single(r => r.Ticket.TicketId == Breached).HasPending);
    }

    /// <summary>
    /// Nothing ever synced shows the never-synced text and an empty list.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_NeverSynced_ShowsAnEmptyListAndTheNeverSyncedText()
    {
        workbench = new HelpdeskWorkbench([], FromCache: false, SyncedAtUtc: null, [], []);
        var vm = Create();

        await vm.LoadAsync();

        Assert.Empty(vm.Rows);
        Assert.True(vm.IsOffline);
        Assert.Equal(loc["Helpdesk_NeverSynced"], vm.LastSyncedText);
    }

    /// <summary>
    /// The filters narrow the list by priority, SLA state, text and minimum risk.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Filters_NarrowTheList()
    {
        var vm = Create();
        await vm.LoadAsync();

        Assert.Equal(3, vm.PriorityChoices.Count);
        vm.SelectedPriority = vm.PriorityChoices.Single(choice => string.Equals(choice.Key, "high", StringComparison.Ordinal));
        Assert.Equal(2, vm.Rows.Count);

        vm.SelectedState = vm.StateChoices.Single(choice => string.Equals(choice.Key, "Breached", StringComparison.Ordinal));
        Assert.Equal("TCK-2", Assert.Single(vm.Rows).Number);

        vm.SelectedState = vm.StateChoices[0];
        vm.SearchText = "vpn";
        Assert.Equal("TCK-3", Assert.Single(vm.Rows).Number);

        vm.SearchText = string.Empty;
        vm.SelectedPriority = vm.PriorityChoices[0];
        vm.MinRiskPercent = 100;
        Assert.Equal("TCK-2", Assert.Single(vm.Rows).Number);
    }

    /// <summary>
    /// Selecting a ticket loads the customer 360 panel and the escalation history, and prefills the knowledge base query.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Select_LoadsTheCustomerPanelAndTheEscalations()
    {
        context = new HelpdeskLoad<HelpdeskCustomerContext>(Context(remaining: 2.5m), FromCache: false, SyncedAt);
        escalations = new HelpdeskLoad<IReadOnlyList<HelpdeskEscalation>>([new HelpdeskEscalation(null, "response", 75, 76, "notify", "sent", SyncedAt)], FromCache: false, SyncedAt);
        var vm = Create();
        await vm.LoadAsync();

        vm.SelectedTicket = vm.Rows.Single(row => row.Ticket.TicketId == Risky);

        Assert.True(vm.HasSelection);
        Assert.True(vm.HasContext);
        Assert.Equal(string.Empty, vm.ContextNote);
        Assert.Equal(3, vm.Contracts.Count);
        Assert.Equal("Gold", vm.Contracts[0].Name);
        Assert.False(vm.Contracts[0].IsLow);
        Assert.True(vm.Contracts[1].IsLow);
        Assert.False(vm.Contracts[2].IsLow);
        Assert.Equal(loc["Helpdesk_ContractNoBank"], vm.Contracts[2].BankText);
        Assert.True(vm.Assets.Single(asset => string.Equals(asset.SerialNumber, "SN-OLD", StringComparison.Ordinal)).IsExpired);
        Assert.False(vm.Assets.Single(asset => string.Equals(asset.SerialNumber, "SN-NEW", StringComparison.Ordinal)).IsExpired);
        Assert.Contains("3", vm.InvoicesText, StringComparison.Ordinal);
        Assert.Equal("TCK-0 Old one", Assert.Single(vm.RecentTickets).Text);
        Assert.True(vm.HasEscalations);
        Assert.Equal("Slow VPN", vm.KnowledgeQuery);
    }

    /// <summary>
    /// Without a connection and a cached context the panel says so, and a cached context is marked as such.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Select_WithoutContext_ShowsTheNoteAndWithCacheMarksIt()
    {
        var vm = Create();
        await vm.LoadAsync();

        vm.SelectedTicket = vm.Rows[0];
        Assert.False(vm.HasContext);
        Assert.Equal(loc["Helpdesk_NoContext"], vm.ContextNote);
        Assert.False(vm.HasEscalations);

        context = new HelpdeskLoad<HelpdeskCustomerContext>(Context(remaining: 1m), FromCache: true, SyncedAt);
        vm.SelectedTicket = vm.Rows[1];
        Assert.True(vm.HasContext);
        Assert.Equal(loc["Helpdesk_ContextCached"], vm.ContextNote);
    }

    /// <summary>
    /// Clearing the selection clears the side panel and closes the open dialogs.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Deselect_ClearsThePanelAndTheDialogs()
    {
        context = new HelpdeskLoad<HelpdeskCustomerContext>(Context(remaining: 2.5m), FromCache: false, SyncedAt);
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedTicket = vm.Rows[0];
        vm.BeginTimeLogCommand.Execute(null);

        vm.SelectedTicket = null;

        Assert.False(vm.HasSelection);
        Assert.False(vm.HasContext);
        Assert.Empty(vm.Contracts);
        Assert.Null(vm.TimeLog);
        Assert.False(vm.IsTimeLogOpen);
    }

    /// <summary>
    /// The time log dialog opens on the selected ticket with the hours left on the contract bank, and shows the expected settlement.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task BeginTimeLog_UsesTheContractBankForThePreview()
    {
        context = new HelpdeskLoad<HelpdeskCustomerContext>(Context(remaining: 2.5m), FromCache: false, SyncedAt);
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedTicket = vm.Rows[0];

        vm.BeginTimeLogCommand.Execute(null);
        Assert.NotNull(vm.TimeLog);
        vm.TimeLog.MinutesText = "30";

        var culture = loc.CurrentCulture;
        var expected = string.Format(culture, loc["Helpdesk_Time_Preview"], string.Format(culture, loc["Helpdesk_Time_ContractDeducted"], 0.5m.ToString("0.##", culture)));
        Assert.True(vm.IsTimeLogOpen);
        Assert.Equal("TCK-2", vm.TimeLog.TicketNumber);
        Assert.Equal(expected, vm.TimeLog.PreviewText);

        vm.CloseTimeLogCommand.Execute(null);
        Assert.Null(vm.TimeLog);
    }

    /// <summary>
    /// The escalation dialog offers the serial numbers of the assets of the customer.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task BeginEscalation_OffersTheAssetSerials()
    {
        context = new HelpdeskLoad<HelpdeskCustomerContext>(Context(remaining: 2.5m), FromCache: false, SyncedAt);
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedTicket = vm.Rows[0];

        vm.BeginEscalationCommand.Execute(null);

        Assert.NotNull(vm.Escalation);
        Assert.True(vm.IsEscalationOpen);
        DssTestData.SameStrings(["SN-OLD", "SN-NEW"], vm.Escalation.Serials);
        vm.CloseEscalationCommand.Execute(null);
        Assert.Null(vm.Escalation);
    }

    /// <summary>
    /// The knowledge base lookup uses the locale of the language of the application and lists the articles found.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SearchKnowledge_UsesTheLocaleOfTheApplicationLanguage()
    {
        articles = new HelpdeskLoad<IReadOnlyList<HelpdeskKbArticle>>([new HelpdeskKbArticle("a1", "Reset the router", "Hold the button")], FromCache: false, SyncedAt);
        var vm = Create();
        vm.KnowledgeQuery = "router";

        await vm.SearchKnowledgeAsync();

        var locale = HelpdeskKnowledgeLocale.From(loc.CurrentCulture);
        sender.Verify(s => s.Send(It.Is<SearchHelpdeskKnowledgeQuery>(q => q.Text == "router" && q.Locale == locale), Any), Times.Once);
        Assert.Equal("Reset the router", Assert.Single(vm.KnowledgeResults).Title);
        Assert.Equal(string.Empty, vm.KnowledgeStatus);
    }

    /// <summary>
    /// A cached search is marked as such, an empty search says so, and a search never made offline says it is unavailable.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SearchKnowledge_ExplainsCacheEmptyAndOffline()
    {
        var vm = Create();
        vm.KnowledgeQuery = "router";

        articles = new HelpdeskLoad<IReadOnlyList<HelpdeskKbArticle>>([new HelpdeskKbArticle("a1", "Reset", string.Empty)], FromCache: true, SyncedAt);
        await vm.SearchKnowledgeAsync();
        Assert.Equal(loc["Helpdesk_KbCached"], vm.KnowledgeStatus);

        articles = new HelpdeskLoad<IReadOnlyList<HelpdeskKbArticle>>([], FromCache: false, SyncedAt);
        await vm.SearchKnowledgeAsync();
        Assert.Equal(loc["Helpdesk_KbNoResults"], vm.KnowledgeStatus);
        Assert.Empty(vm.KnowledgeResults);

        articles = null;
        await vm.SearchKnowledgeAsync();
        Assert.Equal(loc["Helpdesk_KbOffline"], vm.KnowledgeStatus);
    }

    /// <summary>
    /// Dismissing a refused action sends the command and reloads the list.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Dismiss_SendsTheCommandAndReloads()
    {
        var entryId = Guid.NewGuid();
        workbench = workbench with { Conflicts = [new HelpdeskPendingAction(entryId, HelpdeskAction.EscalateRma, Risky, "TCK-3", SyncedAt, "No unit")] };
        var vm = Create();
        await vm.LoadAsync();
        var row = Assert.Single(vm.Conflicts);
        Assert.True(vm.HasConflicts);
        Assert.Equal("No unit", row.Error);

        workbench = workbench with { Conflicts = [] };
        await vm.DismissCommand.ExecuteAsync(row);

        sender.Verify(s => s.Send(It.Is<DismissHelpdeskConflictCommand>(c => c.EntryId == entryId), Any), Times.Once);
        Assert.False(vm.HasConflicts);
    }

    /// <summary>
    /// Sending the queue now flushes the outbox and reloads the list.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task FlushNow_SendsTheQueueAndReloads()
    {
        var vm = Create();

        await vm.FlushCommand.ExecuteAsync(null);

        sender.Verify(s => s.Send(It.IsAny<FlushHelpdeskOutboxCommand>(), Any), Times.Once);
        sender.Verify(s => s.Send(It.IsAny<GetHelpdeskWorkbenchQuery>(), Any), Times.Once);
        Assert.Equal(3, vm.Rows.Count);
    }

    /// <summary>
    /// A user without the permission sees the reason instead of a failure.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_WithoutPermission_ShowsTheReason()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetHelpdeskWorkbenchQuery>(), Any)).ThrowsAsync(new UnauthorizedAccessException("No access"));
        var vm = Create();

        await vm.LoadAsync();

        Assert.Equal("No access", vm.StatusMessage);
        Assert.False(vm.IsBusy);
    }

    private static HelpdeskTicketRisk Ticket(Guid id, string number, string subject, string priority, double risk, bool breached) =>
        new(
            id,
            number,
            subject,
            priority,
            "open",
            CustomerId: null,
            AssignedTeamId: null,
            AssignedTechnicianId: null,
            risk,
            [new HelpdeskBreachTarget(breached ? "resolution" : "response", new DateTime(2027, 1, 10, 10, 0, 0, DateTimeKind.Utc), risk, breached ? HelpdeskBreachTarget.Breached : HelpdeskBreachTarget.AtRisk)]);

    private static HelpdeskCustomerContext Context(decimal remaining) =>
        new(
            null,
            [
                new HelpdeskContract(null, "Gold", "support", 10m, 10m - remaining, remaining, null),
                new HelpdeskContract(null, "Silver", "support", 5m, 5m, 0m, null),
                new HelpdeskContract(null, "Time and material", "t_and_m", null, null, null, null),
            ],
            [
                new HelpdeskAsset(null, "SN-OLD", new DateOnly(2020, 1, 1)),
                new HelpdeskAsset(null, "SN-NEW", new DateOnly(2099, 1, 1)),
            ],
            new HelpdeskOpenInvoices(3, 1, 1250.5m),
            [new HelpdeskRecentTicket(null, "TCK-0", "Old one", "closed")]);

    private HelpdeskViewModel Create() => new(sender.Object, loc, time);
}
