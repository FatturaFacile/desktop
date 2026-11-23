// <copyright file="SwissVatCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.SwissVat;

/// <summary>
/// Pure deterministic Swiss VAT calculation engine supporting Effective Method (Form 050)
/// and Saldosteuersatz Method (Form 051), across both 2024+ (8.1/2.6/3.8%) and 2018-2023 (7.7/2.5/3.7%) epochs.
/// </summary>
public static class SwissVatCalculator
{
    /// <summary>
    /// Computes all official AFC boxes and summary figures for a Swiss VAT period.
    /// </summary>
    /// <param name="input">The calculation input lines and settings.</param>
    /// <returns>The computed Swiss VAT return breakdown.</returns>
    public static SwissVatCalculationResult Calculate(SwissVatCalculationInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var epoch = SwissVatRateSchedule.ForDate(input.PeriodTo);
        var isSss = input.Method == SwissVatMethod.Saldosteuersatz;
        var primarySss = input.SssRatePrimary is > 0m ? input.SssRatePrimary.Value : 6.20m;

        var box220 = 0m;
        var box221 = 0m;
        var box225 = 0m;
        var box230 = 0m;
        var box235 = 0m;
        var box280 = 0m;
        var box900 = 0m;
        var box910 = 0m;
        var netTurnover = 0m;
        var invoicedVat = 0m;

        var stdBase = 0m;
        var stdTax = 0m;
        var redBase = 0m;
        var redTax = 0m;
        var accBase = 0m;
        var accTax = 0m;

        Dictionary<decimal, (decimal Base, decimal Tax)> sssBuckets = [];

        foreach (var line in input.SalesLines)
        {
            var sign = line.IsCreditNote ? -1m : 1m;
            var net = sign * line.NetAmount;
            var tax = line.TaxAmount.HasValue
                ? sign * line.TaxAmount.Value
                : Math.Round(net * line.TaxRate / 100m, 2, MidpointRounding.AwayFromZero);

            if (line.ExemptionType == SwissVatExemptionType.OutOfScope
                || string.Equals(line.AfcBox, "900", StringComparison.Ordinal)
                || string.Equals(line.AfcBox, "910", StringComparison.Ordinal))
            {
                if (string.Equals(line.AfcBox, "910", StringComparison.Ordinal))
                {
                    box910 += net;
                }
                else
                {
                    box900 += net;
                }

                continue;
            }

            netTurnover += net;

            if (line.TaxRate <= 0m && tax == 0m)
            {
                var target = line.AfcBox;
                if (string.IsNullOrWhiteSpace(target))
                {
                    target = line.ExemptionType == SwissVatExemptionType.ExemptWithoutDeduction ? "230" : "220";
                }

                switch (target)
                {
                    case "221":
                        box221 += net;
                        break;
                    case "225":
                        box225 += net;
                        break;
                    case "230":
                        box230 += net;
                        break;
                    case "235":
                        box235 += net;
                        break;
                    case "280":
                        box280 += net;
                        break;
                    default:
                        box220 += net;
                        break;
                }

                continue;
            }

            invoicedVat += tax;

            if (isSss)
            {
                var grossLine = net + tax;
                var sssRate = line.SssRate is > 0m ? line.SssRate.Value : primarySss;
                var sssTax = grossLine * sssRate / 100m;

                if (sssBuckets.TryGetValue(sssRate, out var existing))
                {
                    sssBuckets[sssRate] = (existing.Base + grossLine, existing.Tax + sssTax);
                }
                else
                {
                    sssBuckets[sssRate] = (grossLine, sssTax);
                }
            }
            else if (line.TaxRate is >= 2.40m and <= 2.60m || string.Equals(line.AfcBox, epoch.ReducedBox, StringComparison.Ordinal))
            {
                redBase += net;
                redTax += tax;
            }
            else if (line.TaxRate is >= 3.60m and <= 3.90m || string.Equals(line.AfcBox, epoch.AccommodationBox, StringComparison.Ordinal))
            {
                accBase += net;
                accTax += tax;
            }
            else
            {
                stdBase += net;
                stdTax += tax;
            }
        }

        var box289 = Math.Round(box220 + box221 + box225 + box230 + box235 + box280, 2, MidpointRounding.AwayFromZero);
        var grossTurnover = Math.Round(netTurnover + invoicedVat, 2, MidpointRounding.AwayFromZero);
        var box200 = isSss ? grossTurnover : Math.Round(netTurnover, 2, MidpointRounding.AwayFromZero);
        var box299 = Math.Round(box200 - box289, 2, MidpointRounding.AwayFromZero);

        List<AfcBoxEntry> boxes =
        [
            new("200", "turnover", box200),
            new("220", "deductions", Math.Round(box220, 2, MidpointRounding.AwayFromZero)),
            new("221", "deductions", Math.Round(box221, 2, MidpointRounding.AwayFromZero)),
            new("225", "deductions", Math.Round(box225, 2, MidpointRounding.AwayFromZero)),
            new("230", "deductions", Math.Round(box230, 2, MidpointRounding.AwayFromZero)),
            new("235", "deductions", Math.Round(box235, 2, MidpointRounding.AwayFromZero)),
            new("280", "deductions", Math.Round(box280, 2, MidpointRounding.AwayFromZero)),
            new("289", "deductions", box289),
            new("299", "turnover", box299),
        ];

        var salesOutputTax = 0m;
        if (isSss)
        {
            var ordered = sssBuckets
                .OrderByDescending(kvp => kvp.Key == primarySss ? decimal.MaxValue : kvp.Key)
                .ToList();

            for (var i = 0; i < ordered.Count; i++)
            {
                var (rate, val) = ordered[i];
                var code = i == 0 ? epoch.SssPrimaryBox : epoch.SssSecondaryBox;
                var alias = string.Equals(code, "323", StringComparison.Ordinal) ? "322" : "332";
                var taxRounded = Math.Round(val.Tax, 2, MidpointRounding.AwayFromZero);
                var baseRounded = Math.Round(val.Base, 2, MidpointRounding.AwayFromZero);
                salesOutputTax += taxRounded;
                boxes.Add(new AfcBoxEntry(code, "tax_calculation", taxRounded, baseRounded, rate, taxRounded, alias));
            }

            if (ordered.Count == 0)
            {
                boxes.Add(new AfcBoxEntry(epoch.SssPrimaryBox, "tax_calculation", 0m, 0m, primarySss, 0m, "322"));
            }
        }
        else
        {
            stdBase = Math.Round(stdBase, 2, MidpointRounding.AwayFromZero);
            stdTax = Math.Round(stdTax, 2, MidpointRounding.AwayFromZero);
            redBase = Math.Round(redBase, 2, MidpointRounding.AwayFromZero);
            redTax = Math.Round(redTax, 2, MidpointRounding.AwayFromZero);
            accBase = Math.Round(accBase, 2, MidpointRounding.AwayFromZero);
            accTax = Math.Round(accTax, 2, MidpointRounding.AwayFromZero);
            salesOutputTax = stdTax + redTax + accTax;

            boxes.Add(new AfcBoxEntry(epoch.StandardBox, "tax_calculation", stdTax, stdBase, epoch.StandardRate, stdTax, string.Equals(epoch.StandardBox, "303", StringComparison.Ordinal) ? "302" : "303"));
            boxes.Add(new AfcBoxEntry(epoch.ReducedBox, "tax_calculation", redTax, redBase, epoch.ReducedRate, redTax, string.Equals(epoch.ReducedBox, "313", StringComparison.Ordinal) ? "312" : "313"));
            boxes.Add(new AfcBoxEntry(epoch.AccommodationBox, "tax_calculation", accTax, accBase, epoch.AccommodationRate, accTax, string.Equals(epoch.AccommodationBox, "343", StringComparison.Ordinal) ? "342" : "343"));
        }

        var bezBase = 0m;
        var bezTax = 0m;
        var bezDeductible = 0m;
        foreach (var bez in input.BezugsteuerEntries)
        {
            if (bez.Status is BezugsteuerStatus.Excluded or BezugsteuerStatus.Detected)
            {
                continue;
            }

            bezBase += bez.NetAmountChf;
            bezTax += bez.TaxAmountChf;
            if (!isSss)
            {
                bezDeductible += bez.DeductibleInputTaxChf;
            }
        }

        bezBase = Math.Round(bezBase, 2, MidpointRounding.AwayFromZero);
        bezTax = Math.Round(bezTax, 2, MidpointRounding.AwayFromZero);
        boxes.Add(new AfcBoxEntry(epoch.BezugsteuerBox, "tax_calculation", bezTax, bezBase, epoch.StandardRate, bezTax, string.Equals(epoch.BezugsteuerBox, "383", StringComparison.Ordinal) ? "382" : "383"));

        var box399 = Math.Round(salesOutputTax + bezTax, 2, MidpointRounding.AwayFromZero);
        boxes.Add(new AfcBoxEntry("399", "tax_calculation", box399));

        var box400 = 0m;
        var box405 = 0m;
        var forfeitedInput = 0m;

        foreach (var purchase in input.PurchaseLines)
        {
            if (!purchase.IsDeductible)
            {
                continue;
            }

            if (isSss)
            {
                forfeitedInput += purchase.InputVatAmount;
                continue;
            }

            if (string.Equals(purchase.AfcBox, "405", StringComparison.Ordinal))
            {
                box405 += purchase.InputVatAmount;
            }
            else
            {
                box400 += purchase.InputVatAmount;
            }
        }

        if (!isSss)
        {
            box400 += bezDeductible;
        }

        box400 = Math.Round(box400, 2, MidpointRounding.AwayFromZero);
        box405 = Math.Round(box405, 2, MidpointRounding.AwayFromZero);
        var box410 = isSss ? 0m : Math.Round(input.AdjustmentsBox410, 2, MidpointRounding.AwayFromZero);
        var box415 = isSss ? 0m : Math.Round(input.AdjustmentsBox415, 2, MidpointRounding.AwayFromZero);
        var box420 = isSss ? 0m : Math.Round(input.AdjustmentsBox420, 2, MidpointRounding.AwayFromZero);
        var box479 = isSss ? 0m : Math.Round(box400 + box405 + box410 - box415 - box420, 2, MidpointRounding.AwayFromZero);

        boxes.Add(new AfcBoxEntry("400", "input_tax", box400));
        boxes.Add(new AfcBoxEntry("405", "input_tax", box405));
        boxes.Add(new AfcBoxEntry("410", "input_tax", box410));
        boxes.Add(new AfcBoxEntry("415", "input_tax", box415));
        boxes.Add(new AfcBoxEntry("420", "input_tax", box420));
        boxes.Add(new AfcBoxEntry("479", "input_tax", box479));

        var balance = SwissVatRateSchedule.RoundSwiss(box399 - box479, input.UseCommercial005Rounding);
        var box500 = balance > 0m ? balance : 0m;
        var box510 = balance < 0m ? Math.Abs(balance) : 0m;

        boxes.Add(new AfcBoxEntry("500", "settlement", box500));
        boxes.Add(new AfcBoxEntry("510", "settlement", box510));
        boxes.Add(new AfcBoxEntry("900", "other_flows", Math.Round(box900, 2, MidpointRounding.AwayFromZero)));
        boxes.Add(new AfcBoxEntry("910", "other_flows", Math.Round(box910, 2, MidpointRounding.AwayFromZero)));

        var sssAdjustment = isSss ? Math.Round(invoicedVat - salesOutputTax, 2, MidpointRounding.AwayFromZero) : 0m;

        return new SwissVatCalculationResult(
            PeriodFrom: input.PeriodFrom,
            PeriodTo: input.PeriodTo,
            Method: input.Method,
            ConsiderationBasis: input.ConsiderationBasis,
            Boxes: boxes,
            Box200TotalTurnover: box200,
            Box289TotalDeductions: box289,
            Box299TaxableTurnover: box299,
            Box399TotalOutputTax: box399,
            Box479TotalInputTax: box479,
            Box500PayableAmount: box500,
            Box510CreditAmount: box510,
            GrossTurnover: grossTurnover,
            InvoicedVatTotal: Math.Round(invoicedVat, 2, MidpointRounding.AwayFromZero),
            ForfeitedInputVat: Math.Round(forfeitedInput, 2, MidpointRounding.AwayFromZero),
            SssRevenueAdjustment: sssAdjustment,
            BezugsteuerBase: bezBase,
            BezugsteuerTax: bezTax);
    }
}
