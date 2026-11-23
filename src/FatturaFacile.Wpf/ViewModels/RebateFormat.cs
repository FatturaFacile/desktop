// <copyright file="RebateFormat.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Display and input formats of the supplier rebates views.
/// </summary>
internal static class RebateFormat
{
    /// <summary>
    /// The text of a missing value.
    /// </summary>
    internal const string NoValue = "-";

    private const string NumberFormat = "N2";
    private const string DateFormat = "d";

    /// <summary>
    /// Formats an amount with its currency.
    /// </summary>
    /// <param name="value">The amount.</param>
    /// <param name="currency">The currency.</param>
    /// <param name="culture">The display culture.</param>
    /// <returns>The text, for example "CHF 1'250.00".</returns>
    internal static string Money(decimal value, string currency, CultureInfo culture) => currency + " " + value.ToString(NumberFormat, culture);

    /// <summary>
    /// Formats an optional amount with its currency.
    /// </summary>
    /// <param name="value">The amount, or null.</param>
    /// <param name="currency">The currency.</param>
    /// <param name="culture">The display culture.</param>
    /// <returns>The text, or a dash.</returns>
    internal static string Money(decimal? value, string currency, CultureInfo culture) => value is { } amount ? Money(amount, currency, culture) : NoValue;

    /// <summary>
    /// Formats a percentage.
    /// </summary>
    /// <param name="value">The percentage.</param>
    /// <param name="culture">The display culture.</param>
    /// <returns>The text with the percent sign.</returns>
    internal static string Percent(decimal value, CultureInfo culture) => value.ToString(NumberFormat, culture) + "%";

    /// <summary>
    /// Formats an optional percentage.
    /// </summary>
    /// <param name="value">The percentage, or null.</param>
    /// <param name="culture">The display culture.</param>
    /// <returns>The text, or a dash.</returns>
    internal static string Percent(decimal? value, CultureInfo culture) => value is { } percent ? Percent(percent, culture) : NoValue;

    /// <summary>
    /// Formats a date.
    /// </summary>
    /// <param name="value">The date, or null.</param>
    /// <param name="culture">The display culture.</param>
    /// <returns>The short date, or a dash.</returns>
    internal static string Date(DateOnly? value, CultureInfo culture) => value is { } date ? date.ToString(DateFormat, culture) : NoValue;

    /// <summary>
    /// Reads an amount typed by the user.
    /// </summary>
    /// <param name="text">The typed text.</param>
    /// <param name="culture">The display culture.</param>
    /// <returns>The amount, or null when the text is empty or not a number.</returns>
    internal static decimal? ParseAmount(string? text, CultureInfo culture) =>
        !string.IsNullOrWhiteSpace(text) && decimal.TryParse(text, NumberStyles.Number, culture, out var amount) ? amount : null;
}
