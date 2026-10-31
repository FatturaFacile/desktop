// <copyright file="SwissQrBillValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text;

namespace FatturaFacile.Domain.Services;

/// <summary>
/// Implements Swiss QR-Bill validation and Modulo 10 / Modulo 97 checksum calculations.
/// Directly matches Laravel QrBillGeneratorService validation specifications.
/// </summary>
public static class SwissQrBillValidator
{
    /// <summary>
    /// Validate whether an IBAN is a valid Swiss QR-IBAN.
    /// In Switzerland, QR-IBANs are defined by an IID (positions 5-9) in the range 30000 - 31999.
    /// </summary>
    public static bool IsValidQrIban(string? iban)
    {
        if (string.IsNullOrWhiteSpace(iban)) return false;
        var clean = iban.Replace(" ", "").Trim().ToUpperInvariant();

        if (clean.Length != 21) return false;
        if (!clean.StartsWith("CH") && !clean.StartsWith("LI")) return false;

        var iidString = clean.Substring(4, 5);
        if (!int.TryParse(iidString, out var iid) || iid < 30000 || iid > 31999)
        {
            return false;
        }

        return ValidateIbanChecksum(clean);
    }

    /// <summary>
    /// Standard MOD-97 IBAN checksum validation.
    /// </summary>
    public static bool ValidateIbanChecksum(string iban)
    {
        var clean = iban.Replace(" ", "").Trim().ToUpperInvariant();
        if (clean.Length < 15 || clean.Length > 34) return false;

        // Move first 4 characters to end and convert letters to numbers (A=10, B=11, ... Z=35)
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
                _ = numeric.Append(ch - 'A' + 10);
            }
            else
            {
                return false;
            }
        }

        var remainder = 0;
        var str = numeric.ToString();
        for (var i = 0; i < str.Length; i += 7)
        {
            var chunk = str.Substring(i, Math.Min(7, str.Length - i));
            var combined = remainder > 0 ? remainder.ToString() + chunk : chunk;
            if (!long.TryParse(combined, out var value))
            {
                return false;
            }
            remainder = (int)(value % 97);
        }

        return remainder == 1;
    }

    /// <summary>
    /// Calculate Modulo 10 recursive check digit (Luhn-style table variant used by SIX Group).
    /// </summary>
    public static int CalculateMod10RecursiveCheckDigit(string input)
    {
        var digits = new string(input.Where(char.IsDigit).ToArray());
        int[] table = [0, 9, 4, 6, 8, 2, 7, 1, 3, 5];
        var carry = 0;

        foreach (var ch in digits)
        {
            carry = table[(carry + (ch - '0')) % 10];
        }

        return (10 - carry) % 10;
    }
}
