// <copyright file="BezugsteuerDetector.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.SwissVat;

/// <summary>
/// Detects cross-border foreign supplier acquisitions subject to Swiss Bezugsteuer (Art. 45 LIVA -> Box 382/383).
/// </summary>
public static class BezugsteuerDetector
{
    private static readonly string[] s_softwareKeywords =
        ["software", "saas", "cloud", "aws", "azure", "gcp", "github", "license", "hosting", "subscription"];

    private static readonly string[] s_consultingKeywords =
        ["consulting", "consulenza", "beratung", "conseil", "advisory", "legal", "audit", "treuhand"];

    /// <summary>
    /// Determines whether a supplier invoice originates outside the CH/LI domestic VAT territory without Swiss MWST charged.
    /// </summary>
    /// <param name="supplierCountry">ISO 3166-1 alpha-2 country code.</param>
    /// <param name="chargedSwissVatAmount">Domestic Swiss VAT already charged on the invoice.</param>
    /// <returns>True when Bezugsteuer applies.</returns>
    public static bool IsSubjectToBezugsteuer(string? supplierCountry, decimal chargedSwissVatAmount)
    {
        if (string.IsNullOrWhiteSpace(supplierCountry) || chargedSwissVatAmount > 0m)
        {
            return false;
        }

        var normalized = supplierCountry.Trim().ToUpperInvariant();
        return !string.Equals(normalized, "CH", StringComparison.Ordinal)
            && !string.Equals(normalized, "LI", StringComparison.Ordinal);
    }

    /// <summary>
    /// Infers the Bezugsteuer service category from supplier/description keywords.
    /// </summary>
    /// <param name="description">Invoice or service description.</param>
    /// <returns>The inferred <see cref="BezugsteuerCategory"/>.</returns>
    public static BezugsteuerCategory InferCategory(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return BezugsteuerCategory.OtherCrossBorderService;
        }

        var lower = description.ToLowerInvariant();
        foreach (var kw in s_softwareKeywords)
        {
            if (lower.Contains(kw, StringComparison.Ordinal))
            {
                return BezugsteuerCategory.SoftwareCloud;
            }
        }

        foreach (var kw in s_consultingKeywords)
        {
            if (lower.Contains(kw, StringComparison.Ordinal))
            {
                return BezugsteuerCategory.Consulting;
            }
        }

        return BezugsteuerCategory.OtherCrossBorderService;
    }

    /// <summary>
    /// Evaluates a cross-border supplier invoice and builds a detected <see cref="BezugsteuerEntryDto"/> when applicable.
    /// </summary>
    /// <param name="supplierName">Supplier name.</param>
    /// <param name="supplierCountry">Supplier ISO country code.</param>
    /// <param name="invoiceNumber">Invoice number.</param>
    /// <param name="invoiceDate">Invoice date.</param>
    /// <param name="description">Service description.</param>
    /// <param name="netAmountChf">Net amount in CHF.</param>
    /// <param name="chargedSwissVatAmount">Domestic Swiss VAT charged (0 for cross-border reverse charge).</param>
    /// <param name="method">Company accounting method.</param>
    /// <returns>A detected entry DTO, or null when not subject to Bezugsteuer.</returns>
    public static BezugsteuerEntryDto? Evaluate(
        string supplierName,
        string supplierCountry,
        string invoiceNumber,
        DateOnly invoiceDate,
        string description,
        decimal netAmountChf,
        decimal chargedSwissVatAmount,
        SwissVatMethod method)
    {
        if (!IsSubjectToBezugsteuer(supplierCountry, chargedSwissVatAmount) || netAmountChf <= 0m)
        {
            return null;
        }

        var epoch = SwissVatRateSchedule.ForDate(invoiceDate);
        var rate = epoch.StandardRate;
        var tax = SwissVatRateSchedule.RoundSwiss(netAmountChf * rate / 100m);
        var deductible = method == SwissVatMethod.Effective ? tax : 0m;

        return new BezugsteuerEntryDto(
            Id: Guid.NewGuid(),
            SupplierName: supplierName,
            SupplierCountry: supplierCountry.Trim().ToUpperInvariant(),
            InvoiceNumber: invoiceNumber,
            InvoiceDate: invoiceDate,
            ServiceCategory: InferCategory(description),
            Description: description,
            NetAmountChf: Math.Round(netAmountChf, 2, MidpointRounding.AwayFromZero),
            VatRatePercent: rate,
            TaxAmountChf: tax,
            DeductibleInputTaxChf: deductible,
            AfcBox: epoch.BezugsteuerBox,
            Status: BezugsteuerStatus.Detected);
    }
}
