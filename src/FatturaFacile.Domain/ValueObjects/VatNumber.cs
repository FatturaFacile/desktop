using System.Text.RegularExpressions;
using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.ValueObjects;

/// <summary>
/// Represents a Value Added Tax (VAT / Partita IVA / UID) registration number.
/// </summary>
public sealed partial class VatNumber : ValueObject
{
    /// <summary>
    /// Gets the normalized VAT identification number digits.
    /// </summary>
    public string Value { get; }

/// <summary>
    /// Gets the ISO two-letter country code associated with this VAT number.
    /// </summary>
    public string CountryCode { get; }

/// <summary>
    /// Initializes a new instance of the <see cref="VatNumber"/> class.
    /// </summary>
    /// <param name="value">The raw VAT registration number.</param>
    /// <param name="countryCode">The ISO country code (e.g. IT, CH).</param>
    /// <exception cref="ArgumentException">Thrown when the value or format is invalid.</exception>
    public VatNumber(string value, string countryCode)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("VAT number cannot be empty.", nameof(value));
        }

        CountryCode = string.IsNullOrWhiteSpace(countryCode) ? "IT" : countryCode.Trim().ToUpperInvariant();
        var normalized = Normalize(value, CountryCode);

        if (!IsValid(normalized, CountryCode))
        {
            throw new ArgumentException($"Invalid VAT number '{value}' for country '{CountryCode}'", nameof(value));
        }

        Value = normalized;
    }

/// <summary>
    /// Normalizes raw VAT input by stripping formatting symbols and country prefixes.
    /// </summary>
    /// <param name="input">The raw string input.</param>
    /// <param name="countryCode">The target jurisdiction country code.</param>
    /// <returns>A cleaned, uppercase VAT identifier string.</returns>
    public static string Normalize(string input, string countryCode)
    {
        var cleaned = input.Replace(".", string.Empty, StringComparison.Ordinal)
                           .Replace("-", string.Empty, StringComparison.Ordinal)
                           .Replace(" ", string.Empty, StringComparison.Ordinal)
                           .Trim()
                           .ToUpperInvariant();

        if (string.Equals(countryCode, "IT", StringComparison.Ordinal) && cleaned.StartsWith("IT", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned[2..];
        }

        return cleaned;
    }

/// <summary>
    /// Validates whether the given cleaned VAT identifier conforms to country rules.
    /// </summary>
    /// <param name="cleaned">The normalized VAT string.</param>
    /// <param name="countryCode">The ISO country code.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(string cleaned, string countryCode)
    {
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return false;
        }

        if (string.Equals(countryCode, "IT", StringComparison.Ordinal))
        {
            // Italian Partita IVA: 11 digits
            return ItalianVatRegex.IsMatch(cleaned);
        }

        if (string.Equals(countryCode, "CH", StringComparison.Ordinal))
        {
            // Swiss Enterprise ID (UID): CHE followed by 9 digits
            return SwissUidRegex.IsMatch(cleaned);
        }

        return cleaned.Length >= 5;
    }

/// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CountryCode;
        yield return Value;
    }

/// <inheritdoc/>
    public override string ToString() => string.Equals(CountryCode, "IT", StringComparison.Ordinal) ? $"IT{Value}" : Value;

[GeneratedRegex(@"^\d{11}$", RegexOptions.ExplicitCapture, 2000)]
    private static partial Regex ItalianVatRegex { get; }

[GeneratedRegex(@"^(?:CHE)?\d{9}(?:MWST|TVA|IVA)?$", RegexOptions.ExplicitCapture, 2000)]
    private static partial Regex SwissUidRegex { get; }
}
