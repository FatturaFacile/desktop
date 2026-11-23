// <copyright file="AnnualVatReconciliation.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// The schedule that prepares the annual VAT return: the periodic liquidations of the year against the VAT registers (quadri VE
/// and VF) and the Erario c/IVA account (quadro VL).
/// </summary>
/// <param name="Year">The year.</param>
/// <param name="Rows">The periodic liquidations.</param>
/// <param name="TotalOutputVat">The VAT due on sales over the year.</param>
/// <param name="TotalInputVat">The deductible VAT on purchases over the year.</param>
/// <param name="TotalInterest">The interest of the quarterly liquidations.</param>
/// <param name="OpeningCredit">The credit carried into the first period.</param>
/// <param name="AnnualBalance">The balance of the year: positive is a debit, negative a credit.</param>
/// <param name="SalesTaxable">The taxable amount of the sales register (quadro VE).</param>
/// <param name="SalesVat">The VAT of the sales register.</param>
/// <param name="PurchasesTaxable">The taxable amount of the purchases register (quadro VF).</param>
/// <param name="PurchasesDeductibleVat">The deductible VAT of the purchases register.</param>
/// <param name="LedgerPosted">The net credit (credits minus debits) posted to the Erario c/IVA account in the year.</param>
/// <param name="Findings">The localization key suffixes of the differences found.</param>
public sealed record AnnualVatReconciliation(
    int Year,
    IReadOnlyList<AnnualVatPeriodRow> Rows,
    decimal TotalOutputVat,
    decimal TotalInputVat,
    decimal TotalInterest,
    decimal OpeningCredit,
    decimal AnnualBalance,
    decimal SalesTaxable,
    decimal SalesVat,
    decimal PurchasesTaxable,
    decimal PurchasesDeductibleVat,
    decimal LedgerPosted,
    IReadOnlyList<string> Findings);
