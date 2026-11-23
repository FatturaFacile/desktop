// <copyright file="VatRegisterCsvBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// The machine-readable content of a VAT register: a semicolon-separated file with invariant numbers, also the content whose hash
/// seals a definitive print.
/// </summary>
public static class VatRegisterCsvBuilder
{
    private const string Separator = ";";

    /// <summary>
    /// Builds the CSV of a register.
    /// </summary>
    /// <param name="report">The register.</param>
    /// <returns>The UTF-8 bytes, with a byte order mark.</returns>
    public static byte[] Build(VatRegisterReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        StringBuilder text = new();
        _ = text.AppendLine("section;year;protocol;registration_date;document_date;document_number;partner;partner_tax_id;taxable;vat_rate;vat;deductible_vat;non_deductible_vat;nature");

        foreach (var line in report.Lines)
        {
            _ = text.AppendJoin(
                Separator,
                line.Section,
                line.Year.ToString(CultureInfo.InvariantCulture),
                line.ProtocolNumber.ToString(CultureInfo.InvariantCulture),
                line.RegistrationDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                line.DocumentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Escape(line.DocumentNumber),
                Escape(line.PartnerName ?? line.Description),
                Escape(line.PartnerVatNumber),
                Amount(line.TaxableAmount),
                Amount(line.VatRate),
                Amount(line.VatAmount),
                Amount(line.DeductibleVat),
                Amount(line.NonDeductibleVat),
                Escape(line.VatNature)).AppendLine();
        }

        return [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(text.ToString())];
    }

    /// <summary>
    /// Computes the seal of a register content.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <returns>The SHA-256 as lowercase hexadecimal.</returns>
    public static string Seal(ReadOnlySpan<byte> content) => Convert.ToHexStringLower(SHA256.HashData(content));

    private static string Amount(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);

    private static string Escape(string? value) =>
        string.IsNullOrEmpty(value) ? string.Empty : "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
}
