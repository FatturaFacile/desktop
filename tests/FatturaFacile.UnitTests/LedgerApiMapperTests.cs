// <copyright file="LedgerApiMapperTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using FatturaFacile.Application.Accounting;
using FatturaFacile.Domain.Accounting;
using FatturaFacile.Domain.Entities;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// The mapping between the local ledger models and the JSON of the General Ledger endpoints.
/// </summary>
public sealed class LedgerApiMapperTests
{
    private static readonly Guid AccountA = new(0xA, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    private static readonly Guid AccountB = new(0xB, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    private static readonly Guid ServerA = new(0xAA, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    /// <summary>
    /// A posted entry becomes the POST journals body: dates, ledger type and book, status and four decimal amount strings.
    /// </summary>
    [Fact]
    public void BuildJournalBody_MatchesThePostJournalsContract()
    {
        GeneralLedgerEntry entry = new()
        {
            PostingDate = new DateTime(2026, 5, 17),
            DocumentDate = new DateTime(2026, 5, 16),
            Description = "Sale",
            LedgerType = LedgerBook.Ifrs,
            IsLocked = true,
        };
        entry.Lines.Add(new GeneralLedgerLine { AccountId = AccountA, LineNumber = 1, Debit = 1220m, Description = "Gross" });
        entry.Lines.Add(new GeneralLedgerLine { AccountId = AccountB, LineNumber = 2, Credit = 1220m, CostCenterId = new Guid(7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) });

        using var body = JsonDocument.Parse(LedgerApiMapper.BuildJournalBody(entry, new Dictionary<Guid, Guid> { [AccountA] = ServerA }));
        var root = body.RootElement;

        Assert.Equal("2026-05-17", root.GetProperty("posting_date").GetString());
        Assert.Equal("2026-05-16", root.GetProperty("document_date").GetString());
        Assert.Equal("ifrs", root.GetProperty("ledger_type").GetString());
        Assert.Equal("03", root.GetProperty("post_to_ledgers")[0].GetString());
        Assert.Equal("posted", root.GetProperty("status").GetString());
        var first = root.GetProperty("lines")[0];
        Assert.Equal(ServerA.ToString(), first.GetProperty("account_id").GetString());
        Assert.Equal("1220.0000", first.GetProperty("debit_amount").GetString());
        Assert.Equal("0.0000", first.GetProperty("credit_amount").GetString());
        var second = root.GetProperty("lines")[1];
        Assert.Equal(AccountB.ToString(), second.GetProperty("account_id").GetString());
        Assert.Equal("00000007-0000-0000-0000-000000000000", second.GetProperty("cost_center_id").GetString());
    }

    /// <summary>
    /// The reverse, preview and template post bodies carry the fields of their FormRequests.
    /// </summary>
    [Fact]
    public void BuildBodies_MatchTheFormRequests()
    {
        using var reverse = JsonDocument.Parse(LedgerApiMapper.BuildReverseBody(new DateTime(2026, 6, 1), "typo"));
        Assert.Equal("2026-06-01", reverse.RootElement.GetProperty("reversal_date").GetString());
        Assert.Equal("typo", reverse.RootElement.GetProperty("reason").GetString());

        var input = new TemplateInput(100m, 22m, new Dictionary<string, string>(StringComparer.Ordinal) { ["revenue"] = "3000" });
        using var preview = JsonDocument.Parse(LedgerApiMapper.BuildPreviewBody(input));
        Assert.Equal("100.0000", preview.RootElement.GetProperty("amount").GetString());
        Assert.Equal("22.0000", preview.RootElement.GetProperty("tax_rate").GetString());
        Assert.Equal("3000", preview.RootElement.GetProperty("accounts").GetProperty("revenue").GetString());

        using var post = JsonDocument.Parse(LedgerApiMapper.BuildPostTemplateBody(input, new DateTime(2026, 6, 2), "Invoice", LedgerBook.Tax, post: false));
        Assert.Equal("2026-06-02", post.RootElement.GetProperty("posting_date").GetString());
        Assert.Equal("draft", post.RootElement.GetProperty("status").GetString());
        Assert.Equal("02", post.RootElement.GetProperty("post_to_ledgers")[0].GetString());
        Assert.Equal("Invoice", post.RootElement.GetProperty("description").GetString());
    }

    /// <summary>
    /// The report queries use the ledger codes 01, 02 and 03 and the filters of the endpoints.
    /// </summary>
    [Fact]
    public void BuildReportQueries_UseLedgerCodes()
    {
        Assert.Equal(
            "from=2026-01-01&to=2026-12-31&ledger=02&columns=2",
            LedgerApiMapper.BuildTrialBalanceQuery(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), LedgerBook.Tax, 2, costCenterId: null));
        Assert.Equal(
            "from=2026-01-01&to=2026-12-31&ledger=01&columns=4&cost_center_id=00000007-0000-0000-0000-000000000000",
            LedgerApiMapper.BuildTrialBalanceQuery(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), LedgerBook.Statutory, 4, new Guid(7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)));
        Assert.Equal(
            "account_id=0000000a-0000-0000-0000-000000000000&from=2026-01-01&to=2026-01-31&ledger=03&page=2&per_page=50",
            LedgerApiMapper.BuildAccountLedgerQuery(AccountA, new DateTime(2026, 1, 1), new DateTime(2026, 1, 31), LedgerBook.Ifrs, 2, 50));
    }

    /// <summary>
    /// A two column trial balance of the server is read with string amounts and the balance flag.
    /// </summary>
    [Fact]
    public void ParseTrialBalance_ReadsTheTwoColumnPayload()
    {
        const string Json = """            {"currency":"EUR","accounts":[{"account_id":"0000000a-0000-0000-0000-000000000000","account_code":"1000","account_name":"Cash","account_type":"asset","normal_balance":"debit","opening_balance":"0.0000","debit":"300.5000","credit":"0.0000","closing_balance":"300.5000","debit_balance":"300.5000","credit_balance":"0.0000"},{"account_id":"0000000b-0000-0000-0000-000000000000","account_code":"3000","account_name":"Sales","account_type":"revenue","normal_balance":"credit","opening_balance":"0.0000","debit":"0.0000","credit":"300.5000","closing_balance":"300.5000","debit_balance":"0.0000","credit_balance":"300.5000"}],"totals":{},"is_balanced":true}""";

        var result = LedgerApiMapper.ParseTrialBalance(Json);

        Assert.Equal(2, result.Columns);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal(AccountA, result.Rows[0].AccountId);
        Assert.Equal(300.5m, result.Rows[0].DebitBalance);
        Assert.Equal(300.5m, result.TotalCreditBalance);
        Assert.True(result.IsBalanced);
    }

    /// <summary>
    /// An account ledger of the server keeps its balances and the running balance of each line.
    /// </summary>
    [Fact]
    public void ParseAccountLedger_ReadsLinesAndBalances()
    {
        const string Json = """            {"opening_balance":"10.0000","closing_balance":"25.0000","total_debit":"20.0000","total_credit":"5.0000","lines":[{"journal_entry_id":"0000000a-0000-0000-0000-000000000000","entry_number":"GLE-2026-0001","posting_date":"2026-02-01","description":"Sale","debit":"20.0000","credit":"0.0000","running_balance":"30.0000"},{"journal_entry_id":"0000000b-0000-0000-0000-000000000000","entry_number":"GLE-2026-0002","posting_date":"2026-02-02","description":"Refund","debit":"0.0000","credit":"5.0000","running_balance":"25.0000"}]}""";
        LedgerAccountInfo account = new(AccountA, "1000", "Cash", "asset", "debit");

        var result = LedgerApiMapper.ParseAccountLedger(Json, account);

        Assert.Equal(10m, result.OpeningBalance);
        Assert.Equal(25m, result.ClosingBalance);
        Assert.Equal([30m, 25m], result.Lines.Select(l => l.RunningBalance));
        Assert.Equal(new DateTime(2026, 2, 1), result.Lines[0].PostingDate.Date);
    }

    /// <summary>
    /// The server templates become cache entities that keep their JSON parts and hydrate like the local ones.
    /// </summary>
    [Fact]
    public void ParseTemplates_BuildsCacheEntities()
    {
        const string Json = """            [{"id":"0000000a-0000-0000-0000-000000000000","code":"SALE","category":"revenue","ledger_type":"tax","is_system":true,"names":{"it":"Vendita","de":"Verkauf","fr":"Vente","en":"Sale"},"parameters":[{"key":"rev","type":"account"}],"lines":[{"account":"1100","side":"debit","amount":"amount"},{"account":"$rev","side":"credit","amount":"amount"}]}]""";
        var tenant = Guid.NewGuid();
        var at = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);

        var templates = LedgerApiMapper.ParseTemplates(Json, tenant, at);

        var template = Assert.Single(templates);
        Assert.Equal(AccountA, template.Id);
        Assert.Equal(tenant, template.TenantId);
        Assert.Equal(LedgerBook.Tax, template.LedgerType);
        Assert.True(template.IsSystem);
        var definition = TemplateParser.ToDefinition(template, "de");
        Assert.Equal("Verkauf", definition.Name);
        Assert.Equal(["rev"], definition.AccountKeys);
    }

    /// <summary>
    /// The server preview lines are read back as line inputs.
    /// </summary>
    [Fact]
    public void ParseTemplatePreview_ReadsLines()
    {
        const string Json = """            {"lines":[{"account_id":"0000000a-0000-0000-0000-000000000000","debit_amount":"10.0000","credit_amount":"0.0000","description":null,"cost_center_id":null},{"account_id":"0000000b-0000-0000-0000-000000000000","debit_amount":"0.0000","credit_amount":"10.0000","description":"Net"}],"total_debit":"10.0000","total_credit":"10.0000","is_balanced":true}""";

        var (lines, balanced) = LedgerApiMapper.ParseTemplatePreview(Json);

        Assert.True(balanced);
        Assert.Equal(new JournalLineInput(AccountA, 10m, 0m), lines[0]);
        Assert.Equal(new JournalLineInput(AccountB, 0m, 10m, "Net"), lines[1]);
    }
}
