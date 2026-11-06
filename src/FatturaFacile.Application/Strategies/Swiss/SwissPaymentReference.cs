// <copyright file="SwissPaymentReference.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;
using FatturaFacile.Application.Common.Validation;

namespace FatturaFacile.Application.Strategies.Swiss;

/// <summary>
/// Generation and validation of Swiss QR-bill payment references:
/// QR reference (QRR, 27 digits, MOD 10 recursive) and ISO 11649 creditor reference (SCOR, "RF" + check digits).
/// </summary>
public static class SwissPaymentReference
{
    private const int QrReferenceLength = 27;
    private const int CreditorReferenceMaxPayload = 21;
    private static readonly int[] _mod10Table = [0, 9, 4, 6, 8, 2, 7, 1, 3, 5];

    /// <summary>
    /// Creates a 27-digit QR reference from the digits of a document number (last 26 digits + check digit).
    /// </summary>
    /// <param name="documentNumber">The invoice / document number.</param>
    /// <returns>The QR reference.</returns>
    public static string CreateQrReference(string documentNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentNumber);

        StringBuilder digits = new();

        foreach (var c in documentNumber)
        {
            if (char.IsAsciiDigit(c))
            {
                _ = digits.Append(c);
            }
        }

        var raw = digits.Length > 0 ? digits.ToString() : "1";

        if (raw.Length > QrReferenceLength - 1)
        {
            raw = raw[^(QrReferenceLength - 1)..];
        }

        var payload = raw.PadLeft(QrReferenceLength - 1, '0');
        return string.Create(CultureInfo.InvariantCulture, $"{payload}{ComputeMod10Recursive(payload)}");
    }

    /// <summary>
    /// Validates a QR reference (27 numeric digits with a valid MOD 10 recursive check digit).
    /// </summary>
    /// <param name="reference">The reference to validate.</param>
    /// <returns><see langword="true"/> when valid.</returns>
    public static bool IsValidQrReference(string? reference)
    {
        var value = (reference ?? string.Empty).Replace(" ", string.Empty, StringComparison.Ordinal);

        return value.Length == QrReferenceLength &&
               value.All(char.IsAsciiDigit) &&
               ComputeMod10Recursive(value[..^1]) == value[^1] - '0';
    }

    /// <summary>
    /// Creates an ISO 11649 creditor reference ("RF" + 2 check digits + up to 21 alphanumeric characters).
    /// </summary>
    /// <param name="documentNumber">The invoice / document number.</param>
    /// <returns>The creditor reference.</returns>
    public static string CreateCreditorReference(string documentNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentNumber);

        StringBuilder payload = new();

        foreach (var c in documentNumber.ToUpperInvariant())
        {
            if (char.IsAsciiLetterOrDigit(c))
            {
                _ = payload.Append(c);
            }
        }

        var raw = payload.Length > 0 ? payload.ToString() : "1";

        if (raw.Length > CreditorReferenceMaxPayload)
        {
            raw = raw[^CreditorReferenceMaxPayload..];
        }

        var checkDigits = 98 - IbanValidator.ComputeMod97(raw + "RF00");
        return string.Create(CultureInfo.InvariantCulture, $"RF{checkDigits:00}{raw}");
    }

    /// <summary>
    /// Validates an ISO 11649 creditor reference.
    /// </summary>
    /// <param name="reference">The reference to validate.</param>
    /// <returns><see langword="true"/> when valid.</returns>
    public static bool IsValidCreditorReference(string? reference)
    {
        var value = (reference ?? string.Empty).Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();

        return value.Length is >= 5 and <= 25 &&
               value.StartsWith("RF", StringComparison.Ordinal) &&
               value.All(char.IsAsciiLetterOrDigit) &&
               IbanValidator.ComputeMod97(string.Concat(value.AsSpan(4), value.AsSpan(0, 4))) == 1;
    }

    /// <summary>
    /// Computes the MOD 10 recursive check digit used by QR references.
    /// </summary>
    /// <param name="digits">The numeric payload.</param>
    /// <returns>The check digit (0-9).</returns>
    public static int ComputeMod10Recursive(string digits)
    {
        ArgumentNullException.ThrowIfNull(digits);

        var carry = 0;

        foreach (var c in digits)
        {
            carry = _mod10Table[(carry + (c - '0')) % 10];
        }

        return (10 - carry) % 10;
    }
}
