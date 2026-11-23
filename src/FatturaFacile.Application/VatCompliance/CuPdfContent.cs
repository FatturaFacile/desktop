// <copyright file="CuPdfContent.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// Lays the Certificazioni Uniche out as a printable document.
/// </summary>
public static class CuPdfContent
{
    /// <summary>
    /// Builds the document of the certificates of a year, one row per certificate.
    /// </summary>
    /// <param name="year">The year of the payments.</param>
    /// <param name="company">The name of the withholding agent.</param>
    /// <param name="certificates">The certificates.</param>
    /// <param name="localize">Resolves a resource key to the text of the current language.</param>
    /// <returns>The document to render.</returns>
    public static TabularPdfDocument Build(int year, string company, IReadOnlyList<WithholdingCertificate> certificates, Func<string, string> localize)
    {
        ArgumentNullException.ThrowIfNull(certificates);
        ArgumentNullException.ThrowIfNull(localize);

        string[] headers =
        [
            localize("VatCompliance_Cu_Recipient"),
            localize("VatCompliance_Cu_TaxCode"),
            localize("VatCompliance_Cu_Reason"),
            localize("VatCompliance_Cu_Gross"),
            localize("VatCompliance_Cu_Taxable"),
            localize("VatCompliance_Cu_Withheld"),
            localize("VatCompliance_Cu_WithheldPaid"),
        ];

        var rows = certificates.Select(static c => RowOf(c)).ToList();

        return new TabularPdfDocument(
            string.Format(CultureInfo.CurrentCulture, localize("VatCompliance_Cu_PdfTitle"), year),
            company,
            headers,
            rows,
            [
                string.Format(CultureInfo.CurrentCulture, localize("VatCompliance_Cu_PdfTotals"), Money(certificates.Sum(static c => c.WithheldAmount)), Money(certificates.Sum(static c => c.WithheldPaidAmount))),
                localize("VatCompliance_Cu_PdfNotice"),
            ]);
    }

    private static IReadOnlyList<string> RowOf(WithholdingCertificate c) =>
    [
        c.RecipientName,
        c.RecipientTaxCode,
        c.Reason,
        Money(c.GrossPaid),
        Money(c.TaxableAmount),
        Money(c.WithheldAmount),
        Money(c.WithheldPaidAmount),
    ];

    private static string Money(decimal value) => value.ToString("N2", CultureInfo.CurrentCulture);
}
