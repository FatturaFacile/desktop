// <copyright file="FieldServiceHandlersTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http;
using System.Text.Json;
using FatturaFacile.Application.Features.FieldService;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Enums;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the Field Service command and query handlers: dispatch with its blockers, the preventive run, the PDF download, the
/// conversion to invoice, the contract save and the cached reads.
/// </summary>
public sealed class FieldServiceHandlersTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();

    private readonly ManualTimeProvider clock = new();
    private readonly Mock<IFieldServiceApiClient> api = new();
    private readonly Mock<IFieldServiceStore> store = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldServiceHandlersTests"/> class.
    /// </summary>
    public FieldServiceHandlersTests()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: true);
    }

    /// <summary>
    /// A dispatch posts the technician and the window to the work order route.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Dispatch_PostsTheWindowToTheWorkOrderRoute()
    {
        string? sent = null;
        _ = api.Setup(a => a.SendAsync(HttpMethod.Post, "work-orders/" + FieldServiceTestData.OrderId + "/dispatch", It.IsAny<string?>(), Any))
            .Callback((HttpMethod _, string _, string? body, CancellationToken _) => sent = body)
            .ReturnsAsync(new FieldServiceApiReply(HttpStatusCode.OK, FieldServiceTestData.Envelope("{}")));
        var start = new DateTimeOffset(2027, 3, 2, 13, 0, 0, TimeSpan.Zero);

        var result = await new DispatchTechnicianWorkOrderCommandHandler(api.Object).Handle(Dispatch(start), CancellationToken.None);

        Assert.Equal(FieldServiceCallStatus.Ok, result.Status);
        Assert.Empty(result.Warnings);
        using var body = JsonDocument.Parse(sent!);
        Assert.Equal(FieldServiceTestData.TechnicianId, body.RootElement.GetProperty("technician_id").GetString());
    }

    /// <summary>
    /// A blocked dispatch returns every blocker together, so the dispatcher sees them all at once.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Dispatch_Blocked_ReturnsAllTheBlockers()
    {
        _ = api.Setup(a => a.SendAsync(HttpMethod.Post, It.IsAny<string>(), It.IsAny<string?>(), Any))
            .ReturnsAsync(new FieldServiceApiReply(HttpStatusCode.UnprocessableEntity, FieldServiceTestData.DispatchBlocked));

        var result = await new DispatchTechnicianWorkOrderCommandHandler(api.Object).Handle(Dispatch(DateTimeOffset.UtcNow), CancellationToken.None);

        Assert.Equal(FieldServiceCallStatus.Rejected, result.Status);
        Assert.Equal([FieldServiceWarningKind.Schedule, FieldServiceWarningKind.Skills, FieldServiceWarningKind.VanStock], result.Warnings.Select(w => w.Kind));
        Assert.Contains("Missing skills: electrical", result.Warnings[1].Detail, StringComparison.Ordinal);
    }

    /// <summary>
    /// Dispatching offline or without a session is reported, never thrown, and nothing is queued.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Dispatch_OfflineOrSignedOut_IsReported()
    {
        _ = api.Setup(a => a.SendAsync(HttpMethod.Post, It.IsAny<string>(), It.IsAny<string?>(), Any)).ThrowsAsync(new HttpRequestException("down"));
        var handler = new DispatchTechnicianWorkOrderCommandHandler(api.Object);

        Assert.Equal(FieldServiceCallStatus.Offline, (await handler.Handle(Dispatch(DateTimeOffset.UtcNow), CancellationToken.None)).Status);

        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);
        Assert.Equal(FieldServiceCallStatus.Unauthorized, (await handler.Handle(Dispatch(DateTimeOffset.UtcNow), CancellationToken.None)).Status);
    }

    /// <summary>
    /// The dispatch command is validated: identifiers must be GUIDs and the window must end after it starts.
    /// </summary>
    [Fact]
    public void DispatchValidator_ChecksIdentifiersAndWindow()
    {
        var validator = new DispatchTechnicianWorkOrderCommandValidator();
        var start = DateTimeOffset.UtcNow;

        validator.TestValidate(Dispatch(start)).ShouldNotHaveAnyValidationErrors();
        _ = validator.TestValidate(Dispatch(start) with { WorkOrderId = "x" }).ShouldHaveValidationErrorFor(x => x.WorkOrderId).WithErrorMessage("FieldService_Err_WorkOrder");
        _ = validator.TestValidate(Dispatch(start) with { TechnicianId = Guid.Empty.ToString() }).ShouldHaveValidationErrorFor(x => x.TechnicianId);
        _ = validator.TestValidate(Dispatch(start) with { End = start }).ShouldHaveValidationErrorFor(x => x.End).WithErrorMessage("FieldService_Err_Window");
    }

    /// <summary>
    /// The preventive run reports what the engine evaluated, created and skipped.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Preventive_ReportsTheEngineCounts()
    {
        _ = api.Setup(a => a.SendAsync(HttpMethod.Post, "preventive-maintenance/run", null, Any))
            .ReturnsAsync(new FieldServiceApiReply(HttpStatusCode.OK, FieldServiceTestData.PreventiveRun));

        var result = await new GeneratePreventiveMaintenanceWorkOrdersCommandHandler(api.Object, clock).Handle(new GeneratePreventiveMaintenanceWorkOrdersCommand(), CancellationToken.None);

        Assert.Equal(new FieldServicePreventiveResult(FieldServiceCallStatus.Ok, 7, 2, 3, Detail: null), result);
    }

    /// <summary>
    /// A refused preventive run keeps the server message.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Preventive_Forbidden_IsReported()
    {
        _ = api.Setup(a => a.SendAsync(HttpMethod.Post, "preventive-maintenance/run", null, Any))
            .ReturnsAsync(new FieldServiceApiReply(HttpStatusCode.Forbidden, "{\"message\":\"Module not enabled\"}"));

        var result = await new GeneratePreventiveMaintenanceWorkOrdersCommandHandler(api.Object, clock).Handle(new GeneratePreventiveMaintenanceWorkOrdersCommand(), CancellationToken.None);

        Assert.Equal(FieldServiceCallStatus.Forbidden, result.Status);
        Assert.Equal("Module not enabled", result.Detail);
        Assert.Equal(0, result.Created);
    }

    /// <summary>
    /// The conversion to invoice posts the tax rate to the report route and returns the invoice identifier.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ConvertToInvoice_PostsTheTaxRateAndReturnsTheInvoice()
    {
        string? sent = null;
        _ = api.Setup(a => a.SendAsync(HttpMethod.Post, "service-reports/" + FieldServiceTestData.ReportId + "/invoice", It.IsAny<string?>(), Any))
            .Callback((HttpMethod _, string _, string? body, CancellationToken _) => sent = body)
            .ReturnsAsync(new FieldServiceApiReply(HttpStatusCode.Created, FieldServiceTestData.Envelope("{\"invoice_id\":\"inv-1\"}")));

        var result = await new ConvertServiceReportToInvoiceCommandHandler(api.Object, clock)
            .Handle(new ConvertServiceReportToInvoiceCommand(FieldServiceTestData.ReportId, 8.1m, DueDate: null), CancellationToken.None);

        Assert.Equal(FieldServiceCallStatus.Ok, result.Status);
        Assert.Equal("inv-1", result.Id);
        using var body = JsonDocument.Parse(sent!);
        Assert.Equal("8.1", body.RootElement.GetProperty("tax_rate").GetString());
    }

    /// <summary>
    /// A report that is already invoiced or has nothing billable is refused with the server reason.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ConvertToInvoice_NothingBillable_IsRejectedWithTheReason()
    {
        _ = api.Setup(a => a.SendAsync(HttpMethod.Post, It.IsAny<string>(), It.IsAny<string?>(), Any))
            .ReturnsAsync(new FieldServiceApiReply(HttpStatusCode.UnprocessableEntity, "{\"errors\":{\"report\":[\"Nothing billable\"]}}"));

        var result = await new ConvertServiceReportToInvoiceCommandHandler(api.Object, clock)
            .Handle(new ConvertServiceReportToInvoiceCommand(FieldServiceTestData.ReportId, 22m, DueDate: null), CancellationToken.None);

        Assert.Equal(FieldServiceCallStatus.Rejected, result.Status);
        Assert.Contains("Nothing billable", result.Detail, StringComparison.Ordinal);
    }

    /// <summary>
    /// The invoice command is validated: a GUID report and a tax rate between 0 and 100.
    /// </summary>
    [Fact]
    public void InvoiceValidator_ChecksReportAndTaxRate()
    {
        var validator = new ConvertServiceReportToInvoiceCommandValidator();

        validator.TestValidate(new ConvertServiceReportToInvoiceCommand(FieldServiceTestData.ReportId, 22m, DueDate: null)).ShouldNotHaveAnyValidationErrors();
        _ = validator.TestValidate(new ConvertServiceReportToInvoiceCommand(FieldServiceTestData.ReportId, 101m, DueDate: null)).ShouldHaveValidationErrorFor(x => x.TaxRate);
        _ = validator.TestValidate(new ConvertServiceReportToInvoiceCommand("nope", 22m, DueDate: null)).ShouldHaveValidationErrorFor(x => x.ReportId);
    }

    /// <summary>
    /// A new contract is posted, an edited one is patched, and the identifier of the record comes back.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveContract_PostsNewAndPatchesExisting()
    {
        _ = api.Setup(a => a.SendAsync(HttpMethod.Post, "maintenance-contracts", It.IsAny<string?>(), Any))
            .ReturnsAsync(new FieldServiceApiReply(HttpStatusCode.Created, FieldServiceTestData.Envelope("{\"id\":\"new-id\"}")));
        _ = api.Setup(a => a.SendAsync(HttpMethod.Patch, "maintenance-contracts/" + FieldServiceTestData.ContractId, It.IsAny<string?>(), Any))
            .ReturnsAsync(new FieldServiceApiReply(HttpStatusCode.OK, FieldServiceTestData.Envelope("{\"id\":\"" + FieldServiceTestData.ContractId + "\"}")));
        var handler = new SaveMaintenanceContractCommandHandler(api.Object, clock);

        var created = await handler.Handle(new SaveMaintenanceContractCommand(Contract(id: null)), CancellationToken.None);
        var edited = await handler.Handle(new SaveMaintenanceContractCommand(Contract(FieldServiceTestData.ContractId)), CancellationToken.None);

        Assert.Equal("new-id", created.Id);
        Assert.Equal(FieldServiceTestData.ContractId, edited.Id);
        Assert.Equal(FieldServiceCallStatus.Ok, edited.Status);
    }

    /// <summary>
    /// A contract the server refuses keeps the field errors.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveContract_Refused_KeepsTheFieldErrors()
    {
        _ = api.Setup(a => a.SendAsync(HttpMethod.Post, "maintenance-contracts", It.IsAny<string?>(), Any))
            .ReturnsAsync(new FieldServiceApiReply(HttpStatusCode.UnprocessableEntity, "{\"errors\":{\"contract_number\":[\"Already taken\"]}}"));

        var result = await new SaveMaintenanceContractCommandHandler(api.Object, clock).Handle(new SaveMaintenanceContractCommand(Contract(id: null)), CancellationToken.None);

        Assert.Equal(FieldServiceCallStatus.Rejected, result.Status);
        Assert.Equal("contract_number: Already taken", result.Detail);
    }

    /// <summary>
    /// The contract command is validated like the server request, with localization keys as messages.
    /// </summary>
    [Fact]
    public void ContractValidator_MirrorsTheServerRules()
    {
        var validator = new SaveMaintenanceContractCommandValidator();
        var valid = Contract(id: null);

        validator.TestValidate(new SaveMaintenanceContractCommand(valid)).ShouldNotHaveAnyValidationErrors();
        _ = validator.TestValidate(new SaveMaintenanceContractCommand(valid with { CustomerId = "x" })).ShouldHaveValidationErrorFor(x => x.Contract.CustomerId);
        _ = validator.TestValidate(new SaveMaintenanceContractCommand(valid with { Number = " " })).ShouldHaveValidationErrorFor(x => x.Contract.Number);
        _ = validator.TestValidate(new SaveMaintenanceContractCommand(valid with { Currency = "EURO" })).ShouldHaveValidationErrorFor(x => x.Contract.Currency);
        _ = validator.TestValidate(new SaveMaintenanceContractCommand(valid with { EndDate = new DateOnly(2026, 12, 31) })).ShouldHaveValidationErrorFor(x => x.Contract);
        _ = validator.TestValidate(new SaveMaintenanceContractCommand(valid with { CalloutFee = -1m })).ShouldHaveValidationErrorFor(x => x.Contract);
        _ = validator.TestValidate(new SaveMaintenanceContractCommand(valid with { Frequency = new PreventiveFrequency(0, "month", null, null, null, null) })).ShouldHaveValidationErrorFor(x => x.Contract.Frequency);
        _ = validator.TestValidate(new SaveMaintenanceContractCommand(valid with { Frequency = new PreventiveFrequency(6, "decade", null, null, null, null) })).ShouldHaveValidationErrorFor(x => x.Contract.Frequency);
        validator.TestValidate(new SaveMaintenanceContractCommand(valid with { Frequency = new PreventiveFrequency(null, null, 500m, 1000, 10m, 14) })).ShouldNotHaveValidationErrorFor(x => x.Contract.Frequency);
        _ = validator.TestValidate(new SaveMaintenanceContractCommand(valid with { Frequency = new PreventiveFrequency(null, null, null, null, 80m, null) })).ShouldHaveValidationErrorFor(x => x.Contract.Frequency);
    }

    /// <summary>
    /// The PDF is downloaded and handed to the saver, which returns the path shown to the user.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task DownloadPdf_SavesTheFile()
    {
        var pdf = new FieldServicePdf("service-report-" + FieldServiceTestData.ReportId + ".pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 });
        _ = api.Setup(a => a.DownloadReportPdfAsync(FieldServiceTestData.ReportId, Any)).ReturnsAsync(pdf);
        Mock<IServiceReportPdfSaver> saver = new();
        _ = saver.Setup(s => s.SaveAsync(pdf, Any)).ReturnsAsync("C:\\Reports\\sr.pdf");

        var result = await new DownloadServiceReportPdfCommandHandler(api.Object, saver.Object).Handle(new DownloadServiceReportPdfCommand(FieldServiceTestData.ReportId), CancellationToken.None);

        Assert.Equal(new FieldServicePdfResult(FieldServiceCallStatus.Ok, "C:\\Reports\\sr.pdf", Detail: null), result);
    }

    /// <summary>
    /// A report without a PDF on the server is a rejection and nothing is saved.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task DownloadPdf_NotFound_SavesNothing()
    {
        _ = api.Setup(a => a.DownloadReportPdfAsync(It.IsAny<string>(), Any)).ThrowsAsync(new HttpRequestException("missing", inner: null, HttpStatusCode.NotFound));
        Mock<IServiceReportPdfSaver> saver = new();

        var result = await new DownloadServiceReportPdfCommandHandler(api.Object, saver.Object).Handle(new DownloadServiceReportPdfCommand(FieldServiceTestData.ReportId), CancellationToken.None);

        Assert.Equal(FieldServiceCallStatus.Rejected, result.Status);
        Assert.Null(result.Path);
        saver.Verify(s => s.SaveAsync(It.IsAny<FieldServicePdf>(), Any), Times.Never);
    }

    /// <summary>
    /// The read handlers answer from the cache, with the instant it was pulled, and never call the server.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Queries_ReadTheCacheOnly()
    {
        var at = new DateTime(2027, 3, 2, 7, 0, 0, DateTimeKind.Utc);
        Cache(FieldServiceCacheKinds.Board, FieldServiceCacheKinds.All, FieldServiceTestData.Board, at);
        Cache(FieldServiceCacheKinds.Contracts, FieldServiceCacheKinds.All, FieldServiceTestData.Contracts, at);
        Cache(FieldServiceCacheKinds.History, FieldServiceTestData.EquipmentId, FieldServiceTestData.History, at);
        Cache(FieldServiceCacheKinds.Bundle, FieldServiceTestData.TechnicianId, FieldServiceTestData.Bundle, at);

        var board = await new GetDispatcherGanttAndMapBoardQueryHandler(store.Object).Handle(new GetDispatcherGanttAndMapBoardQuery(), CancellationToken.None);
        var contracts = await new GetMaintenanceContractsQueryHandler(store.Object).Handle(new GetMaintenanceContractsQuery(), CancellationToken.None);
        var history = await new GetCustomerEquipmentServiceHistoryQueryHandler(store.Object).Handle(new GetCustomerEquipmentServiceHistoryQuery(FieldServiceTestData.EquipmentId), CancellationToken.None);
        var bundle = await new GetTechnicianOfflineWorkQueueQueryHandler(store.Object).Handle(new GetTechnicianOfflineWorkQueueQuery(FieldServiceTestData.TechnicianId), CancellationToken.None);
        var missing = await new GetFieldServiceEquipmentQueryHandler(store.Object).Handle(new GetFieldServiceEquipmentQuery(), CancellationToken.None);

        Assert.Equal(2, board?.Value.Lanes.Count);
        Assert.Equal(at, board?.SyncedAtUtc);
        Assert.Equal(2, contracts?.Value.Count);
        Assert.Equal("SN-100", history?.Value.SerialNumber);
        Assert.Equal("Anna Rossi", bundle?.Value.Name);
        Assert.Null(missing);
        api.VerifyNoOtherCalls();
    }

    /// <summary>
    /// The work order read lays the operations still in the outbox over the cached work order.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task WorkOrderQuery_ShowsPendingOperationsOverTheCache()
    {
        var at = new DateTime(2027, 3, 2, 7, 0, 0, DateTimeKind.Utc);
        var order = FieldServiceTestData.WorkOrder;
        Cache(FieldServiceCacheKinds.WorkOrder, FieldServiceTestData.OrderId, order.Replace("\"status\":\"completed\"", "\"status\":\"dispatched\"", StringComparison.Ordinal).Replace("\"service_report\":" + FieldServiceTestData.Report, "\"service_report\":null", StringComparison.Ordinal), at);
        _ = store.Setup(s => s.PendingAsync(Any)).ReturnsAsync(
        [
            FieldServiceTestData.Entry(FieldServiceOperationType.PartConsumption, FieldServiceTestData.OrderId, FieldServiceRequests.PartPayload(FieldServiceTestData.Part()), at),
            FieldServiceTestData.Entry(FieldServiceOperationType.StatusTransition, FieldServiceTestData.OrderId, FieldServiceRequests.StatusPayload("on_site", at), at.AddMinutes(1)),
        ]);

        var result = await new GetFieldWorkOrderQueryHandler(store.Object).Handle(new GetFieldWorkOrderQuery(FieldServiceTestData.OrderId), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("on_site", result.Value.Status);
        Assert.Equal(3, result.Value.Parts.Count);
        Assert.True(result.Value.Parts[2].IsPending);
        Assert.Equal("Filter", result.Value.Parts[2].Description);
        Assert.False(result.Value.Parts[0].IsPending);
        Assert.Null(result.Value.Report);
    }

    /// <summary>
    /// A pending report appears as a pending report and completes the work order, and a pending status never moves it backwards.
    /// </summary>
    [Fact]
    public void Projection_PendingReportCompletesTheOrder_AndStatusNeverMovesBack()
    {
        var order = FieldServiceWorkOrderParser.ParseDetail(FieldServiceTestData.WorkOrder) with { Status = "on_site", Report = null };
        var at = new DateTime(2027, 3, 2, 9, 0, 0, DateTimeKind.Utc);
        var pending = new[]
        {
            FieldServiceTestData.Entry(FieldServiceOperationType.StatusTransition, FieldServiceTestData.OrderId, FieldServiceRequests.StatusPayload("en_route", at), at),
            FieldServiceTestData.Entry(FieldServiceOperationType.ServiceReport, FieldServiceTestData.OrderId, FieldServiceRequests.ReportPayload(FieldServiceTestData.ReportInput(), at), at.AddMinutes(1)),
            FieldServiceTestData.Entry(FieldServiceOperationType.PartConsumption, FieldServiceTestData.OtherOrderId, FieldServiceRequests.PartPayload(FieldServiceTestData.Part()), at),
        };

        var shown = FieldServiceLocalProjection.Apply(order, pending);

        Assert.Equal("completed", shown.Status);
        Assert.NotNull(shown.Report);
        Assert.True(shown.Report.IsPending);
        Assert.Equal("Mario Bianchi", shown.Report.SignerName);
        Assert.Equal("Filter replaced", shown.Report.Summary);
        Assert.Equal(2, shown.Parts.Count);
    }

    /// <summary>
    /// The outbox handlers forward to the offline service.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task OutboxHandlers_ForwardToTheOfflineService()
    {
        Mock<IFieldServiceOfflineService> offline = new();
        var queued = new FieldServiceQueueResult(ErrorKey: null, Sent: false, Conflict: false, "uuid");
        var part = FieldServiceTestData.Part();
        var report = FieldServiceTestData.ReportInput();
        var order = Guid.Parse(FieldServiceTestData.OrderId);
        var entry = Guid.NewGuid();
        _ = offline.Setup(o => o.QueuePartAsync(part, Any)).ReturnsAsync(queued);
        _ = offline.Setup(o => o.QueueReportAsync(report, Any)).ReturnsAsync(queued);
        _ = offline.Setup(o => o.QueueStatusAsync(order, "on_site", Any)).ReturnsAsync(queued);
        _ = offline.Setup(o => o.FlushAsync(Any)).ReturnsAsync(new FieldServiceFlushResult(1, 0, 0, Offline: false));
        _ = offline.Setup(o => o.SummaryAsync(Any)).ReturnsAsync(new FieldServiceOutboxSummary(2, []));

        Assert.Same(queued, await new RecordVanStockPartConsumptionCommandHandler(offline.Object).Handle(new RecordVanStockPartConsumptionCommand(part), CancellationToken.None));
        Assert.Same(queued, await new SignAndCloseServiceReportCommandHandler(offline.Object).Handle(new SignAndCloseServiceReportCommand(report), CancellationToken.None));
        Assert.Same(queued, await new AdvanceFieldWorkOrderStatusCommandHandler(offline.Object).Handle(new AdvanceFieldWorkOrderStatusCommand(order, "on_site"), CancellationToken.None));
        Assert.Equal(1, (await new FlushFieldServiceOutboxCommandHandler(offline.Object).Handle(new FlushFieldServiceOutboxCommand(), CancellationToken.None)).Sent);
        Assert.Equal(2, (await new GetFieldServiceOutboxQueryHandler(offline.Object).Handle(new GetFieldServiceOutboxQuery(), CancellationToken.None)).Pending);

        _ = await new DismissFieldServiceConflictCommandHandler(offline.Object).Handle(new DismissFieldServiceConflictCommand(entry), CancellationToken.None);

        offline.Verify(o => o.DismissConflictAsync(entry, Any), Times.Once);
    }

    private static DispatchTechnicianWorkOrderCommand Dispatch(DateTimeOffset start) =>
        new(FieldServiceTestData.OrderId, FieldServiceTestData.TechnicianId, start, start.AddHours(2));

    private static FieldServiceContractInput Contract(string? id) => new(
        id,
        "dddddddd-0000-0000-0000-0000000000aa",
        "MC-009",
        "Plan",
        new DateOnly(2027, 1, 1),
        EndDate: null,
        "active",
        MaintenanceCoverageType.PartsAndLabor,
        Frequency: null,
        IncludedVisits: 2,
        CalloutFee: 50m,
        HourlyRate: 80m,
        "CHF",
        Notes: null,
        []);

    private void Cache(string kind, string key, string json, DateTime at)
    {
        _ = store.Setup(s => s.FindAsync(kind, key, Any)).ReturnsAsync(new StoredFieldServicePayload(json, at));
    }
}
