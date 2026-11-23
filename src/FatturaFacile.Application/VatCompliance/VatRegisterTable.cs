// <copyright file="VatRegisterTable.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// Lays a VAT register out as a printable table, with the riepilogo per rate and the totals under it.
/// </summary>
public static class VatRegisterTable
{
    /// <summary>
    /// Builds the printable table of a register.
    /// </summary>
    /// <param name="report">The register.</param>
    /// <param name="localize">Resolves a resource key to the text of the current language.</param>
    /// <param name="company">The name of the company printed in the subtitle.</param>
    /// <returns>The document to render.</returns>
    public static TabularPdfDocument Build(VatRegisterReport report, Func<string, string> localize, string company)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(localize);

        string[] headers =
        [
            localize("VatCompliance_Col_Protocol"),
            localize("VatCompliance_Col_Date"),
            localize("VatCompliance_Col_Document"),
            localize("VatCompliance_Col_Partner"),
            localize("VatCompliance_Col_Taxable"),
            localize("VatCompliance_Col_Rate"),
            localize("VatCompliance_Col_Vat"),
            localize("VatCompliance_Col_Deductible"),
            localize("VatCompliance_Col_NonDeductible"),
        ];

        var rows = report.Lines.Select(static l => RowOf(l)).ToList();

        List<string> footer = [];
        footer.AddRange(report.RateTotals.Select(t => string.Format(
            CultureInfo.CurrentCulture,
            localize("VatCompliance_Pdf_RateTotal"),
            t.VatRate.ToString("0.##", CultureInfo.CurrentCulture) + (t.Nature is null ? string.Empty : " " + t.Nature),
            Money(t.Taxable),
            Money(t.Vat))));
        footer.Add(string.Format(CultureInfo.CurrentCulture, localize("VatCompliance_Pdf_Totals"), Money(report.TotalTaxable), Money(report.TotalVat), Money(report.TotalDeductible), Money(report.TotalNonDeductible)));

        return new TabularPdfDocument(
            localize("VatCompliance_Kind_" + report.Kind),
            string.Format(CultureInfo.CurrentCulture, localize("VatCompliance_Pdf_Subtitle"), company, report.From.ToString("d", CultureInfo.CurrentCulture), report.To.ToString("d", CultureInfo.CurrentCulture)),
            headers,
            rows,
            footer);
    }

    private static IReadOnlyList<string> RowOf(VatRegisterEntry l) =>
    [
        l.Section.ToString() + "/" + l.ProtocolNumber.ToString(CultureInfo.InvariantCulture),
        l.RegistrationDate.ToString("d", CultureInfo.CurrentCulture),
        l.DocumentNumber,
        l.PartnerName ?? l.Description ?? string.Empty,
        Money(l.TaxableAmount),
        l.VatRate.ToString("0.##", CultureInfo.CurrentCulture) + (l.VatNature is null ? string.Empty : " " + l.VatNature),
        Money(l.VatAmount),
        Money(l.DeductibleVat),
        Money(l.NonDeductibleVat),
    ];

    private static string Money(decimal value) => value.ToString("N2", CultureInfo.CurrentCulture);
}
