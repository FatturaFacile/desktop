// <copyright file="TemplateEngine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// Turns a parametric template and the inputs of the user into journal lines, mirroring the web-api preview: net, tax (half-up to
/// four decimals) and gross are the amounts a line can carry.
/// </summary>
public static class TemplateEngine
{
    private const int Scale = 4;

    /// <summary>
    /// Hydrates a template.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <param name="input">The inputs of the user.</param>
    /// <param name="accounts">The chart of accounts the account references are resolved against (by code or identifier).</param>
    /// <returns>The lines and the problems found.</returns>
    public static TemplatePreview Preview(TemplateDefinition template, TemplateInput input, IReadOnlyCollection<LedgerAccountInfo> accounts)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(accounts);

        List<TemplateIssue> issues = [];

        if (input.Amount <= 0m)
        {
            issues.Add(new TemplateIssue(TemplateIssueKind.AmountRequired, Key: null));
        }

        if (template.NeedsRate && input.TaxRate is null)
        {
            issues.Add(new TemplateIssue(TemplateIssueKind.RateRequired, Key: null));
        }

        List<(string Key, LedgerAccountInfo Account)> resolved = [];
        foreach (var line in template.Lines)
        {
            if (resolved.Exists(r => string.Equals(r.Key, line.AccountKey, StringComparison.Ordinal)))
            {
                continue;
            }

            var account = Resolve(line, input, accounts);
            if (account is null)
            {
                issues.Add(new TemplateIssue(TemplateIssueKind.AccountUnknown, line.AccountKey));
            }
            else
            {
                resolved.Add((line.AccountKey, account));
            }
        }

        if (issues.Count > 0)
        {
            return new TemplatePreview([], issues);
        }

        var net = Math.Round(input.Amount, Scale, MidpointRounding.AwayFromZero);
        var tax = template.NeedsRate ? Math.Round(net * (input.TaxRate ?? 0m) / 100m, Scale, MidpointRounding.AwayFromZero) : 0m;

        List<JournalLineInput> lines = [];
        foreach (var line in template.Lines)
        {
            var value = Amount(line.Amount, net, tax);
            if (value <= 0m)
            {
                continue;
            }

            var debit = string.Equals(line.Side, "debit", StringComparison.Ordinal);
            var account = resolved.Find(r => string.Equals(r.Key, line.AccountKey, StringComparison.Ordinal)).Account;
            lines.Add(new JournalLineInput(account.Id, debit ? value : 0m, debit ? 0m : value, line.Description, input.CostCenterId));
        }

        return new TemplatePreview(lines, []);
    }

    private static decimal Amount(string kind, decimal net, decimal tax) => kind switch
    {
        "amount" => net,
        "tax" => tax,
        "gross" => net + tax,
        _ => 0m,
    };

    private static LedgerAccountInfo? Resolve(TemplateLineDefinition line, TemplateInput input, IReadOnlyCollection<LedgerAccountInfo> accounts)
    {
        var reference = line.Account;
        var found = !line.IsParameter || input.Accounts.TryGetValue(line.AccountKey, out reference);

        return !found || string.IsNullOrWhiteSpace(reference)
            ? null
            : accounts.FirstOrDefault(a => string.Equals(a.Code, reference, StringComparison.Ordinal)
                || string.Equals(a.Id.ToString(), reference, StringComparison.OrdinalIgnoreCase));
    }
}
