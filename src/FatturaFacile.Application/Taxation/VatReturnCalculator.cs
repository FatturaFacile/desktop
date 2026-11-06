// <copyright file="VatReturnCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Taxation;

/// <summary>
/// Computes a periodic VAT return: Swiss MWST statement with the effective method (turnover, tax by rate, input tax) or
/// Italian periodic liquidation (VAT on sales without split payment and reverse charge, deductible VAT, previous credit,
/// 1 % interest for quarterly taxpayers, minimum payment of EUR 25.82).
/// </summary>
public static class VatReturnCalculator
{
    /// <summary>
    /// The Italian minimum amount to pay; smaller debts are carried to the next period.
    /// </summary>
    public const decimal ItalianMinimumPayment = 25.82m;

    /// <summary>
    /// Computes the return.
    /// </summary>
    /// <param name="country">CH or IT.</param>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <param name="sales">The issued customer invoices of the period with their items.</param>
    /// <param name="purchases">The posted supplier invoices registered in the period.</param>
    /// <param name="previousCredit">The credit carried over from the previous return.</param>
    /// <returns>The return (not saved).</returns>
    public static VatReturn Compute(
        string country,
        DateTime from,
        DateTime to,
        IReadOnlyList<(Invoice Invoice, IReadOnlyList<InvoiceItem> Items)> sales,
        IReadOnlyList<PurchaseInvoice> purchases,
        decimal previousCredit)
    {
        ArgumentNullException.ThrowIfNull(sales);
        ArgumentNullException.ThrowIfNull(purchases);

        var italian = string.Equals(country, "IT", StringComparison.OrdinalIgnoreCase);
        StringBuilder details = new();
        Dictionary<decimal, (decimal Taxable, decimal Tax)> byRate = [];
        var turnover = 0m;

        foreach (var (invoice, items) in sales)
        {
            var sign = invoice.InvoiceType.Contains("credit", StringComparison.OrdinalIgnoreCase) ? -1m : 1m;
            turnover += sign * invoice.TotalNet;
            var vatCollected = !(italian && (invoice.SplitPayment || invoice.ReverseCharge));

            foreach (var item in items.Where(static i => i.TaxRate > 0m))
            {
                var tax = vatCollected ? Math.Round(item.LineTotal * item.TaxRate / 100m, 2, MidpointRounding.AwayFromZero) : 0m;
                var (taxable, collected) = byRate.GetValueOrDefault(item.TaxRate);
                byRate[item.TaxRate] = (taxable + (sign * item.LineTotal), collected + (sign * tax));
            }
        }

        foreach (var (rate, values) in byRate.OrderByDescending(static r => r.Key))
        {
            _ = details.AppendLine(CultureInfo.InvariantCulture, $"{rate:0.##}%: imponibile {values.Taxable:N2}, imposta {values.Tax:N2}");
        }

        var output = byRate.Values.Sum(static v => v.Tax);
        var input = purchases.Sum(static p => p.DeductibleVat);
        _ = details.AppendLine(CultureInfo.InvariantCulture, $"IVA precedente detraibile: {input:N2} ({purchases.Count} fatture fornitori)");

        VatReturn result = new()
        {
            Country = italian ? "IT" : "CH",
            PeriodFrom = from.Date,
            PeriodTo = to.Date,
            Turnover = turnover,
            TaxableTurnover = byRate.Values.Sum(static v => v.Taxable),
            OutputVat = output,
            InputVat = input,
            PreviousCredit = previousCredit,
        };

        var balance = output - input - previousCredit;

        if (italian && balance > 0m && (to.Date - from.Date).TotalDays > 40)
        {
            result.Interest = Math.Round(balance * 0.01m, 2, MidpointRounding.AwayFromZero);
            _ = details.AppendLine(CultureInfo.InvariantCulture, $"Interessi trimestrali 1%: {result.Interest:N2}");
        }

        result.Payable = balance + result.Interest;

        if (italian && result.Payable > 0m && result.Payable < ItalianMinimumPayment)
        {
            _ = details.AppendLine("Importo inferiore a 25.82 EUR: non si versa, si riporta al periodo successivo.");
        }

        result.Details = details.ToString().TrimEnd();
        return result;
    }
}
