// <copyright file="ReportFormat.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;

namespace FatturaFacile.Application.Features.Reports.Builders;

/// <summary>
/// Formatting helpers shared by the report data builders.
/// </summary>
public static class ReportFormat
{
    /// <summary>
    /// Formats an amount with two decimals and group separators.
    /// </summary>
    /// <param name="value">The amount.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>The formatted amount.</returns>
    public static string Amount(decimal value, CultureInfo culture) => value.ToString("N2", culture);

    /// <summary>
    /// Formats an optional amount, returning an empty string when missing or zero.
    /// </summary>
    /// <param name="value">The amount.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>The formatted amount or an empty string.</returns>
    public static string OptionalAmount(decimal? value, CultureInfo culture) => value is { } amount && amount != 0m
        ? amount.ToString("N2", culture)
        : string.Empty;

    /// <summary>
    /// Formats a quantity without trailing zeros.
    /// </summary>
    /// <param name="value">The quantity.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>The formatted quantity.</returns>
    public static string Quantity(decimal value, CultureInfo culture) => value.ToString("0.###", culture);

    /// <summary>
    /// Formats a percentage without trailing zeros (e.g. "22", "8.1").
    /// </summary>
    /// <param name="value">The percentage.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>The formatted percentage.</returns>
    public static string Percent(decimal value, CultureInfo culture) => value.ToString("0.##", culture) + "%";

    /// <summary>
    /// Formats a date.
    /// </summary>
    /// <param name="value">The date.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>The formatted date or an empty string.</returns>
    public static string Date(DateTime? value, CultureInfo culture) => value?.ToString("d", culture) ?? string.Empty;

    /// <summary>
    /// Builds the "street number" line of an address.
    /// </summary>
    /// <param name="street">The street.</param>
    /// <param name="houseNumber">The house number.</param>
    /// <returns>The address line.</returns>
    public static string StreetLine(string? street, string? houseNumber) => $"{street} {houseNumber}".Trim();

    /// <summary>
    /// Builds the "postal code city (province)" line of an address, with the country prefix for foreign addresses.
    /// </summary>
    /// <param name="postalCode">The postal code.</param>
    /// <param name="city">The city.</param>
    /// <param name="province">The province (Italy).</param>
    /// <param name="country">The address country.</param>
    /// <param name="homeCountry">The country of the issuing company.</param>
    /// <returns>The city line.</returns>
    public static string CityLine(string? postalCode, string? city, string? province, string? country, string homeCountry)
    {
        var line = $"{postalCode} {city}".Trim();

        if (!string.IsNullOrWhiteSpace(province))
        {
            line += $" ({province})";
        }

        return !string.IsNullOrWhiteSpace(country) && !string.Equals(country, homeCountry, StringComparison.OrdinalIgnoreCase)
            ? $"{country.ToUpperInvariant()}-{line}"
            : line;
    }

    /// <summary>
    /// Replaces characters not allowed in file names.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A safe file name fragment.</returns>
    public static string SafeFileName(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var invalid = Path.GetInvalidFileNameChars();
        return new string([.. value.Select(c => invalid.Contains(c) ? '_' : c)]);
    }
}
