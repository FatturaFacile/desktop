// <copyright file="ReceivablesViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using FatturaFacile.Application.Features.Receivables;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Receivables;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the receivables hub view model: the dashboard, offline and queue display, the customer selection and the drill-down from the aging.
/// </summary>
public sealed class ReceivablesViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly Guid CustomerId = ReceivablesFixtures.CustomerId;

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private readonly ManualTimeProvider time = new();
    private ReceivablesLoad<ReceivablesDashboard>? dashboard = ReceivablesFixtures.Load(ReceivablesFixtures.Dashboard());
    private ReceivablesLoad<AgingReport>? aging = ReceivablesFixtures.Load(ReceivablesFixtures.Aging());
    private ReceivablesQueue queue = new([], []);

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivablesViewModelTests"/> class.
    /// </summary>
    public ReceivablesViewModelTests()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<FlushReceivablesOutboxCommand>(), Any)).ReturnsAsync(new ReceivablesFlushResult(0, 0, 0, Offline: false));
        _ = sender.Setup(s => s.Send(It.IsAny<GetReceivablesDashboardQuery>(), Any)).ReturnsAsync(() => dashboard);
        _ = sender.Setup(s => s.Send(It.IsAny<GetReceivablesAgingQuery>(), Any)).ReturnsAsync(() => aging);
        _ = sender.Setup(s => s.Send(It.IsAny<GetReceivablesQueueQuery>(), Any)).ReturnsAsync(() => queue);
        _ = sender.Setup(s => s.Send(It.IsAny<GetReceivableItemsQuery>(), Any)).ReturnsAsync(ReceivablesFixtures.Page(ReceivablesFixtures.Item("INV-1", 300m, 40, ReceivableStatus.Overdue)));
        _ = sender.Setup(s => s.Send(It.IsAny<GetBadDebtProvisionQuery>(), Any)).ReturnsAsync(ReceivablesFixtures.Load(ReceivablesFixtures.Provision()));
        _ = sender.Setup(s => s.Send(It.IsAny<GetCustomerCreditStatusQuery>(), Any)).ReturnsAsync(ReceivablesFixtures.Load(ReceivablesFixtures.Credit()));
        _ = sender.Setup(s => s.Send(It.IsAny<DismissReceivablesConflictCommand>(), Any)).Returns(Task.CompletedTask);
    }

    /// <summary>
    /// Loading flushes the queue, then shows the dashboard figures, the aging cells, the customers to choose from and the blocked customers.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ShowsTheKpisTheAgingAndTheBlockedCustomers()
    {
        var vm = Create();
        var culture = loc.CurrentCulture;

        await vm.LoadAsync();

        Assert.Equal(ReceivablesFormat.Money(5000m, culture), vm.TotalExposureText);
        Assert.Equal(ReceivablesFormat.Money(900m, culture), vm.OverdueText);
        Assert.Equal(ReceivablesFormat.Money(300m, culture), vm.Over90Text);
        Assert.Equal(ReceivablesFormat.Money(70m, culture), vm.ProvisionText);
        Assert.Equal(string.Format(culture, loc["Receivables_DsoDays"], 41.5m.ToString("0.##", culture)), vm.DsoText);
        Assert.False(vm.IsOffline);
        Assert.Equal(string.Empty, vm.StatusMessage);
        Assert.Equal(6, vm.Aging.Cells.Count);
        Assert.Equal(ReceivablesFormat.Money(600m, culture), vm.Aging.Cells[0].AmountText);
        Assert.Equal(2, vm.CustomerChoices.Count);
        Assert.Equal("Rossi SA", vm.CustomerChoices[1].Label);
        var blocked = Assert.Single(vm.Blocked);
        Assert.Equal("Rossi SA", blocked.Name);
        Assert.True(vm.HasBlocked);
        Assert.Contains(ReceivablesFixtures.Synced.ToLocalTime().ToString("g", culture), vm.LastSyncedText, StringComparison.Ordinal);
        sender.Verify(s => s.Send(It.IsAny<FlushReceivablesOutboxCommand>(), Any), Times.Once);
    }

    /// <summary>
    /// Offline, the cached figures stay on screen with a notice, and the waiting and the refused actions are listed.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_Offline_ShowsTheNoticeAndTheQueues()
    {
        dashboard = dashboard! with { FromCache = true };
        var pending = new ReceivablesPendingAction(Guid.NewGuid(), ReceivablesAction.ApplyAllocations, "INV-1", ReceivablesFixtures.Synced, null);
        var refused = new ReceivablesPendingAction(Guid.NewGuid(), ReceivablesAction.SetDispute, "INV-2", ReceivablesFixtures.Synced, "Closed");
        queue = new ReceivablesQueue([pending], [refused]);
        var vm = Create();

        await vm.LoadAsync();

        Assert.True(vm.IsOffline);
        Assert.Equal(loc["Receivables_OfflineCache"], vm.StatusMessage);
        Assert.True(vm.HasPending);
        Assert.True(vm.HasConflicts);
        Assert.Equal(loc["Receivables_Action_ApplyAllocations"], Assert.Single(vm.Queue).ActionText);
        Assert.Equal("INV-1", vm.Queue[0].Reference);
        Assert.Equal("Closed", Assert.Single(vm.Conflicts).Error);
    }

    /// <summary>
    /// With nothing ever synced the hub is empty and says so.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_NeverSynced_ShowsTheNeverSyncedText()
    {
        dashboard = null;
        aging = null;
        var vm = Create();

        await vm.LoadAsync();

        Assert.True(vm.IsOffline);
        Assert.Equal(loc["Receivables_NeverSynced"], vm.LastSyncedText);
        Assert.Equal(string.Empty, vm.TotalExposureText);
        Assert.Empty(vm.Aging.Cells);
        Assert.Equal(loc["Receivables_NoData"], vm.Aging.StatusMessage);
        Assert.False(vm.HasBlocked);
    }

    /// <summary>
    /// The currency the user typed is normalized and sent with the dashboard, the aging and the provision queries.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_Currency_IsUppercasedAndPassedToTheQueries()
    {
        var vm = Create();
        vm.CurrencyText = "chf";

        await vm.LoadAsync();

        sender.Verify(s => s.Send(It.Is<GetReceivablesDashboardQuery>(q => q.Currency == "CHF"), Any), Times.Once);
        sender.Verify(s => s.Send(It.Is<GetReceivablesAgingQuery>(q => q.Currency == "CHF" && q.CustomerId == null), Any), Times.Once);
        sender.Verify(s => s.Send(It.Is<GetBadDebtProvisionQuery>(q => q.Currency == "CHF"), Any), Times.Once);
    }

    /// <summary>
    /// Choosing a customer filters the open items to it and loads its credit position and ledger.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SelectCustomer_LoadsTheViewsOfThatCustomer()
    {
        var vm = Create();
        await vm.LoadAsync();

        vm.SelectedCustomer = vm.CustomerChoices[1];

        sender.Verify(s => s.Send(It.Is<GetReceivableItemsQuery>(q => q.Filter.CustomerId == CustomerId), Any), Times.Once);
        sender.Verify(s => s.Send(It.Is<GetCustomerCreditStatusQuery>(q => q.CustomerId == CustomerId), Any), Times.Once);
        sender.Verify(s => s.Send(It.Is<GetCustomerLedgerQuery>(q => q.CustomerId == CustomerId), Any), Times.Once);
        Assert.True(vm.Credit.HasCustomer);
        Assert.True(vm.Ledger.HasCustomer);
    }

    /// <summary>
    /// Clicking a customer figure of the aging opens the items tab filtered by that customer and bucket.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task DrillDown_FromACustomerBucket_ShowsTheItemsOfThatBucket()
    {
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedTab = 1;
        var cell = vm.Aging.Customers[0].Cells.First(candidate => string.Equals(candidate.Key, "31_60", StringComparison.Ordinal));

        await vm.Aging.DrillDownCommand.ExecuteAsync(cell);

        Assert.Equal(0, vm.SelectedTab);
        Assert.Equal(CustomerId.ToString("D"), vm.SelectedCustomer?.Key);
        Assert.Equal("31_60", vm.OpenItems.SelectedBucket?.Key);
        sender.Verify(s => s.Send(It.Is<GetReceivableItemsQuery>(q => q.Filter.Bucket == "31_60" && q.Filter.CustomerId == CustomerId && q.Filter.OpenOnly), Any), Times.Once);
    }

    /// <summary>
    /// Clicking a company figure shows the items of that bucket for every customer.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task DrillDown_FromTheCompanyFigure_ClearsTheCustomer()
    {
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedCustomer = vm.CustomerChoices[1];
        var cell = vm.Aging.Cells.First(candidate => string.Equals(candidate.Key, "121_plus", StringComparison.Ordinal));

        await vm.Aging.DrillDownCommand.ExecuteAsync(cell);

        Assert.Equal(string.Empty, vm.SelectedCustomer?.Key);
        sender.Verify(s => s.Send(It.Is<GetReceivableItemsQuery>(q => q.Filter.Bucket == "121_plus" && q.Filter.CustomerId == null), Any), Times.Once);
    }

    /// <summary>
    /// Dismissing a refused action tells the service and refreshes the queues.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Dismiss_SendsTheCommandAndReloadsTheQueue()
    {
        var refused = new ReceivablesPendingAction(Guid.NewGuid(), ReceivablesAction.SetDispute, "INV-2", ReceivablesFixtures.Synced, "Closed");
        queue = new ReceivablesQueue([], [refused]);
        var vm = Create();
        await vm.LoadAsync();
        queue = new ReceivablesQueue([], []);

        await vm.DismissCommand.ExecuteAsync(vm.Conflicts[0]);

        sender.Verify(s => s.Send(It.Is<DismissReceivablesConflictCommand>(c => c.EntryId == refused.Id), Any), Times.Once);
        Assert.Empty(vm.Conflicts);
        Assert.False(vm.HasConflicts);
    }

    /// <summary>
    /// A customer id typed by the user is added to the list and selected; a bad id is reported.
    /// </summary>
    [Fact]
    public void SelectCustomerId_AddsAChoiceForACustomerNotInTheAging()
    {
        var vm = Create();
        var other = ReceivablesFixtures.OtherCustomerId;

        vm.CustomerIdText = "nonsense";
        vm.SelectCustomerIdCommand.Execute(null);
        Assert.Equal(loc["Receivables_Err_Customer"], vm.StatusMessage);

        vm.CustomerIdText = other.ToString("D");
        vm.SelectCustomerIdCommand.Execute(null);

        Assert.Equal(other.ToString("D"), vm.SelectedCustomer?.Key);
        sender.Verify(s => s.Send(It.Is<GetCustomerCreditStatusQuery>(q => q.CustomerId == other), Any), Times.Once);
    }

    /// <summary>
    /// The button of a blocked customer selects it and opens the credit tab.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ShowBlocked_OpensTheCreditTabOfThatCustomer()
    {
        var vm = Create();
        await vm.LoadAsync();

        vm.ShowBlockedCommand.Execute(vm.Blocked[0]);

        Assert.Equal(3, vm.SelectedTab);
        Assert.Equal(CustomerId.ToString("D"), vm.SelectedCustomer?.Key);
        Assert.True(vm.Credit.HasCustomer);
    }

    private ReceivablesViewModel Create() => new(sender.Object, loc, time);
}
