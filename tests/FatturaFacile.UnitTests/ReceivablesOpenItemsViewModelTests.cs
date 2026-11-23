// <copyright file="ReceivablesOpenItemsViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using System.Globalization;
using CommunityToolkit.Mvvm.Input;
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
/// Tests of the open items view model: rows and paging, filters, the bucket drill-down, the allocations, the reconciliation dialog, the dispute and the reversal.
/// </summary>
public sealed class ReceivablesOpenItemsViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly Guid CustomerId = ReceivablesFixtures.CustomerId;

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private readonly ManualTimeProvider time = new();
    private readonly ReceivableItem overdue = ReceivablesFixtures.Item("INV-1", 400m, 40, ReceivableStatus.Overdue, original: 1000m);
    private int changes;
    private ReceivablesLoad<ReceivableItemPage>? page;
    private ReceivablesActionResult result = ReceivablesFixtures.Sent();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivablesOpenItemsViewModelTests"/> class.
    /// </summary>
    public ReceivablesOpenItemsViewModelTests()
    {
        page = ReceivablesFixtures.Load(new ReceivableItemPage([overdue, ReceivablesFixtures.Item("INV-2", 100m, -5), ReceivablesFixtures.Item("INV-3", 0m, 100, ReceivableStatus.Paid, original: 50m)], 1, 3, 120));
        _ = sender.Setup(s => s.Send(It.IsAny<GetReceivableItemsQuery>(), Any)).ReturnsAsync(() => page);
        _ = sender.Setup(s => s.Send(It.IsAny<GetReceivableItemQuery>(), Any)).ReturnsAsync(() => ReceivablesFixtures.Load(overdue with { Allocations = [ReceivablesFixtures.Allocation(overdue.Id, 600m), ReceivablesFixtures.Allocation(overdue.Id, 50m, reversed: true)] }));
        _ = sender.Setup(s => s.Send(It.IsAny<GetUnreconciledPaymentsQuery>(), Any)).ReturnsAsync(ReceivablesFixtures.Load<IReadOnlyList<UnreconciledPayment>>([new UnreconciledPayment(Guid.NewGuid(), "BANK-1", ReceivablesFixtures.Today, "EUR", 500m, 60m)]));
        _ = sender.Setup(s => s.Send(It.IsAny<SetReceivableDisputeCommand>(), Any)).ReturnsAsync(() => result);
        _ = sender.Setup(s => s.Send(It.IsAny<ReverseReceivableAllocationCommand>(), Any)).ReturnsAsync(() => result);
    }

    private ReceivablesOpenItemsViewModel Create() => new(sender.Object, loc, time, () =>
    {
        changes++;

        return Task.CompletedTask;
    });

    /// <summary>
    /// The list shows the items with their state, the days past due and the page position.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ShowsTheRowsWithStateAndPaging()
    {
        var vm = Create();
        var culture = loc.CurrentCulture;

        await vm.LoadAsync(null, null);

        Assert.Equal(3, vm.Rows.Count);
        Assert.Equal("INV-1", vm.Rows[0].Number);
        Assert.Equal("Overdue", vm.Rows[0].State);
        Assert.Equal("40", vm.Rows[0].DaysText);
        Assert.Equal(ReceivablesFormat.Money(400m, culture), vm.Rows[0].OpenText);
        Assert.Equal(loc["Receivables_Status_overdue"], vm.Rows[0].StatusText);
        Assert.Equal("-", vm.Rows[1].DaysText);
        Assert.Equal("Closed", vm.Rows[2].State);
        Assert.Equal(string.Format(culture, loc["Receivables_PageOf"], "1", "3", "120"), vm.PageText);
        Assert.Equal(3, vm.LastPage);
        Assert.Equal(string.Empty, vm.NoticeText);
        sender.Verify(s => s.Send(It.Is<GetReceivableItemsQuery>(q => q.Filter.OpenOnly && q.Filter.Page == 1 && q.Filter.PerPage == 50 && q.Filter.CustomerId == null && q.Filter.Bucket == null), Any), Times.Once);
    }

    /// <summary>
    /// A cached page comes with the offline notice; with nothing cached the list says there is no data.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_Offline_ShowsTheCacheNotice_AndNoDataWhenThereIsNone()
    {
        page = page! with { FromCache = true };
        var vm = Create();

        await vm.LoadAsync(null, null);
        Assert.Equal(loc["Receivables_OfflineCache"], vm.NoticeText);
        Assert.Equal(3, vm.Rows.Count);

        page = null;
        await vm.LoadAsync(null, null);
        Assert.Empty(vm.Rows);
        Assert.Equal(loc["Receivables_NoData"], vm.StatusMessage);
    }

    /// <summary>
    /// Applying the filters sends the status, the document type, the due dates and the open-only switch, from page one.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ApplyFilters_SendsTheFiltersFromTheFirstPage()
    {
        var vm = Create();
        await vm.LoadAsync(CustomerId, "CHF");
        vm.SelectedStatus = vm.StatusChoices.First(choice => choice.Key == "overdue");
        vm.DocumentType = " credit_note ";
        vm.OpenOnly = false;
        vm.DueFrom = new DateTime(2027, 1, 1);
        vm.DueTo = new DateTime(2027, 3, 31);
        vm.Page = 3;

        await vm.ApplyFiltersCommand.ExecuteAsync(null);

        sender.Verify(s => s.Send(It.Is<GetReceivableItemsQuery>(q =>
            q.Filter.Status == ReceivableStatus.Overdue
            && q.Filter.DocumentType == "credit_note"
            && !q.Filter.OpenOnly
            && q.Filter.DueFrom == new DateOnly(2027, 1, 1)
            && q.Filter.DueTo == new DateOnly(2027, 3, 31)
            && q.Filter.Currency == "CHF"
            && q.Filter.CustomerId == CustomerId
            && q.Filter.Page == 1), Any), Times.Once);
    }

    /// <summary>
    /// Next and previous move one page and stop at the ends.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Paging_NextAndPrevious_StayInsideThePages()
    {
        var vm = Create();
        await vm.LoadAsync(null, null);

        await vm.PreviousPageCommand.ExecuteAsync(null);
        await vm.NextPageCommand.ExecuteAsync(null);

        Assert.Equal(2, vm.Page);
        sender.Verify(s => s.Send(It.Is<GetReceivableItemsQuery>(q => q.Filter.Page == 2), Any), Times.Once);
        vm.Page = 3;
        await vm.NextPageCommand.ExecuteAsync(null);
        Assert.Equal(3, vm.Page);
    }

    /// <summary>
    /// The drill-down of an aging figure filters by bucket and customer and lists the open items only.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task DrillDown_SetsTheBucketAndTheCustomer()
    {
        var vm = Create();
        vm.SetBuckets(new AgingBucketEngine().Buckets);
        vm.OpenOnly = false;

        await vm.DrillDownAsync(CustomerId, "61_90");

        Assert.Equal(7, vm.BucketChoices.Count);
        Assert.Equal("61_90", vm.SelectedBucket?.Key);
        Assert.True(vm.OpenOnly);
        sender.Verify(s => s.Send(It.Is<GetReceivableItemsQuery>(q => q.Filter.Bucket == "61_90" && q.Filter.CustomerId == CustomerId && q.Filter.OpenOnly), Any), Times.Once);
    }

    /// <summary>
    /// Selecting an item loads its allocations; a reversed one cannot be reversed again.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SelectRow_LoadsTheAllocationsOfTheItem()
    {
        var vm = Create();
        await vm.LoadAsync(null, null);

        vm.SelectedRow = vm.Rows[0];

        Assert.True(vm.HasSelection);
        Assert.Equal(2, vm.Allocations.Count);
        Assert.True(vm.Allocations[0].CanReverse);
        Assert.Equal(string.Empty, vm.Allocations[0].StateText);
        Assert.False(vm.Allocations[1].CanReverse);
        Assert.Equal(loc["Receivables_Reversed"], vm.Allocations[1].StateText);
        Assert.Equal(loc["Receivables_Type_payment"], vm.Allocations[0].TypeText);
    }

    /// <summary>
    /// The reconcile button loads the unreconciled payments of the customer and opens the dialog for the selected item.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task BeginReconcile_LoadsThePaymentsAndOpensTheDialog()
    {
        var vm = Create();
        await vm.LoadAsync(null, null);
        vm.SelectedRow = vm.Rows[0];

        await vm.BeginReconcileCommand.ExecuteAsync(null);

        Assert.True(vm.IsReconcileOpen);
        Assert.NotNull(vm.Reconcile);
        Assert.Equal("INV-1", vm.Reconcile.DocumentNumber);
        Assert.Equal(2, vm.Reconcile.PaymentChoices.Count);
        sender.Verify(s => s.Send(It.Is<GetUnreconciledPaymentsQuery>(q => q.CustomerId == CustomerId), Any), Times.Once);

        vm.CloseReconcileCommand.Execute(null);
        Assert.False(vm.IsReconcileOpen);
        Assert.Null(vm.Reconcile);
    }

    /// <summary>
    /// Marking an item as disputed sends the reason, closes the panel and refreshes every view.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task MarkDisputed_SendsTheReasonAndRefreshesTheViews()
    {
        var vm = Create();
        await vm.LoadAsync(null, null);
        vm.SelectedRow = vm.Rows[0];
        vm.BeginDisputeCommand.Execute(null);
        vm.DisputeReason = "Wrong VAT";

        await vm.MarkDisputedCommand.ExecuteAsync(null);

        Assert.False(vm.IsDisputeOpen);
        Assert.Equal(1, changes);
        Assert.Equal(loc["Receivables_Sent"], vm.ActionText);
        Assert.Equal(ReceivablesFormat.Good, vm.ActionState);
        sender.Verify(s => s.Send(It.Is<SetReceivableDisputeCommand>(c => c.Request.Disputed && c.Request.DisputeReason == "Wrong VAT" && c.Request.ItemId == overdue.Id && c.Request.CustomerId == CustomerId), Any), Times.Once);
    }

    /// <summary>
    /// Clearing the dispute sends the cleared state and no reason.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ClearDispute_SendsTheStateWithoutAReason()
    {
        var vm = Create();
        await vm.LoadAsync(null, null);
        vm.SelectedRow = vm.Rows[0];
        vm.BeginDisputeCommand.Execute(null);
        vm.DisputeReason = "ignored";

        await vm.ClearDisputeCommand.ExecuteAsync(null);

        sender.Verify(s => s.Send(It.Is<SetReceivableDisputeCommand>(c => !c.Request.Disputed && c.Request.DisputeReason == null), Any), Times.Once);
    }

    /// <summary>
    /// Reversing an allocation sends it with the customer and refreshes the views.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Reverse_SendsTheReversalOfTheAllocation()
    {
        var vm = Create();
        await vm.LoadAsync(null, null);
        vm.SelectedRow = vm.Rows[0];
        var allocation = vm.Allocations[0];

        await vm.ReverseCommand.ExecuteAsync(allocation);

        sender.Verify(s => s.Send(It.Is<ReverseReceivableAllocationCommand>(c => c.Request.AllocationId == allocation.Allocation.Id && c.Request.CustomerId == CustomerId), Any), Times.Once);
        Assert.Equal(1, changes);
    }

    /// <summary>
    /// A refused action shows its error in red and does not refresh the views.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Reverse_Refused_ShowsTheErrorAndDoesNotRefresh()
    {
        result = new ReceivablesActionResult(ReceivablesActionStatus.Rejected, ReceivablesService.LinesKey);
        var vm = Create();
        await vm.LoadAsync(null, null);
        vm.SelectedRow = vm.Rows[0];

        await vm.ReverseCommand.ExecuteAsync(vm.Allocations[0]);

        Assert.Equal(loc[ReceivablesService.LinesKey], vm.ActionText);
        Assert.Equal(ReceivablesFormat.Bad, vm.ActionState);
        Assert.Equal(0, changes);
    }
}
