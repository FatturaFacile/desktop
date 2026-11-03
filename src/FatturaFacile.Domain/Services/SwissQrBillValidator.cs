// <copyright file="SwissQrBillValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;

namespace FatturaFacile.Domain.Services;

/// <summary>
/// Implements Swiss QR-Bill validation and Modulo 10 / Modulo 97 checksum calculations.
/// Directly matches Laravel QrBillGeneratorService validation specifications.
/// </summary>
public static class SwissQrBillValidator
{
    private const int SwissIbanLength = 21;
    private const int IidStartIndex = 4;
    private const int IidLength = 5;

    private const int MinimumIbanLength = 15;
    private const int MaximumIbanLength = 34;

    private const int QrIbanMinimumIid = 30000;
    private const int QrIbanMaximumIid = 31999;

    private const int IbanModulo = 97;
    private const int ValidIbanRemainder = 1;

    private const int AlphaNumericOffset = 10;
    private const int ChunkLength = 7;

    private const int Mod10Base = 10;

    private static readonly int[] Mod10RecursiveTable =
    [
        0, 9, 4, 6, 8,
        2, 7, 1, 3, 5,
    ];

    /// <summary>
    /// Validate whether an IBAN is a valid Swiss QR-IBAN.
    /// In Switzerland, QR-IBANs are defined by an IID
    /// (positions 5-9) in the range 30000-31999.
    /// </summary>
    /// <param name="iban">The IBAN string to validate.</param>
    /// <returns>
    /// True if the IBAN is a valid Swiss QR-IBAN;
    /// otherwise false.
    /// </returns>
    public static bool IsValidQrIban(string? iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
        {
            return false;
        }

        var clean = iban
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Trim()
            .ToUpperInvariant();

        if (clean.Length != SwissIbanLength)
        {
            return false;
        }

        if (!clean.StartsWith("CH", StringComparison.Ordinal) &&
            !clean.StartsWith("LI", StringComparison.Ordinal))
        {
            return false;
        }

        var iidString = clean.Substring(
            IidStartIndex,
            IidLength);

        return !int.TryParse(
                iidString,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var iid)
            ? false
            : iid >= QrIbanMinimumIid &&
            iid <= QrIbanMaximumIid && ValidateIbanChecksum(clean);
    }

    /// <summary>
    /// Standard MOD-97 IBAN checksum validation.
    /// </summary>
    /// <param name="iban">The IBAN string to validate.</param>
    /// <returns>
    /// True if the IBAN has a valid MOD-97 checksum;
    /// otherwise false.
    /// </returns>
    public static bool ValidateIbanChecksum(string iban)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(iban);

        var clean = iban
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Trim()
            .ToUpperInvariant();

        if (clean.Length is < MinimumIbanLength or
            > MaximumIbanLength)
        {
            return false;
        }

        var reordered = clean[4..] + clean[..4];

        var numeric = new StringBuilder();

        foreach (var ch in reordered)
        {
            if (char.IsDigit(ch))
            {
                _ = numeric.Append(ch);
            }
            else if (char.IsLetter(ch))
            {
                _ = numeric.Append(
                    ch - 'A' + AlphaNumericOffset);
            }
            else
            {
                return false;
            }
        }

        var remainder = 0;
        var numericString = numeric.ToString();

        for (var index = 0;
             index < numericString.Length;
             index += ChunkLength)
        {
            var chunk = numericString.Substring(
                index,
                Math.Min(
                    ChunkLength,
                    numericString.Length - index));

            var combined = remainder > 0
                ? remainder.ToString(CultureInfo.InvariantCulture) + chunk
                : chunk;

            if (!long.TryParse(
                    combined,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var value))
            {
                return false;
            }

            remainder = (int)(value % IbanModulo);
        }

        return remainder == ValidIbanRemainder;
    }

    /// <summary>
    /// Calculates the Swiss recursive Modulo-10 check digit
    /// used by QR references and ESR references.
    /// </summary>
    /// <param name="input">
    /// Input digits from which the check digit is calculated.
    /// Non-digit characters are ignored.
    /// </param>
    /// <returns>
    /// The calculated check digit.
    /// </returns>
    public static int CalculateMod10RecursiveCheckDigit(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var digits = new string(
            [.. input.Where(char.IsDigit)]);

        var carry = 0;

        foreach (var digit in digits)
        {
            carry = Mod10RecursiveTable[
                (carry + (digit - '0')) % Mod10Base];
        }

        return (Mod10Base - carry) % Mod10Base;
    }
}
