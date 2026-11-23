// <copyright file="AnnualVatPeriodRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// One periodic liquidation in the annual VAT reconciliation schedule.
/// </summary>
/// <param name="From">The first day of the period.</param>
/// <param name="To">The last day of the period.</param>
/// <param name="OutputVat">The VAT due on sales.</param>
/// <param name="InputVat">The deductible VAT on purchases.</param>
/// <param name="PreviousCredit">The credit carried in.</param>
/// <param name="Interest">The interest of the quarterly liquidation.</param>
/// <param name="Payable">The amount payable (negative is a credit).</param>
/// <param name="Status">The status of the liquidation: Draft or Final.</param>
public sealed record AnnualVatPeriodRow(DateTime From, DateTime To, decimal OutputVat, decimal InputVat, decimal PreviousCredit, decimal Interest, decimal Payable, string Status);
