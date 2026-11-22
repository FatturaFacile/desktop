// <copyright file="AccountingTemplateEngineTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Hydration of the parametric journal templates (causali contabili).
/// </summary>
public sealed class AccountingTemplateEngineTests
{
    private static readonly LedgerAccountInfo Receivable = new(new Guid(11, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0), "1100", "Receivables", "asset", "debit");
    private static readonly LedgerAccountInfo Revenue = new(new Guid(12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0), "3000", "Revenue", "revenue", "credit");
    private static readonly LedgerAccountInfo OutputVat = new(new Guid(13, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0), "2200", "Output VAT", "liability", "credit");

    private static readonly LedgerAccountInfo[] Chart = [Receivable, Revenue, OutputVat];

    /// <summary>
    /// A sales invoice at 22 percent yields gross to receivables, net to revenue and tax to VAT, balanced.
    /// </summary>
    [Fact]
    public void Preview_SalesInvoice_YieldsBalancedLines()
    {
        var template = BuiltInTemplates.All("en").Single(t => string.Equals(t.Code, "SALES_INVOICE", StringComparison.Ordinal));

        var preview = TemplateEngine.Preview(template, Input(1000m, 22m), Chart);

        Assert.Empty(preview.Issues);
        Assert.True(preview.IsBalanced);
        Assert.Equal(1220m, preview.Totals.Debit);
        Assert.Equal(new JournalLineInput(Receivable.Id, 1220m, 0m), preview.Lines[0]);
        Assert.Equal(new JournalLineInput(Revenue.Id, 0m, 1000m), preview.Lines[1]);
        Assert.Equal(new JournalLineInput(OutputVat.Id, 0m, 220m), preview.Lines[2]);
    }

    /// <summary>
    /// The tax is rounded half up to four decimals and the gross keeps the entry balanced.
    /// </summary>
    [Fact]
    public void Preview_TaxIsRoundedHalfUp_AndStaysBalanced()
    {
        var template = BuiltInTemplates.All("en").Single(t => string.Equals(t.Code, "SALES_INVOICE", StringComparison.Ordinal));

        var preview = TemplateEngine.Preview(template, Input(0.0225m, 10m), Chart);

        Assert.True(preview.IsBalanced);
        Assert.Equal(0.0023m, preview.Lines.Single(l => l.AccountId == OutputVat.Id).Credit);
    }

    /// <summary>
    /// The missing amount, rate and account are reported and no line is produced.
    /// </summary>
    [Fact]
    public void Preview_ReportsMissingInputs()
    {
        var template = BuiltInTemplates.All("en").Single(t => string.Equals(t.Code, "SALES_INVOICE", StringComparison.Ordinal));

        var preview = TemplateEngine.Preview(template, new TemplateInput(0m, TaxRate: null, new Dictionary<string, string>(StringComparer.Ordinal)), Chart);

        Assert.Empty(preview.Lines);
        Assert.Contains(preview.Issues, i => i.Kind == TemplateIssueKind.AmountRequired);
        Assert.Contains(preview.Issues, i => i.Kind == TemplateIssueKind.RateRequired);
        Assert.Contains(preview.Issues, i => i.Kind == TemplateIssueKind.AccountUnknown && string.Equals(i.Key, "revenue_account", StringComparison.Ordinal));
        Assert.False(preview.IsBalanced);
    }

    /// <summary>
    /// A template without rate parameter ignores the tax and an account can be given by identifier.
    /// </summary>
    [Fact]
    public void Preview_Payment_NeedsNoRate_AndAcceptsAccountIds()
    {
        var template = BuiltInTemplates.All("it").Single(t => string.Equals(t.Code, "CUSTOMER_PAYMENT", StringComparison.Ordinal));
        var input = new TemplateInput(
            250m,
            TaxRate: null,
            new Dictionary<string, string>(StringComparer.Ordinal) { ["bank_account"] = Receivable.Id.ToString(), ["receivable_account"] = "3000" });

        var preview = TemplateEngine.Preview(template, input, Chart);

        Assert.True(preview.IsBalanced);
        Assert.False(template.NeedsRate);
        Assert.Equal(250m, preview.Totals.Credit);
    }

    /// <summary>
    /// Every built-in template is hydrated into balanced lines and has a name in the four languages.
    /// </summary>
    [Fact]
    public void BuiltInTemplates_AreBalancedAndLocalized()
    {
        foreach (var language in new[] { "it", "de", "fr", "en" })
        {
            Assert.All(BuiltInTemplates.All(language), template => Assert.False(string.IsNullOrWhiteSpace(template.Name)));
        }

        foreach (var template in BuiltInTemplates.All("en"))
        {
            var accounts = template.AccountKeys.ToDictionary(k => k, _ => Receivable.Code, StringComparer.Ordinal);
            var preview = TemplateEngine.Preview(template, new TemplateInput(100m, template.NeedsRate ? 8.1m : null, accounts), Chart);

            Assert.True(preview.IsBalanced, template.Code);
        }
    }

    /// <summary>
    /// The JSON the server sends is read into the same definition: parameters, lines and the name by language with English fallback.
    /// </summary>
    [Fact]
    public void TemplateParser_ReadsTheServerJson()
    {
        const string Names = "{\"it\":\"Fattura\",\"en\":\"Invoice\"}";
        var parameters = TemplateParser.ParseParameters("[{\"key\":\"vat\",\"type\":\"rate\"},{\"key\":\"revenue\",\"type\":\"account\"}]");
        var lines = TemplateParser.ParseLines("[{\"account\":\"$revenue\",\"side\":\"credit\",\"amount\":\"amount\",\"description\":\"Net\"},{\"account\":\"1100\",\"side\":\"debit\",\"amount\":\"gross\"}]");

        Assert.Equal("Fattura", TemplateParser.PickName(Names, "it", "CODE"));
        Assert.Equal("Invoice", TemplateParser.PickName(Names, "de", "CODE"));
        Assert.Equal("CODE", TemplateParser.PickName("{}", "it", "CODE"));
        Assert.Equal<string>(["vat", "revenue"], parameters.Select(p => p.Key), StringComparer.Ordinal);
        Assert.True(lines[0].IsParameter);
        Assert.Equal("revenue", lines[0].AccountKey);
        Assert.Equal("Net", lines[0].Description);
        Assert.False(lines[1].IsParameter);
        Assert.Null(lines[1].Description);
    }

    private static TemplateInput Input(decimal amount, decimal rate) => new(
        amount,
        rate,
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["receivable_account"] = "1100",
            ["revenue_account"] = "3000",
            ["vat_account"] = "2200",
        });
}
