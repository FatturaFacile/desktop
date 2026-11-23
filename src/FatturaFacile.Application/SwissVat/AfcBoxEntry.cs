// <copyright file="AfcBoxEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SwissVat;

/// <summary>
/// Represents one official AFC / ESTV Form 050/051 box figure.
/// </summary>
/// <param name="Code">Official AFC box code (e.g., 200, 303, 383, 479, 500).</param>
/// <param name="Section">Form section (turnover, deductions, tax_calculation, input_tax, settlement, other_flows).</param>
/// <param name="Amount">Monetary amount in CHF.</param>
/// <param name="BaseAmount">Optional taxable base in CHF for rate boxes.</param>
/// <param name="RatePercent">Optional rate percentage for rate boxes.</param>
/// <param name="TaxAmount">Optional tax amount in CHF for rate boxes.</param>
/// <param name="EpochBoxAlias">Optional historical/current alias box code (e.g., 302 vs 303).</param>
public sealed record AfcBoxEntry(
    string Code,
    string Section,
    decimal Amount,
    decimal? BaseAmount = null,
    decimal? RatePercent = null,
    decimal? TaxAmount = null,
    string? EpochBoxAlias = null);
