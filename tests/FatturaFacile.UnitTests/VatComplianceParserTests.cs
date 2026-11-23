// <copyright file="VatComplianceParserTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests that the answers of the VAT compliance endpoints become local rows, that the request bodies are well formed and that a
/// payload that does not parse is refused.
/// </summary>
public sealed class VatComplianceParserTests
{
    private const string LetterId = "7f0c1f0e-6a7e-4b0e-9d7b-0c1f0e6a7e4b";
    private const string ContactId = "11111111-2222-3333-4444-555555555555";
    private const string SettlementId = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";
    private const string SupplierId = "99999999-8888-7777-6666-555555555555";

#pragma warning disable MA0176 // The identifiers are fixtures that must match the JSON constants above.
    private static readonly Guid LetterGuid = new("7f0c1f0e-6a7e-4b0e-9d7b-0c1f0e6a7e4b");
    private static readonly Guid ContactGuid = new("11111111-2222-3333-4444-555555555555");
    private static readonly Guid SettlementGuid = new("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
#pragma warning restore MA0176

    /// <summary>
    /// A letter of the web-api keeps its identifiers, protocol, amounts and validity.
    /// </summary>
    [Fact]
    public void IntentLetters_AreReadAsSyncedRows()
    {
        var json = $$"""
            [{"id":"{{LetterId}}","contact_id":"{{ContactId}}","direction":"received","protocol_number":"08060120341234567-000001","receipt_date":"2027-01-15","year":2027,"plafond_amount":"50000.00","consumed_amount":"12500.50","valid_from":"2027-01-01","valid_to":"2027-12-31","notes":null}]
            """;

        var letter = Assert.Single(VatComplianceParser.ParseIntentLetters(json));

        Assert.Equal(LetterGuid, letter.ServerId);
        Assert.Equal(ContactGuid, letter.ContactId);
        Assert.Equal(IntentLetterDirection.Received, letter.Direction);
        Assert.Equal("08060120341234567-000001", letter.ProtocolNumber);
        Assert.Equal(50000m, letter.PlafondAmount);
        Assert.Equal(12500.50m, letter.ServerConsumedAmount);
        Assert.Equal(new DateTime(2027, 12, 31, 0, 0, 0, DateTimeKind.Unspecified), letter.ValidTo);
        Assert.Null(letter.Notes);
        Assert.Equal(VatComplianceSyncState.Synced, letter.SyncState);
    }

    /// <summary>
    /// A settlement keeps its period and amounts, and a credit becomes a negative amount payable.
    /// </summary>
    [Fact]
    public void Settlements_AreReadWithTheCreditAsNegativePayable()
    {
        var at = new DateTime(2027, 4, 2, 0, 0, 0, DateTimeKind.Utc);
        var json = $$"""
            [{"id":"{{SettlementId}}","period_type":"quarterly","year":2027,"period":1,"period_from":"2027-01-01","period_to":"2027-03-31","output_vat_amount":220.5,"input_vat_amount":300,"previous_credit":"10.00","previous_debit":"0.00","interest_amount":"0.00","payable_amount":"0.00","net_vat_credit":89.5,"status":"posted","settlement_journal_id":null}]
            """;

        var settlement = Assert.Single(VatComplianceParser.ParseSettlements(json, at));

        Assert.Equal(SettlementGuid, settlement.ServerId);
        Assert.Equal(220.5m, settlement.OutputVat);
        Assert.Equal(300m, settlement.InputVat);
        Assert.Equal(-89.5m, settlement.PayableAmount);
        Assert.Equal("posted", settlement.Status);
        Assert.Equal(at, settlement.SyncedAtUtc);
        Assert.Null(settlement.SettlementJournalId);
    }

    /// <summary>
    /// A withholding settlement gives its withheld payments and one F24 payment per tax code, paid when the settlement is paid.
    /// </summary>
    [Fact]
    public void WithholdingSettlement_GivesLinesAndOneF24PaymentPerTaxCode()
    {
        var json = $$"""
            {"id":"{{SettlementId}}","year":2027,"month":3,"due_date":"2027-04-16","status":"paid","payment_date":"2027-04-15","lines":[{"supplier_id":"{{SupplierId}}","invoice_number":"F1","paid_at":"2027-03-05","reason":"A","f24_code":"1040","gross_paid":"1000.00","taxable_amount":"800.00","rate_percent":"20.00","withheld_amount":"160.00"},{"supplier_id":"{{SupplierId}}","invoice_number":"F2","paid_at":"2027-03-20","reason":"A","f24_code":"1040","gross_paid":"500.00","taxable_amount":"400.00","rate_percent":"20.00","withheld_amount":"80.00"},{"supplier_id":"{{SupplierId}}","invoice_number":"F3","paid_at":"2027-03-21","reason":"R","f24_code":"1038","gross_paid":"300.00","taxable_amount":"120.00","rate_percent":"23.00","withheld_amount":"27.60"}]}
            """;

        var (payments, lines) = VatComplianceParser.ParseWithholdingSettlement(json);

        Assert.Equal(3, lines.Count);
        Assert.All(lines, static l => Assert.Equal(SettlementGuid, l.SettlementServerId));
        Assert.Equal(2, payments.Count);
        var main = Assert.Single(payments, static p => string.Equals(p.TaxCode, "1040", StringComparison.Ordinal));
        Assert.Equal(240m, main.Amount);
        Assert.Equal(new DateTime(2027, 4, 16, 0, 0, 0, DateTimeKind.Unspecified), main.DueDate);
        Assert.Equal(new DateTime(2027, 4, 15, 0, 0, 0, DateTimeKind.Unspecified), main.PaidDate);
        Assert.Equal(VatComplianceSyncState.Synced, main.SyncState);
    }

    /// <summary>
    /// The settlements of a year are listed by identifier.
    /// </summary>
    [Fact]
    public void WithholdingSettlementIds_AreListed()
    {
        var ids = VatComplianceParser.ParseWithholdingSettlementIds($$"""[{"id":"{{SettlementId}}"},{"id":"{{LetterId}}"}]""");

        Guid[] expected = [SettlementGuid, LetterGuid];

        Assert.Equal(expected, ids.ToArray());
    }

    /// <summary>
    /// The protocol numbers of a register are read once per document, even when the document has several rate lines.
    /// </summary>
    [Fact]
    public void RegisterProtocols_AreReadOncePerDocument()
    {
        const string Json = """
            [{"document_number":"2027/1","protocol_number":1},{"document_number":"2027/1","protocol_number":1},{"document_number":"2027/2","protocol_number":2},{"document_number":"2027/3","protocol_number":null}]
            """;

        var protocols = VatComplianceParser.ParseRegisterProtocols(Json);

        Assert.Equal(2, protocols.Count);
        Assert.Equal(2, protocols["2027/2"]);
        Assert.False(protocols.ContainsKey("2027/3"));
    }

    /// <summary>
    /// The request body of a letter carries the fields the web-api validates, with dates as plain days.
    /// </summary>
    [Fact]
    public void IntentLetterJson_CarriesTheRequestFields()
    {
        IntentLetter letter = new()
        {
            ContactId = ContactGuid,
            Direction = IntentLetterDirection.Received,
            ProtocolNumber = "08060120341234567-000001",
            ReceiptDate = new DateTime(2027, 1, 15, 0, 0, 0, DateTimeKind.Unspecified),
            Year = 2027,
            PlafondAmount = 50000.5m,
            ValidFrom = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
            ValidTo = new DateTime(2027, 12, 31, 0, 0, 0, DateTimeKind.Unspecified),
            Notes = "Export \"Q1\"",
        };

        using var document = JsonDocument.Parse(VatComplianceParser.ToIntentLetterJson(letter));
        var root = document.RootElement;

        Assert.Equal(ContactId, root.GetProperty("contact_id").GetString());
        Assert.Equal("received", root.GetProperty("direction").GetString());
        Assert.Equal("2027-01-15", root.GetProperty("receipt_date").GetString());
        Assert.Equal(50000.5m, root.GetProperty("plafond_amount").GetDecimal());
        Assert.Equal("2027-12-31", root.GetProperty("valid_to").GetString());
        Assert.Equal("Export \"Q1\"", root.GetProperty("notes").GetString());
    }

    /// <summary>
    /// The payment body and the identifier of a created letter.
    /// </summary>
    [Fact]
    public void PaymentJsonAndCreatedId_RoundTrip()
    {
        using var payment = JsonDocument.Parse(VatComplianceParser.ToPaymentJson(new DateTime(2027, 4, 15, 0, 0, 0, DateTimeKind.Unspecified)));

        Assert.Equal("2027-04-15", payment.RootElement.GetProperty("payment_date").GetString());
        Assert.Equal(LetterGuid, VatComplianceParser.ParseCreatedId("{\"data\":{\"id\":\"" + LetterId + "\"}}"));
        Assert.Null(VatComplianceParser.ParseCreatedId("""{"data":{}}"""));
    }

    /// <summary>
    /// Payloads that are not what the web-api documents are refused instead of replacing good data.
    /// </summary>
    [Fact]
    public void BrokenPayloads_AreRefused()
    {
        _ = Assert.ThrowsAny<JsonException>(() => VatComplianceParser.ParseIntentLetters("not json"));
        _ = Assert.Throws<JsonException>(() => VatComplianceParser.ParseIntentLetters("""[{"contact_id":"x"}]"""));
        _ = Assert.ThrowsAny<Exception>(() => VatComplianceParser.ParseSettlements("""[{"id":"not-a-guid"}]""", DateTime.UtcNow));
    }
}
