// <copyright file="LedgerApiMapper.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;
using System.Text.Json;
using FatturaFacile.Domain.Accounting;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// Maps between the local ledger models and the JSON of the General Ledger endpoints. Money is always a decimal, written and read
/// with the invariant culture; the server sends amounts as strings with four decimals.
/// </summary>
public static class LedgerApiMapper
{
    /// <summary>
    /// Builds the body of POST journals for a local entry. The ledger book is both the ledger type and the single book posted to.
    /// </summary>
    /// <param name="entry">The entry with its lines.</param>
    /// <param name="serverAccountIds">The server identifier of each local account; an account missing from the map keeps its local identifier.</param>
    /// <returns>The request JSON.</returns>
    public static string BuildJournalBody(GeneralLedgerEntry entry, IReadOnlyDictionary<Guid, Guid>? serverAccountIds = null)
    {
        ArgumentNullException.ThrowIfNull(entry);

        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("posting_date", Day(entry.PostingDate));
            writer.WriteString("document_date", Day(entry.DocumentDate));
            writer.WriteString("description", entry.Description);
            writer.WriteString("ledger_type", entry.LedgerType.ToApiName());
            writer.WriteStartArray("post_to_ledgers");
            writer.WriteStringValue(entry.LedgerType.ToCode());
            writer.WriteEndArray();
            writer.WriteString("status", entry.IsLocked ? "posted" : "draft");
            writer.WriteStartArray("lines");

            foreach (var line in entry.Lines.OrderBy(static l => l.LineNumber))
            {
                writer.WriteStartObject();
                var accountId = serverAccountIds is not null && serverAccountIds.TryGetValue(line.AccountId, out var mapped) ? mapped : line.AccountId;
                writer.WriteString("account_id", accountId);
                writer.WriteString("debit_amount", Money(line.Debit));
                writer.WriteString("credit_amount", Money(line.Credit));

                if (line.Description is not null)
                {
                    writer.WriteString("description", line.Description);
                }

                if (line.CostCenterId is { } costCenter)
                {
                    writer.WriteString("cost_center_id", costCenter);
                }

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Builds the body of POST journals/{id}/reverse.
    /// </summary>
    /// <param name="reversalDate">The reversal date.</param>
    /// <param name="reason">The optional reason.</param>
    /// <returns>The request JSON.</returns>
    public static string BuildReverseBody(DateTime reversalDate, string? reason) =>
        JsonSerializer.Serialize(new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["reversal_date"] = Day(reversalDate),
            ["reason"] = reason,
        });

    /// <summary>
    /// Builds the body of POST accounting-templates/{id}/preview.
    /// </summary>
    /// <param name="input">The inputs.</param>
    /// <returns>The request JSON.</returns>
    public static string BuildPreviewBody(TemplateInput input) => JsonSerializer.Serialize(TemplateFields(input));

    /// <summary>
    /// Builds the body of POST accounting-templates/{id}/post.
    /// </summary>
    /// <param name="input">The inputs.</param>
    /// <param name="postingDate">The posting date.</param>
    /// <param name="description">The optional narrative.</param>
    /// <param name="ledger">The ledger book to post to.</param>
    /// <param name="post">Whether to post or keep a draft.</param>
    /// <returns>The request JSON.</returns>
    public static string BuildPostTemplateBody(TemplateInput input, DateTime postingDate, string? description, LedgerBook ledger, bool post)
    {
        var body = TemplateFields(input);
        body["posting_date"] = Day(postingDate);
        body["status"] = post ? "posted" : "draft";
        body["post_to_ledgers"] = new[] { ledger.ToCode() };

        if (description is not null)
        {
            body["description"] = description;
        }

        return JsonSerializer.Serialize(body);
    }

    /// <summary>
    /// Builds the query string of GET reports/trial-balance.
    /// </summary>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <param name="ledger">The ledger book.</param>
    /// <param name="columns">The layout, 2 or 4.</param>
    /// <param name="costCenterId">The optional cost center.</param>
    /// <returns>The query string.</returns>
    public static string BuildTrialBalanceQuery(DateTime from, DateTime to, LedgerBook ledger, int columns, Guid? costCenterId)
    {
        var query = string.Create(CultureInfo.InvariantCulture, $"from={Day(from)}&to={Day(to)}&ledger={ledger.ToCode()}&columns={(columns == 2 ? 2 : 4)}");

        return costCenterId is { } id ? query + "&cost_center_id=" + id : query;
    }

    /// <summary>
    /// Builds the query string of GET reports/account-ledger.
    /// </summary>
    /// <param name="accountId">The account.</param>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <param name="ledger">The ledger book.</param>
    /// <param name="page">The page.</param>
    /// <param name="perPage">The page size.</param>
    /// <returns>The query string.</returns>
    public static string BuildAccountLedgerQuery(Guid accountId, DateTime from, DateTime to, LedgerBook ledger, int page, int perPage) =>
        string.Create(CultureInfo.InvariantCulture, $"account_id={accountId}&from={Day(from)}&to={Day(to)}&ledger={ledger.ToCode()}&page={page}&per_page={perPage}");

    /// <summary>
    /// Reads a server trial balance.
    /// </summary>
    /// <param name="json">The data JSON.</param>
    /// <returns>The trial balance.</returns>
    public static TrialBalanceResult ParseTrialBalance(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var rows = LedgerJson.Items(root.GetProperty("accounts")).ConvertAll(static a => new TrialBalanceRow(
            LedgerJson.Id(a, "account_id") ?? Guid.Empty,
            LedgerJson.Text(a, "account_code"),
            LedgerJson.Text(a, "account_name"),
            LedgerJson.Text(a, "account_type"),
            LedgerJson.Text(a, "normal_balance"),
            LedgerJson.Number(a, "opening_balance"),
            LedgerJson.Number(a, "debit"),
            LedgerJson.Number(a, "credit"),
            LedgerJson.Number(a, "closing_balance"),
            LedgerJson.Number(a, "debit_balance"),
            LedgerJson.Number(a, "credit_balance")));

        var columns = rows.Exists(static r => r.DebitBalance != 0m || r.CreditBalance != 0m) ? 2 : 4;

        return new TrialBalanceResult(
            columns,
            rows,
            rows.Sum(static r => r.Debit),
            rows.Sum(static r => r.Credit),
            rows.Sum(static r => r.DebitBalance),
            rows.Sum(static r => r.CreditBalance),
            LedgerJson.Flag(root, "is_balanced"));
    }

    /// <summary>
    /// Reads a server account ledger.
    /// </summary>
    /// <param name="json">The data JSON.</param>
    /// <param name="account">The account the ledger is of.</param>
    /// <returns>The account ledger.</returns>
    public static AccountLedgerResult ParseAccountLedger(string json, LedgerAccountInfo account)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentNullException.ThrowIfNull(account);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var lines = LedgerJson.Items(root.GetProperty("lines")).ConvertAll(static l => new AccountLedgerLine(
            LedgerJson.Id(l, "journal_entry_id") ?? Guid.Empty,
            LedgerJson.Text(l, "entry_number"),
            DateTime.Parse(LedgerJson.Text(l, "posting_date"), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal),
            LedgerJson.Text(l, "description"),
            LedgerJson.Number(l, "debit"),
            LedgerJson.Number(l, "credit"),
            LedgerJson.Number(l, "running_balance")));

        return new AccountLedgerResult(
            account,
            LedgerJson.Number(root, "opening_balance"),
            LedgerJson.Number(root, "closing_balance"),
            LedgerJson.Number(root, "total_debit"),
            LedgerJson.Number(root, "total_credit"),
            lines,
            Page: 1,
            PerPage: Math.Max(lines.Count, 1),
            TotalLines: lines.Count);
    }

