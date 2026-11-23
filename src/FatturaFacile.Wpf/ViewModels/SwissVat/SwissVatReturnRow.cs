// <copyright file="SwissVatReturnRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels.SwissVat;

/// <summary>
/// Display row for a periodic Swiss VAT return summary.
/// </summary>
/// <param name="PeriodLabel">Period label.</param>
/// <param name="Method">Accounting method.</param>
/// <param name="Turnover">Box 200 turnover.</param>
/// <param name="OutputTax">Box 399 output tax.</param>
/// <param name="InputTax">Box 479 input tax.</param>
/// <param name="NetBalance">Net payable/credit balance.</param>
/// <param name="Quadratura">Quadratura pass/fail.</param>
internal sealed record SwissVatReturnRow(
    string PeriodLabel,
    string Method,
    string Turnover,
    string OutputTax,
    string InputTax,
    string NetBalance,
    string Quadratura);
