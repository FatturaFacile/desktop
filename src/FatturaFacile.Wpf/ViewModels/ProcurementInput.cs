// <copyright file="ProcurementInput.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Reads the numbers typed in the purchase order fulfilment forms.
/// </summary>
internal static class ProcurementInput
{
    /// <summary>
    /// Parses an optional decimal typed by the user, with the culture of the user and, failing that, the invariant culture.
    /// </summary>
    /// <param name="text">The text, empty for none.</param>
    /// <param name="culture">The culture of the user.</param>
    /// <param name="value">The value, null when the text is empty.</param>
    /// <returns>True when the text is empty or a number.</returns>
    internal static bool TryDecimal(string? text, CultureInfo culture, out decimal? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        var trimmed = text.Trim();
        if (decimal.TryParse(trimmed, NumberStyles.Number, culture, out var local) || decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.InvariantCulture, out local))
        {
            value = local;

            return true;
        }

        return false;
    }
}
