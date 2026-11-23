// <copyright file="SwissVatReturnDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.SwissVat;

/// <summary>
/// Data transfer object for a Swiss VAT periodic return.
/// </summary>
/// <param name="Id">Return identifier.</param>
/// <param name="FiscalYear">Fiscal year.</param>
/// <param name="PeriodType">Period frequency.</param>
/// <param name="PeriodNumber">Period number within the year.</param>
/// <param name="PeriodLabel">Period label (e.g. 2026-Q1).</param>
/// <param name="PeriodFrom">Period start date.</param>
/// <param name="PeriodTo">Period end date.</param>
/// <param name="AccountingMethod">Effective or Saldosteuersatz.</param>
/// <param name="ConsiderationBasis">Agreed or Received.</param>
/// <param name="Status">Lifecycle status.</param>
/// <param name="Box200TotalTurnover">Box 200 total turnover.</param>
/// <param name="Box289TotalDeductions">Box 289 total deductions.</param>
/// <param name="Box299TaxableTurnover">Box 299 taxable turnover.</param>
/// <param name="Box399TotalOutputTax">Box 399 total output tax.</param>
/// <param name="Box479TotalInputTax">Box 479 total deductible input tax.</param>
/// <param name="Box500PayableAmount">Box 500 net payable VAT.</param>
/// <param name="Box510CreditAmount">Box 510 net credit VAT.</param>
/// <param name="GrossTurnover">Gross turnover including VAT.</param>
/// <param name="ForfeitedInputVat">Input VAT forfeited under Saldosteuersatz.</param>
/// <param name="BezugsteuerBase">Bezugsteuer taxable base.</param>
/// <param name="BezugsteuerTax">Bezugsteuer tax amount.</param>
/// <param name="QuadraturaPassed">True when quadratura checks passed.</param>
public sealed record SwissVatReturnDto(
    Guid Id,
    int FiscalYear,
    SwissVatFrequency PeriodType,
    int PeriodNumber,
    string PeriodLabel,
    DateOnly PeriodFrom,
    DateOnly PeriodTo,
    SwissVatMethod AccountingMethod,
    SwissVatConsiderationBasis ConsiderationBasis,
    SwissVatReturnStatus Status,
    decimal Box200TotalTurnover,
    decimal Box289TotalDeductions,
    decimal Box299TaxableTurnover,
    decimal Box399TotalOutputTax,
    decimal Box479TotalInputTax,
    decimal Box500PayableAmount,
    decimal Box510CreditAmount,
    decimal GrossTurnover,
    decimal ForfeitedInputVat,
    decimal BezugsteuerBase,
    decimal BezugsteuerTax,
    bool QuadraturaPassed);
