// <copyright file="FieldServiceRequestsTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Enums;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the bodies sent to the Field Service endpoints and of the checks an operation passes before it enters the outbox.
/// </summary>
public sealed class FieldServiceRequestsTests
{
    private static readonly DateTime Moment = new(2027, 3, 2, 8, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// The dispatch body carries the technician and the window as ISO 8601 instants.
    /// </summary>
    [Fact]
    public void Dispatch_BuildsTheServerBody()
    {
        var start = new DateTimeOffset(2027, 3, 2, 9, 0, 0, TimeSpan.FromHours(1));

        using var body = JsonDocument.Parse(FieldServiceRequests.Dispatch(FieldServiceTestData.TechnicianId, start, start.AddHours(2)));

        Assert.Equal(FieldServiceTestData.TechnicianId, body.RootElement.GetProperty("technician_id").GetString());
        Assert.Equal(start, body.RootElement.GetProperty("scheduled_start").GetDateTimeOffset());
        Assert.Equal(start.AddHours(2), body.RootElement.GetProperty("scheduled_end").GetDateTimeOffset());
    }

    /// <summary>
    /// A new contract sends its number, the coverage type and the preventive rule as the server validates them.
    /// </summary>
    [Fact]
    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP004:Don't ignore created IDisposable", Justification = "The array enumerator is a struct whose Dispose is a no-op.")]
    public void Contract_New_SendsNumberCoverageAndRule()
    {
        var input = Contract(id: null, new PreventiveFrequency(6, "month", 500m, 10000, 10m, 14));

        using var body = JsonDocument.Parse(FieldServiceRequests.Contract(input));
        var root = body.RootElement;

        Assert.Equal("MC-001", root.GetProperty("contract_number").GetString());
        Assert.Equal("preventive_only", root.GetProperty("coverage_type").GetString());
        Assert.Equal("2027-01-01", root.GetProperty("start_date").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("end_date").ValueKind);
        Assert.Equal("50.5", root.GetProperty("callout_fee_amount").GetString());
        Assert.Equal(4, root.GetProperty("included_visits_per_year").GetInt32());
        var rule = root.GetProperty("preventive_frequency_rule");
        Assert.Equal(6, rule.GetProperty("calendar").GetProperty("every").GetInt32());
        Assert.Equal("month", rule.GetProperty("calendar").GetProperty("unit").GetString());
        Assert.Equal("500", rule.GetProperty("meter").GetProperty("operating_hours").GetString());
        Assert.Equal(10000, rule.GetProperty("meter").GetProperty("cycles").GetInt32());
        Assert.Equal(14, rule.GetProperty("lead_days").GetInt32());
        Assert.Equal<string>([FieldServiceTestData.EquipmentId], root.GetProperty("equipment_passport_ids").EnumerateArray().Select(e => e.GetString() ?? string.Empty), StringComparer.Ordinal);
    }

    /// <summary>
    /// An edited contract never sends its number again: it never changes, and the server would check it against itself.
    /// </summary>
    [Fact]
    public void Contract_Edit_OmitsTheNumber()
    {
        using var body = JsonDocument.Parse(FieldServiceRequests.Contract(Contract(FieldServiceTestData.ContractId, frequency: null)));

        Assert.False(body.RootElement.TryGetProperty("contract_number", out _));
        Assert.Equal(JsonValueKind.Null, body.RootElement.GetProperty("preventive_frequency_rule").ValueKind);
    }

    /// <summary>
    /// The invoice body carries the tax rate and the optional due date.
    /// </summary>
    [Fact]
    public void Invoice_BuildsTheServerBody()
    {
        using var body = JsonDocument.Parse(FieldServiceRequests.Invoice(8.1m, new DateOnly(2027, 4, 1)));

        Assert.Equal("8.1", body.RootElement.GetProperty("tax_rate").GetString());
        Assert.Equal("2027-04-01", body.RootElement.GetProperty("due_date").GetString());
    }

    /// <summary>
    /// The operation payloads use the field names of the mobile-sync rules.
    /// </summary>
    [Fact]
    public void Payloads_UseTheMobileSyncFieldNames()
    {
        using var status = JsonDocument.Parse(FieldServiceRequests.StatusPayload("on_site", Moment));
        Assert.Equal("on_site", status.RootElement.GetProperty("status").GetString());
        Assert.Equal(Moment, status.RootElement.GetProperty("at").GetDateTime().ToUniversalTime());

        using var part = JsonDocument.Parse(FieldServiceRequests.PartPayload(FieldServiceTestData.Part()));
        Assert.Equal(FieldServiceTestData.ProductId, part.RootElement.GetProperty("product_id").GetString());
        Assert.Equal("2", part.RootElement.GetProperty("quantity").GetString());
        Assert.Equal("12.5", part.RootElement.GetProperty("unit_price").GetString());
        Assert.Equal(JsonValueKind.Null, part.RootElement.GetProperty("lot_number").ValueKind);

        using var report = JsonDocument.Parse(FieldServiceRequests.ReportPayload(FieldServiceTestData.ReportInput(), Moment));
        var root = report.RootElement;
        Assert.Equal("Filter replaced", root.GetProperty("work_performed_summary").GetString());
        Assert.Equal("2.5", root.GetProperty("labor_hours").GetString());
        Assert.True(root.GetProperty("apply_callout_fee").GetBoolean());
        Assert.Equal("1520.5", root.GetProperty("meter_reading_hours").GetString());
        Assert.Equal(4000, root.GetProperty("meter_reading_cycles").GetInt32());
        Assert.Equal("de-CH", root.GetProperty("locale").GetString());
        Assert.Equal("pass", root.GetProperty("checklist")[0].GetProperty("result").GetString());
        Assert.StartsWith("<svg", root.GetProperty("signature_svg").GetString(), StringComparison.Ordinal);
    }

    /// <summary>
    /// The batch carries the client identifier, the type and the payload of every operation, in order.
    /// </summary>
    [Fact]
    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP004:Don't ignore created IDisposable", Justification = "The array enumerator is a struct whose Dispose is a no-op.")]
    public void Batch_CarriesTheClientIdentifierOfEachOperation()
    {
        var first = FieldServiceTestData.Entry(FieldServiceOperationType.PartConsumption, FieldServiceTestData.OrderId, FieldServiceRequests.PartPayload(FieldServiceTestData.Part()), Moment);
        var second = FieldServiceTestData.Entry(FieldServiceOperationType.StatusTransition, FieldServiceTestData.OrderId, FieldServiceRequests.StatusPayload("on_site", Moment), Moment);

        using var body = JsonDocument.Parse(FieldServiceBatch.Build([first, second]));
        var operations = body.RootElement.GetProperty("operations").EnumerateArray().ToList();

        Assert.Equal(2, operations.Count);
        Assert.Equal(first.ClientUuid, operations[0].GetProperty("client_uuid").GetString());
        Assert.Equal("part_consumption", operations[0].GetProperty("type").GetString());
        Assert.Equal(FieldServiceTestData.OrderId, operations[0].GetProperty("work_order_id").GetString());
        Assert.Equal(FieldServiceTestData.ProductId, operations[0].GetProperty("payload").GetProperty("product_id").GetString());
        Assert.Equal("status_transition", operations[1].GetProperty("type").GetString());
    }

    /// <summary>
    /// Only en route and on site can be set by the technician.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <param name="valid">Whether it is accepted.</param>
    [Theory]
    [InlineData("en_route", true)]
    [InlineData("on_site", true)]
    [InlineData("completed", false)]
    [InlineData("", false)]
    public void CheckStatus_AcceptsOnlyTechnicianStatuses(string status, bool valid) => Assert.Equal(valid, FieldServiceValidation.CheckStatus(status) is null);

    /// <summary>
    /// A part needs a positive quantity, a non-negative price and a lot of at most 60 characters.
    /// </summary>
    [Fact]
    public void CheckPart_RejectsWhatTheServerWouldReject()
    {
        Assert.Null(FieldServiceValidation.CheckPart(FieldServiceTestData.Part()));
        Assert.Equal(FieldServiceValidation.QuantityKey, FieldServiceValidation.CheckPart(FieldServiceTestData.Part(0m)));
        Assert.Equal(FieldServiceValidation.QuantityKey, FieldServiceValidation.CheckPart(FieldServiceTestData.Part() with { UnitPrice = -1m }));
        Assert.Equal(FieldServiceValidation.LotKey, FieldServiceValidation.CheckPart(FieldServiceTestData.Part() with { LotNumber = new string('x', 61) }));
        Assert.Null(FieldServiceValidation.CheckPart(FieldServiceTestData.Part() with { LotNumber = new string('x', 60) }));
    }

    /// <summary>
    /// A report is refused for a missing summary, hours out of range, a missing signer, an unsafe signature or a bad checklist.
    /// </summary>
    [Fact]
    public void CheckReport_RejectsWhatTheServerWouldReject()
    {
        var valid = FieldServiceTestData.ReportInput();

        Assert.Null(FieldServiceValidation.CheckReport(valid));
        Assert.Equal(FieldServiceValidation.SummaryKey, FieldServiceValidation.CheckReport(valid with { Summary = "  " }));
        Assert.Equal(FieldServiceValidation.HoursKey, FieldServiceValidation.CheckReport(valid with { LaborHours = 1000m }));
        Assert.Equal(FieldServiceValidation.HoursKey, FieldServiceValidation.CheckReport(valid with { TravelKm = -1m }));
        Assert.Equal(FieldServiceValidation.SignerKey, FieldServiceValidation.CheckReport(valid with { SignerName = string.Empty }));
        Assert.Equal(FieldServiceValidation.SignatureKey, FieldServiceValidation.CheckReport(valid with { SignatureSvg = "<svg><script>x()</script></svg>" }));
        Assert.Equal(FieldServiceValidation.SignatureKey, FieldServiceValidation.CheckReport(valid with { SignatureSvg = "<svg onload=\"x()\"></svg>" }));
        Assert.Equal(FieldServiceValidation.SignatureKey, FieldServiceValidation.CheckReport(valid with { SignatureSvg = "<div/>" }));
        Assert.Equal(FieldServiceValidation.ChecklistKey, FieldServiceValidation.CheckReport(valid with { Checklist = [new FieldServiceChecklistItem("x", "maybe", Notes: null)] }));
    }

    /// <summary>
    /// An unsupported language falls back to Italian, a supported one is kept.
    /// </summary>
    /// <param name="requested">The requested language.</param>
    /// <param name="expected">The language used.</param>
    [Theory]
    [InlineData("de-CH", "de-CH")]
    [InlineData("it-CH", "it-CH")]
    [InlineData("fr-CH", "fr-CH")]
    [InlineData("en", "en")]
    [InlineData("en-US", "it-IT")]
    [InlineData(null, "it-IT")]
    public void SupportedLocale_FallsBackToItalian(string? requested, string expected) => Assert.Equal(expected, FieldServiceValidation.SupportedLocale(requested));

    private static FieldServiceContractInput Contract(string? id, PreventiveFrequency? frequency) => new(
        id,
        "cust-1",
        "MC-001",
        "Chiller plan",
        new DateOnly(2027, 1, 1),
        EndDate: null,
        "active",
        MaintenanceCoverageType.PreventiveOnly,
        frequency,
        IncludedVisits: 4,
        CalloutFee: 50.5m,
        HourlyRate: null,
        "CHF",
        Notes: null,
        [FieldServiceTestData.EquipmentId]);
}
