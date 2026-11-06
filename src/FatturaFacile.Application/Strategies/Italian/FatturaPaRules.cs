// <copyright file="FatturaPaRules.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Strategies.Italian;

/// <summary>
/// Shared FatturaPA rules (document types, VAT natures, stamp duty, recipient code) based on the
/// Agenzia delle Entrate technical specifications for schema v1.2.x.
/// </summary>
public static class FatturaPaRules
{
    /// <summary>
    /// The stamp duty threshold: exempt / excluded amounts above EUR 77.47 require a EUR 2.00 virtual stamp.
    /// </summary>
    public const decimal StampDutyThreshold = 77.47m;

    /// <summary>
    /// The virtual stamp duty amount.
    /// </summary>
    public const decimal StampDutyAmount = 2.00m;

    private static readonly string[] _stampDutyNaturePrefixes = ["N1", "N2", "N3", "N4"];

    /// <summary>
    /// Gets the valid VAT nature codes.
    /// </summary>
    public static IReadOnlySet<string> ValidNatures { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "N1", "N2.1", "N2.2", "N3.1", "N3.2", "N3.3", "N3.4", "N3.5", "N3.6", "N4", "N5",
        "N6.1", "N6.2", "N6.3", "N6.4", "N6.5", "N6.6", "N6.7", "N6.8", "N6.9", "N7",
    };

    /// <summary>
    /// Maps the internal invoice type to the FatturaPA TipoDocumento code.
    /// </summary>
    /// <param name="invoiceType">The internal invoice type.</param>
    /// <returns>The TipoDocumento code, or null when the type cannot be sent to SDI (e.g. proforma).</returns>
    public static string? GetDocumentType(string? invoiceType)
    {
        return (invoiceType ?? "standard").ToUpperInvariant() switch
        {
            "STANDARD" => "TD01",
            "CREDIT_NOTE" or "CREDITNOTE" => "TD04",
            "DEBIT_NOTE" or "DEBITNOTE" => "TD05",
            "ADVANCE_INVOICE" => "TD02",
            _ => null,
        };
    }

    /// <summary>
    /// Determines whether a nature code is subject to the virtual stamp duty (exempt, excluded, non-taxable operations).
    /// </summary>
    /// <param name="nature">The nature code.</param>
    /// <returns><see langword="true"/> when the stamp duty applies to amounts with that nature.</returns>
    public static bool IsStampDutyNature(string? nature) => nature is { Length: >= 2 } && _stampDutyNaturePrefixes.Contains(nature[..2], StringComparer.Ordinal);

    /// <summary>
    /// Groups the invoice lines by VAT rate and nature and computes the VAT on each group total (as checked by SDI).
    /// </summary>
    /// <param name="items">The invoice lines.</param>
    /// <returns>The VAT summaries ordered by rate.</returns>
    public static IReadOnlyList<FatturaPaVatSummary> ComputeVatSummaries(IEnumerable<InvoiceItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var summaries = items
            .GroupBy(static i => (i.TaxRate, Nature: i.TaxRate == 0m ? i.VatNature : null))
            .Select(static g =>
            {
                var taxable = g.Sum(static i => i.LineTotal);
                var tax = Math.Round(taxable * g.Key.TaxRate / 100m, 2, MidpointRounding.AwayFromZero);
                return new FatturaPaVatSummary(g.Key.TaxRate, g.Key.Nature, taxable, tax);
            })
            .OrderByDescending(static s => s.Rate)
            .ThenBy(static s => s.Nature, StringComparer.Ordinal);

        return [.. summaries];
    }

    /// <summary>
    /// Computes the stamp duty amount due for the invoice lines (EUR 2.00 when exempt amounts exceed EUR 77.47).
    /// </summary>
    /// <param name="items">The invoice lines.</param>
    /// <returns>The stamp duty amount, or zero.</returns>
    public static decimal ComputeStampDuty(IEnumerable<InvoiceItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var exemptTotal = items.Where(static i => i.TaxRate == 0m && IsStampDutyNature(i.VatNature)).Sum(static i => i.LineTotal);
        return exemptTotal > StampDutyThreshold ? StampDutyAmount : 0m;
    }

    /// <summary>
    /// Removes the country prefix and separators from a VAT number.
    /// </summary>
    /// <param name="vatNumber">The VAT number.</param>
    /// <returns>The VAT identifier code.</returns>
    public static string CleanVatNumber(string? vatNumber)
    {
        var value = string.Concat((vatNumber ?? string.Empty).Where(char.IsAsciiLetterOrDigit)).ToUpperInvariant();

        if (value.Length > 2 && char.IsAsciiLetter(value[0]) && char.IsAsciiLetter(value[1]) &&
            !value.StartsWith("CHE", StringComparison.Ordinal))
        {
            value = value[2..];
        }

        return value;
    }

    /// <summary>
    /// Resolves the SDI recipient code: 6 characters for public administrations, 7 for private parties,
    /// "0000000" when the customer only has a PEC or is a consumer, "XXXXXXX" for foreign customers.
    /// </summary>
    /// <param name="contact">The customer.</param>
    /// <returns>The CodiceDestinatario value.</returns>
    public static string ResolveRecipientCode(Contact contact)
    {
        ArgumentNullException.ThrowIfNull(contact);

        if (!string.Equals(contact.Country, "IT", StringComparison.OrdinalIgnoreCase))
        {
            return "XXXXXXX";
        }

        var code = contact.SdiCode?.Trim().ToUpperInvariant();
        var fallback = contact.IsPublicAdministration ? string.Empty : "0000000";
        return string.IsNullOrEmpty(code) ? fallback : code;
    }
}
