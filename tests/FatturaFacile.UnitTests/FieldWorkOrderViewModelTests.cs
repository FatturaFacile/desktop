// <copyright file="FieldWorkOrderViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.FieldService;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the work order view model: the covered and chargeable parts, the service report viewer, the pending operations of an
/// offline technician, the forms that queue operations, the PDF download and the conversion to invoice.
/// </summary>
public sealed class FieldWorkOrderViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly DateTime SyncedAt = new(2027, 3, 2, 7, 0, 0, DateTimeKind.Utc);

    private readonly Mock<ISender> sender = new();
    private readonly ILocalizationService loc = LocalizationService.Instance;

    private FieldServiceCached<FieldServiceWorkOrder>? detail = new(FieldServiceWorkOrderParser.ParseDetail(FieldServiceTestData.WorkOrder), SyncedAt);

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldWorkOrderViewModelTests"/> class.
    /// </summary>
    public FieldWorkOrderViewModelTests()
    {
        var list = FieldServiceWorkOrderParser.ParseList(FieldServiceTestData.WorkOrderList);
        _ = sender.Setup(s => s.Send(It.IsAny<GetFieldWorkOrdersQuery>(), Any)).ReturnsAsync(new FieldServiceCached<IReadOnlyList<FieldServiceWorkOrder>>(list, SyncedAt));
        _ = sender.Setup(s => s.Send(It.IsAny<GetFieldWorkOrderQuery>(), Any)).ReturnsAsync(() => detail);
        _ = sender.Setup(s => s.Send(It.IsAny<GetDispatcherGanttAndMapBoardQuery>(), Any)).ReturnsAsync(new FieldServiceCached<FieldServiceBoard>(FieldServiceBoardParser.Parse(FieldServiceTestData.Board), SyncedAt));
        _ = sender.Setup(s => s.Send(It.IsAny<GetTechnicianOfflineWorkQueueQuery>(), Any)).ReturnsAsync(new FieldServiceCached<FieldServiceTechnicianBundle>(FieldServiceBundleParser.Parse(FieldServiceTestData.Bundle), SyncedAt));
        _ = sender.Setup(s => s.Send(It.IsAny<SyncFieldServiceCommand>(), Any)).ReturnsAsync(new FieldServiceCallResult(FieldServiceCallStatus.Ok));
    }

    /// <summary>
    /// Loading shows the picker, the header and the parts with the covered and the chargeable ones told apart.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ShowsPartsWithCoveredAndChargeableApart()
    {
        var vm = await LoadedAsync();

        Assert.Equal(2, vm.WorkOrders.Count);
        Assert.Equal("WO-1", vm.SelectedWorkOrder?.Number);
        Assert.True(vm.HasOrder);
        Assert.Equal("WO-1", vm.NumberText);
        Assert.Equal(2, vm.Parts.Count);
        Assert.True(vm.Parts[0].IsCovered);
        Assert.Equal(loc["FieldService_Part_Covered"], vm.Parts[0].CoverageText);
        Assert.False(vm.Parts[1].IsCovered);
        Assert.Equal(loc["FieldService_Part_Chargeable"], vm.Parts[1].CoverageText);
        Assert.False(vm.HasPendingOperations);
    }

    /// <summary>
    /// The service report viewer shows the seal, the covered and chargeable totals and every billing line with its coverage.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ShowsTheServiceReportAndItsBilling()
    {
        var vm = await LoadedAsync();
        var culture = loc.CurrentCulture;

        Assert.True(vm.HasReport);
        Assert.False(vm.ReportIsPending);
        Assert.Equal("SR-2027-0001", vm.ReportNumberText);
        Assert.Equal("Filter replaced", vm.ReportSummaryText);
        Assert.False(vm.SealIsBroken);
        Assert.Equal(loc["FieldService_Report_SealOk"], vm.ReportSealText);
        Assert.Equal("CHF " + 12.5m.ToString("N2", culture), vm.BillableTotalText);
        Assert.Equal("CHF " + 250m.ToString("N2", culture), vm.CoveredTotalText);
        Assert.Equal(3, vm.BillingLines.Count);
        Assert.Equal([true, true, false], vm.BillingLines.Select(line => line.IsCovered));
        Assert.Equal(loc["FieldService_Kind_callout"], vm.BillingLines[1].KindText);
        Assert.Equal(2, vm.ReportChecklist.Count);
        Assert.True(vm.CanDownloadPdf);
        Assert.True(vm.CanInvoice);
        Assert.False(vm.CanSign);
    }

    /// <summary>
    /// A report whose seal no longer matches is flagged, and an invoiced report cannot be invoiced again.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_BrokenSealAndInvoicedReport_AreFlagged()
    {
        Change(order => order with { Report = order.Report! with { SignatureIntact = false, InvoiceId = "inv-1" } });

        var vm = await LoadedAsync();

        Assert.True(vm.SealIsBroken);
        Assert.Equal(loc["FieldService_Report_SealBroken"], vm.ReportSealText);
        Assert.False(vm.CanInvoice);
        Assert.Contains("inv-1", vm.InvoiceText, StringComparison.Ordinal);
    }

    /// <summary>
    /// What the technician did offline is shown as pending: the part waits to be priced, the report waits to be sealed.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_PendingOperations_AreMarkedPending()
    {
        FieldServicePart pendingPart = new(string.Empty, "p9", "Bearing", 1m, 0m, Covered: false, IsPending: true);
        Change(order => order with { Parts = [.. order.Parts, pendingPart], Report = order.Report! with { IsPending = true } });

        var vm = await LoadedAsync();

        Assert.True(vm.HasPendingOperations);
        Assert.Equal(3, vm.Parts.Count);
        Assert.True(vm.Parts[2].IsPending);
        Assert.Equal(loc["FieldService_Part_Pending"], vm.Parts[2].CoverageText);
        Assert.Equal(loc["FieldService_Report_Pending"], vm.ReportNumberText);
        Assert.False(vm.CanDownloadPdf);
        Assert.False(vm.CanInvoice);
        Assert.Equal("-", vm.BillableTotalText);
    }

    /// <summary>
    /// Recording a part saves it locally, shows the queued message, tells the hub and clears the form.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task RecordPart_OfflineQueuesTheOperationAndClearsTheForm()
    {
        OpenOrder();
        FieldServicePartInput? sent = null;
        _ = sender.Setup(s => s.Send(It.IsAny<RecordVanStockPartConsumptionCommand>(), Any))
            .Callback((IRequest<FieldServiceQueueResult> command, CancellationToken _) => sent = ((RecordVanStockPartConsumptionCommand)command).Part)
            .ReturnsAsync(new FieldServiceQueueResult(ErrorKey: null, Sent: false, Conflict: false, "uuid"));
        var vm = await LoadedAsync();
        var raised = 0;
        vm.OutboxChanged += (_, _) => raised++;
        vm.PartProductId = FieldServiceTestData.ProductId;
        vm.PartDescription = " Filter ";
        vm.PartQuantity = "2.5";
        vm.PartUnitPrice = "12.5";
        vm.PartLot = " L-1 ";

        await vm.RecordPartAsync();

        Assert.NotNull(sent);
        Assert.Equal(FieldServiceTestData.OrderGuid, sent.WorkOrderId);
        Assert.Equal(FieldServiceTestData.ProductGuid, sent.ProductId);
        Assert.Equal("Filter", sent.Description);
        Assert.Equal(2.5m, sent.Quantity);
        Assert.Equal(12.5m, sent.UnitPrice);
        Assert.Equal("L-1", sent.LotNumber);
        Assert.Equal(loc["FieldService_QueueQueued"], vm.StatusMessage);
        Assert.True(vm.IsOffline);
        Assert.Equal(1, raised);
        Assert.Empty(vm.PartProductId);
        Assert.Equal("1", vm.PartQuantity);
    }

    /// <summary>
    /// A part the server applied at once is pulled back, so it shows priced and with its coverage.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task RecordPart_SentAtOnce_PullsTheWorkOrder()
    {
        OpenOrder();
        _ = sender.Setup(s => s.Send(It.IsAny<RecordVanStockPartConsumptionCommand>(), Any)).ReturnsAsync(new FieldServiceQueueResult(ErrorKey: null, Sent: true, Conflict: false, "uuid"));
        var vm = await LoadedAsync();
        vm.PartProductId = FieldServiceTestData.ProductId;

        await vm.RecordPartAsync();

        sender.Verify(s => s.Send(It.Is<SyncFieldServiceCommand>(c => c.Scope == FieldServiceSyncScope.WorkOrder && c.Key == FieldServiceTestData.OrderId), Any), Times.Once);
        Assert.Equal(loc["FieldService_QueueSent"], vm.StatusMessage);
        Assert.False(vm.IsOffline);
    }

    /// <summary>
    /// A part the service refuses keeps the form and shows the reason; a rejection by the server shows the conflict.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task RecordPart_RefusedOrRejected_ShowsTheReasonAndKeepsTheForm()
    {
        OpenOrder();
        var vm = await LoadedAsync();
        vm.PartProductId = FieldServiceTestData.ProductId;
        _ = sender.Setup(s => s.Send(It.IsAny<RecordVanStockPartConsumptionCommand>(), Any)).ReturnsAsync(new FieldServiceQueueResult(FieldServiceValidation.QuantityKey, Sent: false, Conflict: false, string.Empty));
        var raised = 0;
        vm.OutboxChanged += (_, _) => raised++;

        await vm.RecordPartAsync();

        Assert.Equal(loc[FieldServiceValidation.QuantityKey], vm.StatusMessage);
        Assert.Equal(FieldServiceTestData.ProductId, vm.PartProductId);
        Assert.Equal(0, raised);

        _ = sender.Setup(s => s.Send(It.IsAny<RecordVanStockPartConsumptionCommand>(), Any)).ReturnsAsync(new FieldServiceQueueResult(ErrorKey: null, Sent: false, Conflict: true, "uuid"));
        await vm.RecordPartAsync();

        Assert.Equal(loc["FieldService_QueueConflict"], vm.StatusMessage);
    }

    /// <summary>
    /// A part without a product identifier or with a bad number is refused before anything is queued.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task RecordPart_WithBadInput_IsRefusedLocally()
    {
        OpenOrder();
        var vm = await LoadedAsync();

        await vm.RecordPartAsync();
        Assert.Equal(loc["FieldService_Err_Product"], vm.StatusMessage);

        vm.PartProductId = FieldServiceTestData.ProductId;
        vm.PartQuantity = "abc";
        await vm.RecordPartAsync();
        Assert.Equal(loc["FieldService_Err_Number"], vm.StatusMessage);

        sender.Verify(s => s.Send(It.IsAny<RecordVanStockPartConsumptionCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// The service report form sends the checklist of the work type, the hours with either decimal separator, the meter readings
    /// and the language of the user.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SignAndClose_SendsTheReportOfTheForm()
    {
        OpenOrder();
        FieldServiceReportInput? sent = null;
        _ = sender.Setup(s => s.Send(It.IsAny<SignAndCloseServiceReportCommand>(), Any))
            .Callback((IRequest<FieldServiceQueueResult> command, CancellationToken _) => sent = ((SignAndCloseServiceReportCommand)command).Report)
            .ReturnsAsync(new FieldServiceQueueResult(ErrorKey: null, Sent: false, Conflict: false, "uuid"));
        var vm = await LoadedAsync();
        Assert.True(vm.CanSign);
        Assert.Equal(4, vm.ChecklistEdit.Count);
        vm.ChecklistEdit[1].Result = "fail";
        vm.ChecklistEdit[1].Notes = " Leak ";
        vm.SignSummary = "  Filter replaced ";
        vm.SignLaborHours = "2.5";
        vm.SignTravelHours = "0.5";
        vm.SignTravelKm = "30";
        vm.SignApplyCallout = true;
        vm.SignMeterHours = "1520.5";
        vm.SignMeterCycles = "4000";
        vm.SignSignerName = " Mario Bianchi ";
        vm.SignSignerRole = string.Empty;
        vm.SignInkSketch = Sketch(strokes: 2);

        await vm.SignAndCloseAsync();

        Assert.NotNull(sent);
        Assert.Equal("Filter replaced", sent.Summary);
        Assert.Equal(2.5m, sent.LaborHours);
        Assert.Equal(0.5m, sent.TravelHours);
        Assert.Equal(30m, sent.TravelKm);
        Assert.True(sent.ApplyCalloutFee);
        Assert.Equal(1520.5m, sent.MeterHours);
        Assert.Equal(4000, sent.MeterCycles);
        Assert.Equal("Mario Bianchi", sent.SignerName);
        Assert.Null(sent.SignerRole);
        Assert.True(FieldServiceValidation.IsSafeSvg(sent.SignatureSvg));
        Assert.Equal(4, sent.Checklist.Count);
        Assert.Equal("fail", sent.Checklist[1].Result);
        Assert.Equal("Leak", sent.Checklist[1].Notes);
        Assert.Equal(["visual_inspection", "lubrication", "safety_devices", "functional_test"], sent.Checklist.Select(line => line.Item), StringComparer.Ordinal);
        Assert.Contains(sent.Locale, new[] { "it-IT", "it-CH", "de-CH", "fr-CH", "en" }, StringComparer.Ordinal);
        Assert.Equal(loc["FieldService_QueueQueued"], vm.StatusMessage);
        Assert.Empty(vm.SignSummary);
    }

    /// <summary>
    /// The language of the PDF follows the language of the user, and the user can change it among the five locales.
    /// </summary>
    [Fact]
    public void Locales_OfferTheFiveReportLanguages()
    {
        FieldWorkOrderViewModel vm = new(sender.Object, loc);

        Assert.Equal(["it-IT", "it-CH", "de-CH", "fr-CH", "en"], vm.Locales.Select(choice => choice.Value), StringComparer.Ordinal);
        Assert.NotNull(vm.SelectedLocale);
    }

    /// <summary>
    /// A report form with a bad number is refused before anything is queued.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SignAndClose_WithABadNumber_IsRefusedLocally()
    {
        OpenOrder();
        var vm = await LoadedAsync();
        vm.SignLaborHours = "many";

        await vm.SignAndCloseAsync();

        Assert.Equal(loc["FieldService_Err_Number"], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<SignAndCloseServiceReportCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// The ink drawn on the pad becomes a safe vector SVG with its SHA-256 shown next to the pad.
    /// </summary>
    [Fact]
    public void InkSketch_BecomesASafeSvgWithItsHash()
    {
        FieldWorkOrderViewModel vm = new(sender.Object, loc);
        Assert.Equal(loc["FieldService_Signature_Empty"], vm.SignSignatureHashText);

        vm.SignInkSketch = Sketch(strokes: 2);

        Assert.StartsWith("<svg", vm.SignSignatureSvg, StringComparison.Ordinal);
        Assert.Contains("<path", vm.SignSignatureSvg, StringComparison.Ordinal);
        Assert.True(FieldServiceValidation.IsSafeSvg(vm.SignSignatureSvg));
        Assert.Contains(FieldServiceSignatureSvg.Sha256Hex(vm.SignSignatureSvg), vm.SignSignatureHashText, StringComparison.Ordinal);
        Assert.Equal(string.Format(loc.CurrentCulture, loc["FieldService_Signature_Strokes"], "2"), vm.SignStrokesText);
    }

    /// <summary>
    /// Clearing the pad empties the SVG and the hash and asks the pad to clear itself.
    /// </summary>
    [Fact]
    public void ClearSignature_EmptiesTheSignatureAndRequestsAClear()
    {
        FieldWorkOrderViewModel vm = new(sender.Object, loc) { SignInkSketch = Sketch(strokes: 1) };
        var requests = vm.SignInkClearRequest;

        vm.ClearSignature();

        Assert.Empty(vm.SignSignatureSvg);
        Assert.Null(vm.SignInkSketch);
        Assert.Equal(loc["FieldService_Signature_Empty"], vm.SignSignatureHashText);
        Assert.Equal(requests + 1, vm.SignInkClearRequest);
    }

    /// <summary>
    /// Undo takes back the last stroke only, and does nothing on an empty pad.
    /// </summary>
    [Fact]
    public void UndoSignature_TakesBackTheLastStroke()
    {
        FieldWorkOrderViewModel vm = new(sender.Object, loc);
        vm.UndoSignature();
        Assert.Equal(0, vm.SignInkUndoRequest);

        vm.SignInkSketch = Sketch(strokes: 2);
        var before = vm.SignSignatureSvg;

        vm.UndoSignature();

        Assert.Equal(1, vm.SignInkUndoRequest);
        _ = Assert.Single(vm.SignInkSketch.Strokes);
        Assert.NotEqual(before, vm.SignSignatureSvg, StringComparer.Ordinal);
        Assert.True(FieldServiceValidation.IsSafeSvg(vm.SignSignatureSvg));

        vm.UndoSignature();

        Assert.Empty(vm.SignSignatureSvg);
    }

    /// <summary>
    /// Signing without any ink is refused before anything is queued, like the server would refuse an empty signature.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SignAndClose_WithoutInk_IsRefusedLocally()
    {
        OpenOrder();
        var vm = await LoadedAsync();
        vm.SignSummary = "Done";
        vm.SignSignerName = "Mario Bianchi";

        await vm.SignAndCloseAsync();

        Assert.Equal(loc["FieldService_Err_Signature"], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<SignAndCloseServiceReportCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// The technician buttons follow the status of the work order and queue the matching status.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task EnRoute_QueuesTheStatusOfADispatchedOrder()
    {
        OpenOrder();
        AdvanceFieldWorkOrderStatusCommand? sent = null;
        _ = sender.Setup(s => s.Send(It.IsAny<AdvanceFieldWorkOrderStatusCommand>(), Any))
            .Callback((IRequest<FieldServiceQueueResult> command, CancellationToken _) => sent = (AdvanceFieldWorkOrderStatusCommand)command)
            .ReturnsAsync(new FieldServiceQueueResult(ErrorKey: null, Sent: false, Conflict: false, "uuid"));
        var vm = await LoadedAsync();
        Assert.True(vm.CanAdvance);

        await vm.EnRouteAsync();

        Assert.NotNull(sent);
        Assert.Equal(FieldServiceTestData.OrderGuid, sent.WorkOrderId);
        Assert.Equal("en_route", sent.Status);

        Change(order => order with { Status = "on_site" });
        await vm.SyncAsync();

        Assert.False(vm.CanAdvance);
    }

    /// <summary>
    /// The PDF download sends the report and shows where the file was saved.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task DownloadPdf_ShowsTheSavedPath()
    {
        DownloadServiceReportPdfCommand? sent = null;
        _ = sender.Setup(s => s.Send(It.IsAny<DownloadServiceReportPdfCommand>(), Any))
            .Callback((IRequest<FieldServicePdfResult> command, CancellationToken _) => sent = (DownloadServiceReportPdfCommand)command)
            .ReturnsAsync(new FieldServicePdfResult(FieldServiceCallStatus.Ok, "C:\\Reports\\sr.pdf", Detail: null));
        var vm = await LoadedAsync();

        await vm.DownloadPdfAsync();

        Assert.Equal(FieldServiceTestData.ReportId, sent?.ReportId);
        Assert.Contains("C:\\Reports\\sr.pdf", vm.StatusMessage, StringComparison.Ordinal);
    }

    /// <summary>
    /// A PDF the server cannot give is shown as the server reason, and an offline download is flagged offline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task DownloadPdf_Offline_IsFlagged()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<DownloadServiceReportPdfCommand>(), Any)).ReturnsAsync(new FieldServicePdfResult(FieldServiceCallStatus.Offline, Path: null, Detail: null));
        var vm = await LoadedAsync();

        await vm.DownloadPdfAsync();

        Assert.True(vm.IsOffline);
        Assert.Equal(loc["FieldService_OfflineUsingCache"], vm.StatusMessage);
    }

    /// <summary>
    /// Converting to invoice sends the report and the tax rate, then refreshes the work order and shows the invoice.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ConvertToInvoice_CreatesTheDraftAndRefreshes()
    {
        ConvertServiceReportToInvoiceCommand? sent = null;
        _ = sender.Setup(s => s.Send(It.IsAny<ConvertServiceReportToInvoiceCommand>(), Any))
            .Callback((IRequest<FieldServiceCallResult> command, CancellationToken _) => sent = (ConvertServiceReportToInvoiceCommand)command)
            .ReturnsAsync(new FieldServiceCallResult(FieldServiceCallStatus.Ok, Id: "inv-9"));
        var vm = await LoadedAsync();
        vm.TaxRateText = "8.1";

        await vm.ConvertToInvoiceAsync();

        Assert.NotNull(sent);
        Assert.Equal(FieldServiceTestData.ReportId, sent.ReportId);
        Assert.Equal(8.1m, sent.TaxRate);
        Assert.Contains("inv-9", vm.StatusMessage, StringComparison.Ordinal);
        sender.Verify(s => s.Send(It.Is<SyncFieldServiceCommand>(c => c.Scope == FieldServiceSyncScope.WorkOrder), Any), Times.AtLeastOnce);
    }

    /// <summary>
    /// A bad tax rate is refused locally, and a rejection by the server is shown with its reason.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ConvertToInvoice_BadRateOrRejected_IsShown()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<ConvertServiceReportToInvoiceCommand>(), Any)).ReturnsAsync(new FieldServiceCallResult(FieldServiceCallStatus.Rejected, "Nothing billable"));
        var vm = await LoadedAsync();
        vm.TaxRateText = "x";

        await vm.ConvertToInvoiceAsync();

        Assert.Equal(loc["FieldService_Err_TaxRate"], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<ConvertServiceReportToInvoiceCommand>(), Any), Times.Never);

        vm.TaxRateText = "22";
        await vm.ConvertToInvoiceAsync();

        Assert.Contains("Nothing billable", vm.StatusMessage, StringComparison.Ordinal);
    }

    /// <summary>
    /// Selecting a technician shows his offline work bundle with the customer and the equipment of each work order.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SelectingATechnician_ShowsTheOfflineBundle()
    {
        var vm = await LoadedAsync();
        Assert.Equal(2, vm.Technicians.Count);
        Assert.Empty(vm.Bundle);

        vm.SelectedTechnician = vm.Technicians[0];

        var item = Assert.Single(vm.Bundle);
        Assert.Equal("WO-1", item.Number);
        Assert.Equal("ACME SA", item.CustomerText);
        Assert.Equal("SN-100", item.EquipmentText);
        sender.Verify(s => s.Send(It.Is<GetTechnicianOfflineWorkQueueQuery>(q => q.TechnicianId == FieldServiceTestData.TechnicianId), Any), Times.AtLeastOnce);
    }

    /// <summary>
    /// With nothing cached and no server the picker stays empty and offline is shown.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_WithNothingCachedAndNoServer_IsEmptyAndOffline()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetFieldWorkOrdersQuery>(), Any)).ReturnsAsync((FieldServiceCached<IReadOnlyList<FieldServiceWorkOrder>>?)null);
        _ = sender.Setup(s => s.Send(It.IsAny<SyncFieldServiceCommand>(), Any)).ReturnsAsync(new FieldServiceCallResult(FieldServiceCallStatus.Offline));
        detail = null;

        var vm = await LoadedAsync();

        Assert.Empty(vm.WorkOrders);
        Assert.False(vm.HasOrder);
        Assert.True(vm.IsOffline);
        Assert.Equal(loc["FieldService_NeverSynced"], vm.LastSyncedText);
    }

    /// <summary>
    /// Makes the sample work order a dispatched visit without report, so the technician forms are open.
    /// </summary>
    /// <param name="strokes">The number of strokes of the sketch.</param>
    private static FieldServiceInkSketch Sketch(int strokes) => new(
        [.. Enumerable.Range(0, strokes).Select(index => (IReadOnlyList<FieldServiceInkPoint>)[new FieldServiceInkPoint(10 + (index * 20), 10), new FieldServiceInkPoint(40 + (index * 20), 60), new FieldServiceInkPoint(70 + (index * 20), 20)])],
        300,
        120);

    private void OpenOrder() => Change(order => order with { Status = "dispatched", Report = null, Parts = [] });

    private void Change(Func<FieldServiceWorkOrder, FieldServiceWorkOrder> edit)
    {
        var current = detail ?? throw new InvalidOperationException("The sample work order is not cached.");
        detail = current with { Value = edit(current.Value) };
    }

    private async Task<FieldWorkOrderViewModel> LoadedAsync()
    {
        FieldWorkOrderViewModel vm = new(sender.Object, loc);
        await vm.LoadAsync();

        return vm;
    }
}
