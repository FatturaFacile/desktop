// <copyright file="VatRegisterReport.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// A VAT register over a period: its lines in protocol order, the riepilogo per rate, the totals and the gaps in the numbering.
/// </summary>
/// <param name="Kind">The register.</param>
/// <param name="Section">The sezionale, or null for all of them.</param>
/// <param name="From">The first day.</param>
/// <param name="To">The last day.</param>
/// <param name="Lines">The lines in protocol order.</param>
/// <param name="RateTotals">The totals per rate.</param>
/// <param name="TotalTaxable">The taxable total.</param>
/// <param name="TotalVat">The VAT total.</param>
/// <param name="TotalDeductible">The deductible VAT total.</param>
/// <param name="TotalNonDeductible">The non-deductible VAT total.</param>
/// <param name="Gaps">The missing protocol numbers of the series shown.</param>
/// <param name="FinalizedLines">How many lines are locked by a definitive print.</param>
public sealed record VatRegisterReport(
    VatRegisterKind Kind,
    VatRegisterSection? Section,
    DateTime From,
    DateTime To,
    IReadOnlyList<VatRegisterEntry> Lines,
    IReadOnlyList<VatRegisterRateTotal> RateTotals,
    decimal TotalTaxable,
    decimal TotalVat,
    decimal TotalDeductible,
    decimal TotalNonDeductible,
    IReadOnlyList<int> Gaps,
    int FinalizedLines);
