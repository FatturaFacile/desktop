// <copyright file="MesKeypad.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;

namespace FatturaFacile.Application.Mes;

/// <summary>
/// Logic of the numeric touch keypad: the quantity is typed as a string, so decimals are exact and the soft keyboard is never
/// needed.
/// </summary>
public static class MesKeypad
{
    /// <summary>
    /// The key that removes the last character.
    /// </summary>
    public const string Back = "back";

    /// <summary>
    /// The key that resets the quantity to zero.
    /// </summary>
    public const string Clear = "clear";

    private const string Zero = "0";
    private const string Point = ".";
    private const int MaxIntegerDigits = 9;
    private const int MaxDecimals = 4;

    /// <summary>
    /// Applies a key to the quantity being typed: digits append, the decimal point starts the decimals once, the back key removes
    /// the last character and the clear key resets to zero.
    /// </summary>
    /// <param name="current">The quantity typed so far.</param>
    /// <param name="key">The key.</param>
    /// <returns>The new quantity.</returns>
    public static string Apply(string current, string key)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(key);

        return key switch
        {
            Clear => Zero,
            Back => Remove(current),
            Point => current.Contains('.', StringComparison.Ordinal) ? current : current + Point,
            _ when key.Length == 1 && char.IsAsciiDigit(key[0]) => Append(current, key),
            _ => current,
        };
    }

    /// <summary>
    /// Adds a whole number of pieces (a quick increment) to the quantity.
    /// </summary>
    /// <param name="current">The quantity typed so far.</param>
    /// <param name="pieces">The pieces to add.</param>
    /// <returns>The new quantity.</returns>
    public static string Add(string current, int pieces) =>
        Math.Max(0m, Parse(current) + pieces).ToString("0.####", CultureInfo.InvariantCulture);

    /// <summary>
    /// Parses the typed quantity.
    /// </summary>
    /// <param name="text">The quantity as typed.</param>
    /// <returns>The quantity, zero when it is not a number.</returns>
    public static decimal Parse(string text) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : 0m;

    private static string Remove(string current)
    {
        var next = current.Length > 0 ? current[..^1] : string.Empty;

        return next.Length == 0 ? Zero : next;
    }

    private static string Append(string current, string digit)
    {
        return current.Split('.') switch
        {
            [_, var decimals] => decimals.Length >= MaxDecimals ? current : current + digit,
            [Zero] => digit,
            [var whole] => whole.Length >= MaxIntegerDigits ? current : current + digit,
            _ => current,
        };
    }
}
