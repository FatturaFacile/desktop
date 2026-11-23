// <copyright file="CuTelematicFileBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// Builds the working file of the Certificazioni Uniche for the telematic channel: a header record, one record per certificate and
/// a trailer. It carries the figures the official fixed-width supply needs but is not that supply: it must be loaded in the official
/// control software of the Agenzia delle Entrate before it is sent.
/// </summary>
public static class CuTelematicFileBuilder
{
    private const char Separator = '|';
    private const string LineEnd = "\r\n";

    /// <summary>
    /// Gets the name of the file.
    /// </summary>
    /// <param name="taxCode">The tax code of the withholding agent.</param>
    /// <param name="year">The year of the payments.</param>
    /// <returns>The file name.</returns>
    public static string FileName(string taxCode, int year) => string.Create(CultureInfo.InvariantCulture, $"CU_{taxCode}_{year}.txt");

    /// <summary>
    /// Builds the file.
    /// </summary>
    /// <param name="taxCode">The tax code of the withholding agent.</param>
    /// <param name="year">The year of the payments.</param>
    /// <param name="certificates">The certificates.</param>
    /// <returns>The UTF-8 bytes, without a byte order mark.</returns>
    public static byte[] Build(string taxCode, int year, IReadOnlyList<WithholdingCertificate> certificates)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taxCode);
        ArgumentNullException.ThrowIfNull(certificates);

        var count = certificates.Count.ToString(CultureInfo.InvariantCulture);
        List<string> records = [string.Join(Separator, "A", "CU", (year + 1).ToString(CultureInfo.InvariantCulture), taxCode, count)];
        records.AddRange(certificates.Select(static c => RecordOf(c)));
        records.Add(string.Join(Separator, "Z", count));

        return Encoding.UTF8.GetBytes(string.Concat(records.Select(static r => r + LineEnd)));
    }

    private static string RecordOf(WithholdingCertificate certificate) => string.Join(
        Separator,
        "B",
        certificate.Year.ToString(CultureInfo.InvariantCulture),
        Clean(certificate.RecipientTaxCode),
        Clean(certificate.RecipientName),
        Clean(certificate.Reason),
        Amount(certificate.GrossPaid),
        Amount(certificate.TaxableAmount),
        Amount(certificate.WithheldAmount),
        Amount(certificate.WithheldPaidAmount));

    private static string Amount(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);

    private static string Clean(string value) => value.Replace(Separator, ' ').Trim().ToUpperInvariant();
}
