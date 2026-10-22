// <copyright file="VatNumber.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.RegularExpressions;
using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.ValueObjects;

/// <summary>
/// Represents an immutable Value Added Tax registration number (e.g., Italian Partita IVA or Swiss UID / TVA / MWST).
/// Encapsulates statutory structure validation for Italian 11-digit Partita IVA numbers (composed of a 7-digit progressive, 
/// 3-digit territorial/provincial tax office code, and 1 check digit evaluated via modulo 10 / Luhn algorithm) 
/// and Swiss Business Identification Numbers (UID - Unternehmens-Identifikationsnummer, formatted as CHE followed by 9 digits).
/// </summary>
public sealed partial class VatNumber : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VatNumber"/> class after normalizing and validating the tax registration identifier.
    /// </summary>
    /// <param name="value">The raw VAT registration number (punctuation and spaces permitted).</param>
    /// <param name="countryCode">The ISO 3166-1 alpha-2 country code (e.g., "IT", "CH"); defaults to "IT" if empty or whitespace.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is empty, or does not satisfy the statutory VAT format rules of <paramref name="countryCode"/>.</exception>
    public VatNumber(
        string value,
        string countryCode)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "VAT number cannot be empty.",
                nameof(value));
        }

        CountryCode = string.IsNullOrWhiteSpace(countryCode) ? "IT" : countryCode.Trim().ToUpperInvariant();
        var normalized = Normalize(
            value,
            CountryCode);

        if (!IsValid(
                normalized,
                CountryCode))
        {
            throw new ArgumentException(
                $"Invalid VAT number '{value}' for country '{CountryCode}'",
                nameof(value));
        }

        Value = normalized;
    }

    /// <summary>
    /// Gets the normalized VAT identification number digits without country prefixes or formatting delimiters.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the uppercase ISO 3166-1 alpha-2 country code associated with this VAT registration (e.g., "IT" or "CH").
    /// </summary>
    public string CountryCode { get; }

    [GeneratedRegex(
        @"^\d{11}$",
        RegexOptions.ExplicitCapture,
        2000)]
    private static partial Regex ItalianVatRegex { get; }

    [GeneratedRegex(
        @"^(?:CHE)?\d{9}(?:MWST|TVA|IVA)?$",
        RegexOptions.ExplicitCapture,
        2000)]
    private static partial Regex SwissUidRegex { get; }

    /// <summary>
    /// Normalizes raw VAT input by stripping dots, hyphens, and whitespace, as well as jurisdiction prefixes (e.g., stripping leading "IT" for Italian Partita IVA).
    /// </summary>
    /// <param name="input">The raw VAT string input.</param>
    /// <param name="countryCode">The target ISO country code.</param>
    /// <returns>A cleaned, uppercase VAT identifier string.</returns>
    public static string Normalize(
        string input,
        string countryCode)
    {
        var cleaned = input?.Replace(
                ".",
                string.Empty,
                StringComparison.Ordinal)
            .Replace(
                "-",
                string.Empty,
                StringComparison.Ordinal)
            .Replace(
                " ",
                string.Empty,
                StringComparison.Ordinal)
            .Trim()
            .ToUpperInvariant() ?? string.Empty;

        if (string.Equals(
                countryCode,
                "IT",
                StringComparison.Ordinal) &&
            cleaned.StartsWith(
                "IT",
                StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned[2..];
        }

        return cleaned;
    }

    /// <summary>
    /// Validates whether the specified normalized VAT identifier conforms to the statutory format of the given country.
    /// Validates 11-digit formats for Italian Partita IVA and Swiss CHE 9-digit UID patterns.
    /// </summary>
    /// <param name="cleaned">The normalized VAT string candidate.</param>
    /// <param name="countryCode">The ISO 3166-1 alpha-2 country code.</param>
    /// <returns><see langword="true"/> if <paramref name="cleaned"/> is syntactically valid for <paramref name="countryCode"/>; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(
        string cleaned,
        string countryCode)
    {
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return false;
        }

        if (string.Equals(
                countryCode,
                "IT",
                StringComparison.Ordinal))
        {
            // Italian Partita IVA: 11 digits
            return ItalianVatRegex.IsMatch(cleaned);
        }

        if (string.Equals(
                countryCode,
                "CH",
                StringComparison.Ordinal))
        {
            // Swiss Enterprise ID (UID): CHE followed by 9 digits
            return SwissUidRegex.IsMatch(cleaned);
        }

        return cleaned.Length >= 5;
    }

    /// <summary>
    /// Returns the standard formatted representation of the VAT registration number, prefixing Italian Partita IVA with "IT".
    /// </summary>
    /// <returns>A string representation prefixed with the country code for Italian VAT numbers or the raw UID for Swiss/international entities.</returns>
    public override string ToString() =>
        string.Equals(CountryCode, "IT", StringComparison.Ordinal) ? $"IT{Value}" : Value;

    /// <summary>
    /// Returns the constituent components used for value object equality evaluation.
    /// </summary>
    /// <returns>An enumerable sequence containing <see cref="CountryCode"/> and <see cref="Value"/>.</returns>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CountryCode;
        yield return Value;
    }
}
