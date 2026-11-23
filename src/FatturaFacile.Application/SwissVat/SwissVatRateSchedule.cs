// <copyright file="SwissVatRateSchedule.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SwissVat;

/// <summary>
/// Statutory Swiss VAT rate epochs (2024+ vs 2018-2023) and official AFC Saldosteuersatz rate bands.
/// </summary>
public static class SwissVatRateSchedule
{
    /// <summary>
    /// Effective date of current Swiss VAT rates (8.1%, 2.6%, 3.8%).
    /// </summary>
    public static readonly DateOnly CurrentEpochStart = new(2024, 1, 1);

    /// <summary>
    /// Official AFC/ESTV Saldosteuersatz rate percentages.
    /// </summary>
    public static readonly IReadOnlyList<decimal> SaldosteuersatzBands =
    [
        0.10m, 0.60m, 1.30m, 2.10m, 3.00m, 3.70m, 4.50m, 5.30m, 6.20m, 6.80m,
    ];

    /// <summary>
    /// Returns the statutory rates and AFC box codes active on <paramref name="date"/>.
    /// </summary>
    /// <param name="date">The reference date.</param>
    /// <returns>The active rate epoch metadata.</returns>
    public static SwissVatEpochRates ForDate(DateOnly date)
    {
        if (date >= CurrentEpochStart)
        {
            return new SwissVatEpochRates(
                StandardRate: 8.10m,
                ReducedRate: 2.60m,
                AccommodationRate: 3.80m,
                StandardBox: "303",
                ReducedBox: "313",
                AccommodationBox: "343",
                BezugsteuerBox: "383",
                SssPrimaryBox: "323",
                SssSecondaryBox: "333");
        }

        return new SwissVatEpochRates(
            StandardRate: 7.70m,
            ReducedRate: 2.50m,
            AccommodationRate: 3.70m,
            StandardBox: "302",
            ReducedBox: "312",
            AccommodationBox: "342",
            BezugsteuerBox: "382",
            SssPrimaryBox: "322",
            SssSecondaryBox: "332");
    }

    /// <summary>
    /// Rounds an amount according to Swiss commercial 0.05 CHF rounding or standard 0.01 CHF rounding.
    /// </summary>
    /// <param name="amount">The raw amount.</param>
    /// <param name="commercial005">True for 0.05 CHF rounding; false for 0.01 CHF.</param>
    /// <returns>The rounded CHF amount.</returns>
    public static decimal RoundSwiss(decimal amount, bool commercial005 = true) =>
        commercial005
            ? Math.Round(amount * 20m, 0, MidpointRounding.AwayFromZero) / 20m
            : Math.Round(amount, 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Validates a Swiss UID-MWST number (CHE-XXX.XXX.XXX MWST) including its Mod-11 check digit.
    /// </summary>
    /// <param name="raw">The raw UID string.</param>
    /// <returns>True when valid; otherwise false.</returns>
    public static bool IsValidUidMwst(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        Span<char> digits = stackalloc char[9];
        var count = 0;
        foreach (var ch in raw)
        {
            if (char.IsAsciiDigit(ch))
            {
                if (count >= 9)
                {
                    return false;
                }

                digits[count++] = ch;
            }
        }

        if (count != 9)
        {
            return false;
        }

        if (digits.SequenceEqual("000000000"))
        {
            return false;
        }

        ReadOnlySpan<int> weights = [5, 4, 3, 2, 7, 6, 5, 4];
        var sum = 0;
        for (var i = 0; i < 8; i++)
        {
            sum += (digits[i] - '0') * weights[i];
        }

        var expected = (11 - (sum % 11)) % 11;
        return expected != 10 && expected == (digits[8] - '0');
    }

    /// <summary>
    /// Formats a UID string to canonical <c>CHE-XXX.XXX.XXX MWST</c>, or returns a zeroed placeholder when invalid.
    /// </summary>
    /// <param name="raw">The raw UID string.</param>
    /// <returns>Canonical formatted UID-MWST string.</returns>
    public static string NormalizeUid(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "CHE-000.000.000 MWST";
        }

        var digits = new string([.. raw.Where(char.IsAsciiDigit)]);
        if (digits.Length != 9)
        {
            return "CHE-000.000.000 MWST";
        }

        return string.Concat("CHE-", digits.AsSpan(0, 3), ".", digits.AsSpan(3, 3), ".", digits.AsSpan(6, 3), " MWST");
    }
}
