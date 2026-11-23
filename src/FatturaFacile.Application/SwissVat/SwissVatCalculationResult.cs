// <copyright file="SwissVatCalculationResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.SwissVat;

/// <summary>
/// Complete result of a Swiss VAT return calculation.
/// </summary>
/// <param name="PeriodFrom">Period start date.</param>
/// <param name="PeriodTo">Period end date.</param>
/// <param name="Method">Accounting method used.</param>
/// <param name="ConsiderationBasis">Consideration basis used.</param>
/// <param name="Boxes">Ordered list of official AFC box entries.</param>
/// <param name="Box200TotalTurnover">Box 200 total turnover.</param>
/// <param name="Box289TotalDeductions">Box 289 total deductions.</param>
/// <param name="Box299TaxableTurnover">Box 299 taxable turnover.</param>
/// <param name="Box399TotalOutputTax">Box 399 total output tax.</param>
/// <param name="Box479TotalInputTax">Box 479 total deductible input tax.</param>
/// <param name="Box500PayableAmount">Box 500 net payable VAT.</param>
/// <param name="Box510CreditAmount">Box 510 net VAT credit.</param>
/// <param name="GrossTurnover">Gross turnover including invoiced VAT.</param>
/// <param name="InvoicedVatTotal">Total VAT invoiced to customers.</param>
/// <param name="ForfeitedInputVat">Input VAT forfeited under Saldosteuersatz.</param>
/// <param name="SssRevenueAdjustment">Gross-to-net revenue adjustment under Saldosteuersatz.</param>
/// <param name="BezugsteuerBase">Bezugsteuer taxable base (Box 382/383).</param>
/// <param name="BezugsteuerTax">Bezugsteuer tax amount (Box 382/383).</param>
public sealed record SwissVatCalculationResult(
    DateOnly PeriodFrom,
    DateOnly PeriodTo,
    SwissVatMethod Method,
    SwissVatConsiderationBasis ConsiderationBasis,
    IReadOnlyList<AfcBoxEntry> Boxes,
    decimal Box200TotalTurnover,
    decimal Box289TotalDeductions,
    decimal Box299TaxableTurnover,
    decimal Box399TotalOutputTax,
    decimal Box479TotalInputTax,
    decimal Box500PayableAmount,
    decimal Box510CreditAmount,
    decimal GrossTurnover,
    decimal InvoicedVatTotal,
    decimal ForfeitedInputVat,
    decimal SssRevenueAdjustment,
    decimal BezugsteuerBase,
    decimal BezugsteuerTax);
