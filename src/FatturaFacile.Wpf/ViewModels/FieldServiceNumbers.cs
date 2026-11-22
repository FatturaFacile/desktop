// <copyright file="FieldServiceNumbers.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Reads the numbers typed in the Field Service forms. The decimal separator of the user's culture is accepted and so is the
/// point, but never a thousands separator, so "1.5" is one and a half hours and not fifteen.
/// </summary>
internal static class FieldServiceNumbers
{
    private const NumberStyles AmountStyle = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;

    /// <summary>
    /// Reads a required amount.
    /// </summary>
    /// <param name="text">The text typed.</param>
    /// <param name="culture">The culture of the user.</param>
    /// <param name="value">The amount.</param>
    /// <returns>True when the text is a number.</returns>
    public static bool TryAmount(string text, CultureInfo culture, out decimal value) =>
        decimal.TryParse(text, AmountStyle, culture, out value) || decimal.TryParse(text, AmountStyle, CultureInfo.InvariantCulture, out value);

    /// <summary>
    /// Reads an optional amount: blank is valid and means none.
    /// </summary>
    /// <param name="text">The text typed.</param>
    /// <param name="culture">The culture of the user.</param>
    /// <param name="value">The amount, or null when blank.</param>
    /// <returns>True when the text is blank or a number.</returns>
    public static bool TryOptional(string text, CultureInfo culture, out decimal? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        if (!TryAmount(text, culture, out var parsed))
        {
            return false;
        }

        value = parsed;

        return true;
    }

    /// <summary>
    /// Reads an optional whole number: blank is valid and means none.
    /// </summary>
    /// <param name="text">The text typed.</param>
    /// <param name="culture">The culture of the user.</param>
    /// <param name="value">The number, or null when blank.</param>
    /// <returns>True when the text is blank or a whole number.</returns>
    public static bool TryWhole(string text, CultureInfo culture, out int? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        if (!int.TryParse(text, NumberStyles.AllowLeadingSign, culture, out var parsed))
        {
            return false;
        }

        value = parsed;

        return true;
    }
}
