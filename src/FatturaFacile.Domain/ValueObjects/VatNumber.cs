using System.Text.RegularExpressions;
using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.ValueObjects;

public sealed partial class VatNumber : ValueObject
{
    public string Value { get; }
    public string CountryCode { get; }

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

    public static bool IsValid(string cleaned, string countryCode)
    {
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return false;
        }

        if (string.Equals(countryCode, "IT", StringComparison.Ordinal))
        {
            // Italian Partita IVA: 11 digits
            return ItalianVatRegex().IsMatch(cleaned);
        }

        if (string.Equals(countryCode, "CH", StringComparison.Ordinal))
        {
            // Swiss Enterprise ID (UID): CHE followed by 9 digits
            return SwissUidRegex().IsMatch(cleaned);
        }

        return cleaned.Length >= 5;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CountryCode;
        yield return Value;
    }

    public override string ToString() => string.Equals(CountryCode, "IT", StringComparison.Ordinal) ? $"IT{Value}" : Value;

    [GeneratedRegex(@"^\d{11}$")]
    private static partial Regex ItalianVatRegex();

    [GeneratedRegex(@"^(CHE)?\d{9}(MWST|TVA|IVA)?$")]
    private static partial Regex SwissUidRegex();
}