    /// <summary>
    /// Reads the accounting templates of the server into cache entities.
    /// </summary>
    /// <param name="json">The data JSON: an array of templates.</param>
    /// <param name="tenantId">The active tenant.</param>
    /// <param name="syncedAt">The pull instant.</param>
    /// <returns>The templates.</returns>
    public static IReadOnlyList<AccountingTemplate> ParseTemplates(string json, Guid tenantId, DateTime syncedAt)
    {
        ArgumentNullException.ThrowIfNull(json);

        using var document = JsonDocument.Parse(json);

        return LedgerJson.Items(document.RootElement).ConvertAll(t => new AccountingTemplate
        {
            Id = LedgerJson.Id(t, "id") ?? Guid.NewGuid(),
            TenantId = tenantId,
            Code = LedgerJson.Text(t, "code"),
            Category = LedgerJson.OptionalText(t, "category") ?? "general",
            LedgerType = LedgerBookCodes.FromApi(LedgerJson.Text(t, "ledger_type")),
            NamesJson = Raw(t, "names", "{}"),
            ParametersJson = Raw(t, "parameters", "[]"),
            LinesJson = Raw(t, "lines", "[]"),
            IsSystem = LedgerJson.Flag(t, "is_system"),
            SyncedAt = syncedAt,
        });
    }

    /// <summary>
    /// Reads the preview of a template computed by the server.
    /// </summary>
    /// <param name="json">The data JSON.</param>
    /// <returns>The lines and the server balance flag.</returns>
    public static (IReadOnlyList<JournalLineInput> Lines, bool IsBalanced) ParseTemplatePreview(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var lines = LedgerJson.Items(root.GetProperty("lines")).ConvertAll(static l => new JournalLineInput(
            LedgerJson.Id(l, "account_id") ?? Guid.Empty,
            LedgerJson.Number(l, "debit_amount"),
            LedgerJson.Number(l, "credit_amount"),
            LedgerJson.OptionalText(l, "description"),
            LedgerJson.Id(l, "cost_center_id")));

        return (lines, LedgerJson.Flag(root, "is_balanced"));
    }

    private static Dictionary<string, object?> TemplateFields(TemplateInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        Dictionary<string, object?> body = new(StringComparer.Ordinal)
        {
            ["amount"] = Money(input.Amount),
            ["accounts"] = input.Accounts,
        };

        if (input.TaxRate is { } rate)
        {
            body["tax_rate"] = Money(rate);
        }

        if (input.CostCenterId is { } costCenter)
        {
            body["cost_center_id"] = costCenter;
        }

        return body;
    }

    private static string Raw(JsonElement element, string property, string fallback) =>
        element.TryGetProperty(property, out var value) ? value.GetRawText() : fallback;

    private static string Day(DateTime value) => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string Money(decimal value) => value.ToString("0.0000", CultureInfo.InvariantCulture);
}
