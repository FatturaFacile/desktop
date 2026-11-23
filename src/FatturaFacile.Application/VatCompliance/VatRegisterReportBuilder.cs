// <copyright file="VatRegisterReportBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// Builds the printable report of a VAT register from its lines.
/// </summary>
public static class VatRegisterReportBuilder
{
    /// <summary>
    /// Builds the report of the lines of one register in a period.
    /// </summary>
    /// <param name="kind">The register.</param>
    /// <param name="section">The sezionale, or null for all of them.</param>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <param name="entries">The lines of the register, already filtered by period and section.</param>
    /// <returns>The report.</returns>
    public static VatRegisterReport Build(VatRegisterKind kind, VatRegisterSection? section, DateTime from, DateTime to, IEnumerable<VatRegisterEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var lines = entries
            .OrderBy(static e => e.Section)
            .ThenBy(static e => e.Year)
            .ThenBy(static e => e.ProtocolNumber)
            .ThenBy(static e => e.VatRate)
            .ToList();

        var rateTotals = lines
            .GroupBy(static l => (l.VatRate, Nature: l.VatRate == 0m ? l.VatNature : null))
            .OrderByDescending(static g => g.Key.VatRate)
            .ThenBy(static g => g.Key.Nature, StringComparer.Ordinal)
            .Select(static g => new VatRegisterRateTotal(g.Key.VatRate, g.Key.Nature, g.Sum(static l => l.TaxableAmount), g.Sum(static l => l.VatAmount), g.Sum(static l => l.DeductibleVat), g.Sum(static l => l.NonDeductibleVat)))
            .ToList();

        var gaps = lines
            .GroupBy(static l => (l.Section, l.Year))
            .SelectMany(static g => VatRegisterNumbering.FindGaps(g.Select(static l => l.ProtocolNumber)))
            .ToList();

        return new VatRegisterReport(
            kind,
            section,
            from,
            to,
            lines,
            rateTotals,
            lines.Sum(static l => l.TaxableAmount),
            lines.Sum(static l => l.VatAmount),
            lines.Sum(static l => l.DeductibleVat),
            lines.Sum(static l => l.NonDeductibleVat),
            gaps,
            lines.Count(static l => l.IsFinalized));
    }
}
