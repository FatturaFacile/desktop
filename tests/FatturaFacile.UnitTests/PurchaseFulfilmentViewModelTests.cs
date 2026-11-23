// <copyright file="PurchaseFulfilmentViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using FatturaFacile.Application.Features.Procurement;
using FatturaFacile.Application.Procurement;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the purchase order fulfilment workbench (Mod 046): the overdue list, the orders with the dual unit of measure and the
/// cascade, the supplier confirmation entry, the dispatch, the dropshipment DDT and delivery, the pricing preview and the offline notice.
/// </summary>
public sealed class PurchaseFulfilmentViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly Guid OrderId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid LineOneId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid LineTwoId = new("33333333-3333-3333-3333-333333333333");
    private static readonly Guid DropId = new("88888888-8888-8888-8888-888888888888");
    private static readonly DateTime Synced = new(2027, 4, 1, 8, 0, 0, DateTimeKind.Utc);

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private readonly ManualTimeProvider time = new();
    private PurchaseOrderView order = Order("sent");
    private bool fromCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="PurchaseFulfilmentViewModelTests"/> class.
    /// </summary>
    public PurchaseFulfilmentViewModelTests()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<FlushProcurementOutboxCommand>(), Any)).ReturnsAsync(new ProcurementFlushResult(0, 0, 0, Offline: false));
        _ = sender.Setup(s => s.Send(It.IsAny<GetOverdueOrdersQuery>(), Any)).ReturnsAsync(() => Load<IReadOnlyList<OverdueOrderView>>([new OverdueOrderView(order, 12, [LineOneId], 1, null, NextReminderDue: true)]));
        _ = sender.Setup(s => s.Send(It.IsAny<GetPurchaseOrdersQuery>(), Any)).ReturnsAsync(() => Load<IReadOnlyList<PurchaseOrderView>>([order]));
        _ = sender.Setup(s => s.Send(It.IsAny<GetPurchaseOrderQuery>(), Any)).ReturnsAsync(() => Load(order));
        _ = sender.Setup(s => s.Send(It.IsAny<GetOrderConfirmationsQuery>(), Any)).ReturnsAsync(Load<IReadOnlyList<SupplierConfirmationView>>([]));
        _ = sender.Setup(s => s.Send(It.IsAny<GetOrderDispatchesQuery>(), Any)).ReturnsAsync(Load<IReadOnlyList<DispatchView>>([]));
        _ = sender.Setup(s => s.Send(It.IsAny<GetOrderRemindersQuery>(), Any)).ReturnsAsync(Load<IReadOnlyList<ReminderView>>([]));
        _ = sender.Setup(s => s.Send(It.IsAny<GetDropshipmentsQuery>(), Any)).ReturnsAsync(() => Load<IReadOnlyList<DropshipmentView>>([Dropship("ddt_issued")]));
        _ = sender.Setup(s => s.Send(It.IsAny<GetDropshipmentQuery>(), Any)).ReturnsAsync(() => Load(Dropship("ddt_issued")));
        _ = sender.Setup(s => s.Send(It.IsAny<GetProcurementQueueQuery>(), Any)).ReturnsAsync(new ProcurementQueue([], []));
        _ = sender.Setup(s => s.Send(It.IsAny<DismissProcurementConflictCommand>(), Any)).Returns(Task.CompletedTask);
    }

    /// <summary>
    /// Loading replays the queue and shows the overdue orders and the orders, with the localized status including confirmed.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ShowsOverdueOrdersAndTheLocalizedStatus()
    {
        order = Order("confirmed");
        var vm = Create();

        await vm.LoadAsync();

        var overdue = Assert.Single(vm.OverdueOrders);
        Assert.Equal("PO-1", overdue.Number);
        Assert.Equal("12", overdue.DaysText);
        Assert.True(overdue.IsReminderDue);
        var row = Assert.Single(vm.Orders);
        Assert.Equal(loc["Procurement_Status_confirmed"], row.StatusText);
        Assert.Equal("AB-7", row.ConfirmationText);
        Assert.Equal(11, vm.StatusFilters.Count);
        Assert.False(vm.IsOffline);
        sender.Verify(s => s.Send(It.IsAny<FlushProcurementOutboxCommand>(), Any), Times.Once);
    }

    /// <summary>
    /// The data of the cache is flagged as offline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_FromCache_MarksTheViewOffline()
    {
        fromCache = true;
        var vm = Create();

        await vm.LoadAsync();

        Assert.True(vm.IsOffline);
        Assert.NotEqual(loc["Procurement_NeverSynced"], vm.LastSyncedText);
    }

    /// <summary>
    /// Selecting an order shows its lines with the quantity in both units of measure and the cascading discount.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SelectOrder_ShowsTheDualUnitOfMeasureAndTheCascade()
    {
        var vm = Create();
        await vm.LoadAsync();

        vm.SelectedOrder = vm.Orders[0];

        Assert.True(vm.HasSelectedOrder);
        Assert.Equal(2, vm.OrderLines.Count);
        var first = vm.OrderLines[0];
        Assert.Equal("10 BOX", first.QuantityText);
        Assert.Equal("120 PZ (1 BOX = 12 PZ)", first.BaseQuantityText);
        Assert.Equal("10% + 5%", first.CascadeText);
        Assert.Equal("5 PZ", vm.OrderLines[1].BaseQuantityText);
        Assert.Equal(vm.OrderLines.Count, vm.ConfirmationLines.Count);
    }

    /// <summary>
    /// A reminder is sent for the selected overdue order with its own command id.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Remind_SendsTheCommandForTheSelectedOverdueOrder()
    {
        SendOrderReminderCommand? captured = null;
        _ = sender.Setup(s => s.Send(It.IsAny<SendOrderReminderCommand>(), Any))
            .Callback<IRequest<ProcurementActionResult>, CancellationToken>((command, _) => captured = (SendOrderReminderCommand)command)
            .ReturnsAsync(new ProcurementActionResult(ProcurementActionStatus.Sent));
        var vm = Create();
        await vm.LoadAsync();

        Assert.False(vm.RemindCommand.CanExecute(null));
        vm.SelectedOverdue = vm.OverdueOrders[0];
        Assert.True(vm.RemindCommand.CanExecute(null));
        await vm.RemindCommand.ExecuteAsync(null);

        Assert.NotNull(captured);
        Assert.Equal(OrderId, captured.Request.OrderId);
        Assert.NotEqual(Guid.Empty, captured.Request.CommandId);
        Assert.Equal(loc["Procurement_Done_Sent"], vm.StatusMessage);
    }

    /// <summary>
    /// An order is dispatched only while it is approved or sent, never once the supplier confirmed it.
    /// </summary>
    /// <param name="status">The wire status.</param>
    /// <param name="expected">Whether dispatch is available.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData("approved", true)]
    [InlineData("sent", true)]
    [InlineData("confirmed", false)]
    [InlineData("draft", false)]
    public async Task Dispatch_IsAvailableOnlyForApprovedOrSentOrders(string status, bool expected)
    {
        order = Order(status);
        var vm = Create();
        await vm.LoadAsync();

        vm.SelectedOrder = vm.Orders[0];

        Assert.Equal(expected, vm.DispatchCommand.CanExecute(null));
    }

    /// <summary>
    /// Only the lines the user edited are sent with the confirmation; an untouched line means as ordered.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Confirmation_SendsOnlyTheEditedLines()
    {
        RecordSupplierConfirmationCommand? captured = null;
        _ = sender.Setup(s => s.Send(It.IsAny<RecordSupplierConfirmationCommand>(), Any))
            .Callback<IRequest<ProcurementActionResult>, CancellationToken>((command, _) => captured = (RecordSupplierConfirmationCommand)command)
            .ReturnsAsync(new ProcurementActionResult(ProcurementActionStatus.Queued));
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedOrder = vm.Orders[0];
        vm.ConfirmationNumber = " AB-7 ";
        vm.ConfirmationLines[0].ConfirmedQuantityText = "8";
        vm.ConfirmationLines[0].Reason = "Short stock";

        await vm.RecordConfirmationCommand.ExecuteAsync(null);

        Assert.NotNull(captured);
        Assert.Equal(" AB-7 ", captured.Request.ConfirmationNumber);
        var line = Assert.Single(captured.Request.Lines);
        Assert.Equal(LineOneId, line.PurchaseOrderLineId);
        Assert.Equal(8m, line.ConfirmedQuantity);
        Assert.Equal(100m, line.ConfirmedUnitPrice);
        Assert.Equal("Short stock", line.VarianceReason);
        Assert.Equal(loc["Procurement_Done_Queued"], vm.StatusMessage);
        Assert.Equal(string.Empty, vm.ConfirmationNumber);
    }

    /// <summary>
    /// A quantity that is not a number stops the confirmation before anything is sent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Confirmation_InvalidNumber_SendsNothing()
    {
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedOrder = vm.Orders[0];
        vm.ConfirmationNumber = "AB-7";
        vm.ConfirmationLines[1].ConfirmedQuantityText = "many";

        await vm.RecordConfirmationCommand.ExecuteAsync(null);

        Assert.Equal(loc["Procurement_InvalidNumber"], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<RecordSupplierConfirmationCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// A refusal of the local validation is shown with its localized message.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Confirmation_Rejected_ShowsTheLocalizedError()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<RecordSupplierConfirmationCommand>(), Any))
            .ReturnsAsync(new ProcurementActionResult(ProcurementActionStatus.Rejected, ProcurementService.NumberKey));
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedOrder = vm.Orders[0];

        await vm.RecordConfirmationCommand.ExecuteAsync(null);

        Assert.Equal(loc[ProcurementService.NumberKey], vm.StatusMessage);
    }

    /// <summary>
    /// The DDT is available while the dropshipment waits for it and the delivery once the DDT is issued; the delivery lists the open
    /// quantity of the supplier order.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Dropship_Selection_EnablesTheDeliveryAndListsTheOpenLines()
    {
        var vm = Create();
        await vm.LoadAsync();

        vm.SelectedDropship = vm.Dropships[0];

        Assert.Equal(loc["Procurement_Dropship_ddt_issued"], vm.Dropships[0].StatusText);
        Assert.False(vm.IssueDdtCommand.CanExecute(null));
        Assert.True(vm.RecordDeliveryCommand.CanExecute(null));
        Assert.Equal(2, vm.DeliveryLines.Count);
        Assert.Equal("8 BOX", vm.DeliveryLines[0].OpenText);
        _ = Assert.Single(vm.Movements);
    }

    /// <summary>
    /// The delivery sends the quantities typed per line, with the proof of delivery.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task RecordDelivery_SendsTheTypedQuantities()
    {
        RecordDropshipDeliveryCommand? captured = null;
        _ = sender.Setup(s => s.Send(It.IsAny<RecordDropshipDeliveryCommand>(), Any))
            .Callback<IRequest<ProcurementActionResult>, CancellationToken>((command, _) => captured = (RecordDropshipDeliveryCommand)command)
            .ReturnsAsync(new ProcurementActionResult(ProcurementActionStatus.Sent));
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedDropship = vm.Dropships[0];
        vm.DeliveryProof = "POD-1";
        vm.DeliveryLines[0].QuantityText = "6";

        await vm.RecordDeliveryCommand.ExecuteAsync(null);

        Assert.NotNull(captured);
        Assert.Equal(DropId, captured.Request.DropshipmentId);
        Assert.Equal("POD-1", captured.Request.DeliveryProofReference);
        var line = Assert.Single(captured.Request.Lines);
        Assert.Equal(LineOneId, line.PurchaseOrderLineId);
        Assert.Equal(6m, line.QuantityDelivered);
    }

    /// <summary>
    /// The pricing preview shows the cascade, the base-unit price and a note when it is the local estimate.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task PreviewPricing_ShowsTheAmountsAndTheEstimateNote()
    {
        PreviewPurchasePricingQuery? captured = null;
        _ = sender.Setup(s => s.Send(It.IsAny<PreviewPurchasePricingQuery>(), Any))
            .Callback<IRequest<ProcurementLoad<PricingPreview>>, CancellationToken>((query, _) => captured = (PreviewPurchasePricingQuery)query)
            .ReturnsAsync((IRequest<ProcurementLoad<PricingPreview>> query, CancellationToken _) =>
                new ProcurementLoad<PricingPreview>(PurchasePricingCalculator.Compute(((PreviewPurchasePricingQuery)query).Request), FromCache: true, Synced));
        var vm = Create();
        vm.PricingQuantityText = "10";
        vm.PricingPriceText = "100";
        vm.PricingCascadeText = "10 + 5";
        vm.PricingFactorText = "12";

        await vm.PreviewPricingCommand.ExecuteAsync(null);

        Assert.NotNull(captured);
        Assert.Equal([10m, 5m], captured.Request.DiscountCascade);
        Assert.Equal(12m, captured.Request.UomConversionFactor);
        Assert.True(vm.HasPricing);
        Assert.Equal(855m.ToString("N2", loc.CurrentCulture), vm.LineNetText);
        Assert.Equal(120m.ToString("0.####", loc.CurrentCulture), vm.QuantityBaseText);
        Assert.Equal(loc["Procurement_PricingEstimate"], vm.PricingNote);
    }

    /// <summary>
    /// A cascade step above 100 percent stops the preview before the server is asked.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task PreviewPricing_InvalidCascade_ShowsAMessage()
    {
        var vm = Create();
        vm.PricingPriceText = "100";
        vm.PricingCascadeText = "10 + 120";

        await vm.PreviewPricingCommand.ExecuteAsync(null);

        Assert.False(vm.HasPricing);
        Assert.Equal(loc["Procurement_InvalidNumber"], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<PreviewPurchasePricingQuery>(), Any), Times.Never);
    }

    /// <summary>
    /// The queue lists the writes waiting and the refused ones; a refused one can be dismissed.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Queue_ListsPendingAndRefusedWrites()
    {
        var entry = Guid.NewGuid();
        var when = new DateTime(2027, 4, 1, 7, 0, 0, DateTimeKind.Utc);
        _ = sender.Setup(s => s.Send(It.IsAny<GetProcurementQueueQuery>(), Any)).ReturnsAsync(new ProcurementQueue(
            [new ProcurementPendingAction(Guid.NewGuid(), ProcurementAction.DispatchOrder, "PO-1", when, null)],
            [new ProcurementPendingAction(entry, ProcurementAction.SendReminder, "PO-2", when, "Order not overdue")]));
        var vm = Create();

        await vm.LoadAsync();

        Assert.Equal(loc["Procurement_Action_DispatchOrder"], Assert.Single(vm.PendingQueue).ActionText);
        var conflict = Assert.Single(vm.ConflictQueue);
        Assert.Equal("Order not overdue", conflict.ErrorText);

        await vm.DismissConflictCommand.ExecuteAsync(conflict);

        sender.Verify(s => s.Send(It.Is<DismissProcurementConflictCommand>(c => c.EntryId == entry), Any), Times.Once);
    }

    private static PurchaseOrderView Order(string status) => new(
        OrderId,
        "PO-1",
        Guid.NewGuid(),
        "Rossi SA",
        status,
        new DateOnly(2027, 3, 1),
        new DateOnly(2027, 3, 20),
        new DateOnly(2027, 3, 25),
        "CHF",
        1043.10m,
        IsDropship: true,
        Guid.NewGuid(),
        ReminderCount: 1,
        LastReminderAt: null,
        status == "confirmed" ? "AB-7" : null,
        DispatchChannel: null,
        [
            new PurchaseOrderLineView(LineOneId, 1, "Screws", 10m, 2m, "BOX", "PZ", 12m, 120m, 24m, null, [10m, 5m], 14.5m, new DateOnly(2027, 3, 25), null, 100m, 855m),
            new PurchaseOrderLineView(LineTwoId, 2, "Nuts", 5m, 0m, "PZ", "PZ", 1m, 5m, 0m, null, [], 0m, null, null, 2m, 10m),
        ]);

    private static DropshipmentView Dropship(string status) => new(
        DropId,
        OrderId,
        Guid.NewGuid(),
        status,
        "Bianchi SA",
        "Via Roma 5, 6900 Lugano, CH",
        "DDT-1",
        Synced,
        SupplierShipmentReference: null,
        DeliveryProofReference: null,
        BillingAuthorized: false,
        BillingAuthorizationReference: null,
        DeliveredAt: null,
        [new DropshipMovementView(LineOneId, "in", 24m, "PZ", 7.125m, 171m, Synced)]);

    private ProcurementLoad<T> Load<T>(T data) => new(data, fromCache, Synced);

    private PurchaseFulfilmentViewModel Create() => new(sender.Object, loc, time);
}
