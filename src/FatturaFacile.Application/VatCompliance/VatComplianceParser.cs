// <copyright file="VatComplianceParser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.Json;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// Reads the answers of the VAT compliance endpoints of the web-api into local rows and writes the request bodies. A payload that
/// does not parse throws <see cref="JsonException"/>, so that it never replaces good local data.
/// </summary>
[SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP004:Don't ignore created IDisposable", Justification = "The array enumerator is a struct whose Dispose is a no-op; it is enumerated to completion.")]
public static class VatComplianceParser
{
    private const string DateFormat = "yyyy-MM-dd";

    /// <summary>
    /// Reads the declarations of intent.
    /// </summary>
    /// <param name="json">The `data` array.</param>
    /// <returns>The letters, in Synced state.</returns>
    public static IReadOnlyList<IntentLetter> ParseIntentLetters(string json)
    {
        using var document = JsonDocument.Parse(json);
        List<IntentLetter> letters = [];

        foreach (var element in document.RootElement.EnumerateArray())
        {
            letters.Add(ToLetter(element));
        }

        return letters;
    }

    /// <summary>
    /// Reads the VAT settlements.
    /// </summary>
    /// <param name="json">The `data` array.</param>
    /// <param name="syncedAtUtc">When the answer was received.</param>
    /// <returns>The settlements.</returns>
    public static IReadOnlyList<VatSettlementSnapshot> ParseSettlements(string json, DateTime syncedAtUtc)
    {
        using var document = JsonDocument.Parse(json);
        List<VatSettlementSnapshot> settlements = [];

        foreach (var element in document.RootElement.EnumerateArray())
        {
            settlements.Add(ToSettlement(element, syncedAtUtc));
        }

        return settlements;
    }

    /// <summary>
    /// Lists the withholding settlements of a year.
    /// </summary>
    /// <param name="json">The `data` array.</param>
    /// <returns>The identifiers of the settlements.</returns>
    public static IReadOnlyList<Guid> ParseWithholdingSettlementIds(string json)
    {
        using var document = JsonDocument.Parse(json);
        List<Guid> ids = [];

        foreach (var element in document.RootElement.EnumerateArray())
        {
            ids.Add(Guid.Parse(Text(element, "id")));
        }

        return ids;
    }

    /// <summary>
    /// Reads one withholding settlement: the F24 payments (one per tax code) and the withheld payments.
    /// </summary>
    /// <param name="json">The `data` object with its lines.</param>
    /// <returns>The payments and the lines.</returns>
    public static (IReadOnlyList<WithholdingF24Payment> Payments, IReadOnlyList<WithholdingLine> Lines) ParseWithholdingSettlement(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var settlementId = Guid.Parse(Text(root, "id"));
        var year = Integer(root, "year");
        var month = Integer(root, "month");
        var due = Optional(root, "due_date") is { } text
            ? DateTime.ParseExact(text[..DateFormat.Length], DateFormat, CultureInfo.InvariantCulture)
            : new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Unspecified).AddMonths(1).AddDays(15);
        var paidDate = PaidDateOf(root);

        List<WithholdingLine> lines = [];
        if (root.TryGetProperty("lines", out var array) && array.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in array.EnumerateArray())
            {
                lines.Add(ToLine(element, settlementId, year, month));
            }
        }

        List<WithholdingF24Payment> payments = [];
        foreach (var group in lines.GroupBy(static l => l.F24Code, StringComparer.Ordinal))
        {
            payments.Add(ToPayment(group.Key, group.Sum(static l => l.WithheldAmount), year, month, due, paidDate, settlementId));
        }

        return (payments, lines);
    }

    /// <summary>
    /// Reads the protocol numbers the web-api assigned to the documents of a register.
    /// </summary>
    /// <param name="json">The `data` array of the register lines.</param>
    /// <returns>The protocol number of each document number that has one.</returns>
    public static IReadOnlyDictionary<string, int> ParseRegisterProtocols(string json)
    {
        using var document = JsonDocument.Parse(json);
        List<(string Number, int Protocol)> found = [];

        foreach (var line in document.RootElement.EnumerateArray())
        {
            if (Optional(line, "document_number") is { } number && line.TryGetProperty("protocol_number", out var protocol) && protocol.ValueKind == JsonValueKind.Number)
            {
                found.Add((number, protocol.GetInt32()));
            }
        }

        // The lines of one document, one per rate, carry the same protocol.
        return found.DistinctBy(static f => f.Number, StringComparer.Ordinal).ToDictionary(static f => f.Number, static f => f.Protocol, StringComparer.Ordinal);
    }

    /// <summary>
    /// Writes the request body of a declaration of intent.
    /// </summary>
    /// <param name="letter">The letter.</param>
    /// <returns>The JSON body.</returns>
    public static string ToIntentLetterJson(IntentLetter letter)
    {
        ArgumentNullException.ThrowIfNull(letter);

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("contact_id", letter.ContactId);
            writer.WriteString("direction", letter.Direction == IntentLetterDirection.Issued ? "issued" : "received");
            writer.WriteString("protocol_number", letter.ProtocolNumber);
            writer.WriteString("receipt_date", letter.ReceiptDate.ToString(DateFormat, CultureInfo.InvariantCulture));
            writer.WriteNumber("year", letter.Year);
            writer.WriteNumber("plafond_amount", letter.PlafondAmount);
            writer.WriteString("valid_from", letter.ValidFrom.ToString(DateFormat, CultureInfo.InvariantCulture));
            writer.WriteString("valid_to", letter.ValidTo.ToString(DateFormat, CultureInfo.InvariantCulture));
            writer.WriteString("notes", letter.Notes);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Writes the request body that marks a withholding settlement as paid.
    /// </summary>
    /// <param name="paymentDate">The date of the payment.</param>
    /// <returns>The JSON body.</returns>
    public static string ToPaymentJson(DateTime paymentDate)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("payment_date", paymentDate.ToString(DateFormat, CultureInfo.InvariantCulture));
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Reads the identifier of a created declaration from the answer of the web-api.
    /// </summary>
    /// <param name="body">The answer body.</param>
    /// <returns>The identifier, or null when the answer has none.</returns>
    public static Guid? ParseCreatedId(string body)
    {
        using var document = JsonDocument.Parse(body);

        return document.RootElement.TryGetProperty("data", out var data) && data.TryGetProperty("id", out var id) && Guid.TryParse(id.GetString(), out var parsed) ? parsed : null;
    }

    private static IntentLetter ToLetter(JsonElement element) => new()
    {
        ServerId = Guid.Parse(Text(element, "id")),
        ContactId = Guid.Parse(Text(element, "contact_id")),
        Direction = string.Equals(Optional(element, "direction"), "issued", StringComparison.OrdinalIgnoreCase) ? IntentLetterDirection.Issued : IntentLetterDirection.Received,
        ProtocolNumber = Text(element, "protocol_number"),
        ReceiptDate = Date(element, "receipt_date"),
        Year = Integer(element, "year"),
        PlafondAmount = Number(element, "plafond_amount"),
        ServerConsumedAmount = Number(element, "consumed_amount"),
        ValidFrom = Date(element, "valid_from"),
        ValidTo = Date(element, "valid_to"),
        Notes = Optional(element, "notes"),
        SyncState = VatComplianceSyncState.Synced,
    };

    private static VatSettlementSnapshot ToSettlement(JsonElement element, DateTime syncedAtUtc) => new()
    {
        ServerId = Guid.Parse(Text(element, "id")),
        PeriodType = Optional(element, "period_type") ?? string.Empty,
        Year = Integer(element, "year"),
        Period = Integer(element, "period"),
        PeriodFrom = Date(element, "period_from"),
        PeriodTo = Date(element, "period_to"),
        OutputVat = Number(element, "output_vat_amount"),
        InputVat = Number(element, "input_vat_amount"),
        PreviousCredit = Number(element, "previous_credit"),
        PreviousDebit = Number(element, "previous_debit"),
        Interest = Number(element, "interest_amount"),

        // The settlement carries a credit apart from the amount payable; here a credit is a negative amount.
        PayableAmount = Number(element, "payable_amount") - Number(element, "net_vat_credit"),
        Status = Optional(element, "status") ?? string.Empty,
        SettlementJournalId = Optional(element, "settlement_journal_id") is { } journal ? Guid.Parse(journal) : null,
        SyncedAtUtc = syncedAtUtc,
    };

    private static WithholdingLine ToLine(JsonElement element, Guid settlementId, int year, int month) => new()
    {
        SettlementServerId = settlementId,
        Year = year,
        Month = month,
        SupplierId = Guid.Parse(Text(element, "supplier_id")),
        InvoiceNumber = Optional(element, "invoice_number") ?? string.Empty,
        PaidAt = Date(element, "paid_at"),
        Reason = Optional(element, "reason") ?? string.Empty,
        F24Code = Optional(element, "f24_code") ?? string.Empty,
        GrossPaid = Number(element, "gross_paid"),
        TaxableAmount = Number(element, "taxable_amount"),
        RatePercent = Number(element, "rate_percent"),
        WithheldAmount = Number(element, "withheld_amount"),
    };

    private static WithholdingF24Payment ToPayment(string taxCode, decimal amount, int year, int month, DateTime due, DateTime? paidDate, Guid settlementId) => new()
    {
        Year = year,
        Month = month,
        TaxCode = taxCode,
        Amount = amount,
        DueDate = due,
        PaidDate = paidDate,
        SettlementServerId = settlementId,
        SyncState = VatComplianceSyncState.Synced,
    };

    private static DateTime? PaidDateOf(JsonElement root)
    {
        if (!string.Equals(Optional(root, "status"), "paid", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var text = Optional(root, "payment_date");

        return text is null ? null : DateTime.ParseExact(text[..DateFormat.Length], DateFormat, CultureInfo.InvariantCulture);
    }

    private static string Text(JsonElement element, string name) =>
        Optional(element, name) ?? throw new JsonException("The answer has no " + name + ".");

    private static string? Optional(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) ? TextOf(value) : null;

    private static string? TextOf(JsonElement value) => value.ValueKind == JsonValueKind.String ? value.GetString() : RawOf(value);

    private static string? RawOf(JsonElement value) => value.ValueKind == JsonValueKind.Null ? null : value.GetRawText();

    private static int Integer(JsonElement element, string name) =>
        int.Parse(Text(element, name), NumberStyles.Integer, CultureInfo.InvariantCulture);

    private static decimal Number(JsonElement element, string name) =>
        string.IsNullOrEmpty(Optional(element, name)) ? 0m : decimal.Parse(Optional(element, name)!, NumberStyles.Float, CultureInfo.InvariantCulture);

    private static DateTime Date(JsonElement element, string name) =>
        DateTime.ParseExact(Text(element, name)[..DateFormat.Length], DateFormat, CultureInfo.InvariantCulture);
}
