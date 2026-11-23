// <copyright file="PartitaIva.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Taxation;

/// <summary>
/// The check of an Italian partita IVA: eleven digits whose last digit is the Luhn check digit.
/// </summary>
public static class PartitaIva
{
    /// <summary>
    /// The length of a partita IVA.
    /// </summary>
    public const int Length = 11;

    private const int Base = 10;
    private const int Nine = 9;

    /// <summary>
    /// Checks the format and the check digit of a partita IVA, without the IT prefix.
    /// </summary>
    /// <param name="value">The number to check.</param>
    /// <returns>True when the number is well formed.</returns>
    public static bool IsValid(string? value)
    {
        if (value is null || value.Length != Length || !value.All(char.IsAsciiDigit))
        {
            return false;
        }

        var sum = 0;
        for (var index = 0; index < Length; index++)
        {
            var digit = value[index] - '0';
            if (index % 2 != 0)
            {
                digit *= 2;
                if (digit > Nine)
                {
                    digit -= Nine;
                }
            }

            sum += digit;
        }

        return sum % Base == 0;
    }
}
