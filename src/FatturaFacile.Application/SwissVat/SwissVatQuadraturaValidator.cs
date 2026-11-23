// <copyright file="SwissVatQuadraturaValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.SwissVat;

/// <summary>
/// Evaluates accounting quadratura checks (Q1..Q9) on a computed Swiss VAT return.
/// </summary>
public static class SwissVatQuadraturaValidator
{
    private const decimal Tolerance = 0.05m;

    /// <summary>
    /// Runs Q1..Q9 checks on <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The calculated return result.</param>
    /// <param name="uidMwst">Configured UID-MWST number.</param>
    /// <param name="pendingDetectedBezugsteuerCount">Number of unreviewed detected Bezugsteuer entries.</param>
    /// <returns>The structured quadratura report.</returns>
    public static QuadraturaReportDto Validate(
        SwissVatCalculationResult result,
        string? uidMwst,
        int pendingDetectedBezugsteuerCount = 0)
    {
        ArgumentNullException.ThrowIfNull(result);

        var map = result.Boxes.ToDictionary(static b => b.Code, static b => b, StringComparer.Ordinal);
        var epoch = SwissVatRateSchedule.ForDate(result.PeriodTo);
        var isSss = result.Method == SwissVatMethod.Saldosteuersatz;
        List<QuadraturaCheckDto> checks = [];

        decimal GetAmount(string code) => map.TryGetValue(code, out var entry) ? entry.Amount : 0m;
        decimal GetBase(string code) => map.TryGetValue(code, out var entry) ? entry.BaseAmount ?? 0m : 0m;
        decimal GetTax(string code) => map.TryGetValue(code, out var entry) ? entry.TaxAmount ?? 0m : 0m;

        // Q1: 200 - 289 == 299
        var expected299 = Math.Round(result.Box200TotalTurnover - result.Box289TotalDeductions, 2, MidpointRounding.AwayFromZero);
        checks.Add(Check("Q1_TURNOVER_BALANCE", "blocker", expected299, result.Box299TaxableTurnover, "SwissVat_Quad_Q1"));

        // Q2: sum(220..280) == 289
        var dedSum = GetAmount("220") + GetAmount("221") + GetAmount("225") + GetAmount("230") + GetAmount("235") + GetAmount("280");
        checks.Add(Check("Q2_DEDUCTIONS_SUM", "blocker", dedSum, result.Box289TotalDeductions, "SwissVat_Quad_Q2"));

        // Q3: Taxable turnover 299 == sum of rate bases
        var rateBaseSum = isSss
            ? GetBase(epoch.SssPrimaryBox) + GetBase(epoch.SssSecondaryBox)
            : GetBase(epoch.StandardBox) + GetBase(epoch.ReducedBox) + GetBase(epoch.AccommodationBox);
        checks.Add(Check("Q3_TAXABLE_BASE_MATCH", "blocker", result.Box299TaxableTurnover, rateBaseSum, "SwissVat_Quad_Q3"));

        // Q4: Sales VAT sum + Bezugsteuer == 399
        var salesTaxSum = isSss
            ? GetTax(epoch.SssPrimaryBox) + GetTax(epoch.SssSecondaryBox)
            : GetTax(epoch.StandardBox) + GetTax(epoch.ReducedBox) + GetTax(epoch.AccommodationBox);
        var expected399 = salesTaxSum + GetTax(epoch.BezugsteuerBox);
        checks.Add(Check("Q4_OUTPUT_TAX_SUM", "blocker", expected399, result.Box399TotalOutputTax, "SwissVat_Quad_Q4"));

        // Q5: 400 + 405 + 410 - 415 - 420 == 479 (0 under SSS)
        var expected479 = isSss
            ? 0m
            : GetAmount("400") + GetAmount("405") + GetAmount("410") - GetAmount("415") - GetAmount("420");
        checks.Add(Check("Q5_INPUT_TAX_BALANCE", "blocker", expected479, result.Box479TotalInputTax, "SwissVat_Quad_Q5"));

        // Q6: 399 - 479 == 500 - 510
        var expectedNet = result.Box399TotalOutputTax - result.Box479TotalInputTax;
        var actualNet = result.Box500PayableAmount - result.Box510CreditAmount;
        checks.Add(Check("Q6_NET_SETTLEMENT_BALANCE", "blocker", expectedNet, actualNet, "SwissVat_Quad_Q6"));

        // Q7: Bezugsteuer consistency
        var expectedBezTax = SwissVatRateSchedule.RoundSwiss(result.BezugsteuerBase * epoch.StandardRate / 100m);
        checks.Add(Check("Q7_BEZUGSTEUER_CONSISTENCY", "blocker", expectedBezTax, result.BezugsteuerTax, "SwissVat_Quad_Q7", 0.10m));

        // Q8: Valid UID-MWST configured
        var hasUid = SwissVatRateSchedule.IsValidUidMwst(uidMwst);
        checks.Add(new QuadraturaCheckDto(
            Code: "Q8_UID_MWST_PRESENT",
            Severity: "warning",
            Passed: hasUid,
            Expected: "CHE-XXX.XXX.XXX MWST",
            Actual: uidMwst ?? string.Empty,
            Difference: "0.00",
            MessageKey: "SwissVat_Quad_Q8"));

        // Q9: Unconfirmed auto-detected Bezugsteuer entries
        checks.Add(new QuadraturaCheckDto(
            Code: "Q9_UNCONFIRMED_BEZUGSTEUER",
            Severity: "warning",
            Passed: pendingDetectedBezugsteuerCount == 0,
            Expected: "0",
            Actual: pendingDetectedBezugsteuerCount.ToString(CultureInfo.InvariantCulture),
            Difference: pendingDetectedBezugsteuerCount.ToString(CultureInfo.InvariantCulture),
            MessageKey: "SwissVat_Quad_Q9"));

        var blockers = checks.Count(static c => !c.Passed && string.Equals(c.Severity, "blocker", StringComparison.Ordinal));
        var warnings = checks.Count(static c => !c.Passed && string.Equals(c.Severity, "warning", StringComparison.Ordinal));

        return new QuadraturaReportDto(blockers == 0, checks, blockers, warnings);
    }

    private static QuadraturaCheckDto Check(
        string code,
        string severity,
        decimal expected,
        decimal actual,
        string messageKey,
        decimal tolerance = Tolerance)
    {
        var diff = Math.Round(Math.Abs(expected - actual), 2, MidpointRounding.AwayFromZero);
        return new QuadraturaCheckDto(
            Code: code,
            Severity: severity,
            Passed: diff <= tolerance,
            Expected: expected.ToString("0.00", CultureInfo.InvariantCulture),
            Actual: actual.ToString("0.00", CultureInfo.InvariantCulture),
            Difference: diff.ToString("0.00", CultureInfo.InvariantCulture),
            MessageKey: messageKey);
    }
}
