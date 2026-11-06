// <copyright file="IbanValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Validation;

/// <summary>
/// ISO 13616 IBAN validation (MOD 97-10) including Swiss QR-IBAN detection (QR-IID 30000-31999).
/// </summary>
public static class IbanValidator
{
    private const int SwissIbanLength = 21;
    private const int ItalianIbanLength = 27;
    private const int QrIidMin = 30000;
    private const int QrIidMax = 31999;

    /// <summary>
    /// Removes blanks and converts the IBAN to upper case.
    /// </summary>
    /// <param name="iban">The raw IBAN.</param>
    /// <returns>The electronic (compact) IBAN format.</returns>
    public static string Normalize(string? iban) => (iban ?? string.Empty).Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();

    /// <summary>
    /// Validates the IBAN structure and its MOD 97-10 check digits.
    /// </summary>
    /// <param name="iban">The IBAN to validate.</param>
    /// <returns><see langword="true"/> when the IBAN is formally valid.</returns>
    public static bool IsValid(string? iban)
    {
        var value = Normalize(iban);

        if (value.Length is < 15 or > 34)
        {
            return false;
        }

        if (!char.IsAsciiLetterUpper(value[0]) || !char.IsAsciiLetterUpper(value[1]) ||
            !char.IsAsciiDigit(value[2]) || !char.IsAsciiDigit(value[3]))
        {
            return false;
        }

        foreach (var c in value)
        {
            if (!char.IsAsciiLetterOrDigit(c))
            {
                return false;
            }
        }

        var expectedLength = value[..2] switch
        {
            "CH" or "LI" => SwissIbanLength,
            "IT" => ItalianIbanLength,
            _ => value.Length,
        };

        return value.Length == expectedLength &&
               ComputeMod97(string.Concat(value.AsSpan(4), value.AsSpan(0, 4))) == 1;
    }

    /// <summary>
    /// Determines whether the IBAN is a valid Swiss / Liechtenstein QR-IBAN (institution id 30000-31999).
    /// </summary>
    /// <param name="iban">The IBAN to inspect.</param>
    /// <returns><see langword="true"/> for a valid QR-IBAN.</returns>
    public static bool IsSwissQrIban(string? iban)
    {
        var value = Normalize(iban);

        return IsSwissOrLiechtensteinIban(value) &&
               int.TryParse(
                   value.AsSpan(4, 5),
                   System.Globalization.NumberStyles.None,
                   System.Globalization.CultureInfo.InvariantCulture,
                   out var iid) &&
               iid is >= QrIidMin and <= QrIidMax;
    }

    /// <summary>
    /// Determines whether the IBAN is a valid Swiss or Liechtenstein IBAN (the only accounts allowed on a QR-bill).
    /// </summary>
    /// <param name="iban">The IBAN to inspect.</param>
    /// <returns><see langword="true"/> when valid and issued in CH or LI.</returns>
    public static bool IsSwissOrLiechtensteinIban(string? iban)
    {
        var value = Normalize(iban);
        return IsValid(value) &&
               (value.StartsWith("CH", StringComparison.Ordinal) || value.StartsWith("LI", StringComparison.Ordinal));
    }

    /// <summary>
    /// Computes the ISO 7064 MOD 97-10 remainder of an alphanumeric string (letters A-Z map to 10-35).
    /// </summary>
    /// <param name="value">The alphanumeric value.</param>
    /// <returns>The remainder modulo 97.</returns>
    /// <exception cref="ArgumentException">Thrown when the value contains characters other than 0-9 and A-Z.</exception>
    public static int ComputeMod97(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var remainder = 0;

        foreach (var c in value)
        {
            if (char.IsAsciiDigit(c))
            {
                remainder = ((remainder * 10) + (c - '0')) % 97;
            }
            else if (char.IsAsciiLetterUpper(c))
            {
                var number = c - 'A' + 10;
                remainder = ((remainder * 100) + number) % 97;
            }
            else
            {
                throw new ArgumentException($"Invalid character '{c}' in MOD 97 input.", nameof(value));
            }
        }

        return remainder;
    }
}
