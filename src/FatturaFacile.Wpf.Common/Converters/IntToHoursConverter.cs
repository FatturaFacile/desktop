// <copyright file="IntToHoursConverter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Windows.Data;

namespace FatturaFacile.Wpf.Common.Converters;

/// <summary>
/// Converts an integer 24-hour value (0-23) into a formatted AM/PM hour string.
/// </summary>
public class IntToHoursConverter : IValueConverter
{

    /// <inheritdoc />
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is outside the valid 24-hour range (0 to 23).</exception>
    public object? Convert(
        object?     value,
        Type        targetType,
        object?     parameter,
        CultureInfo culture)
    {
        var intValue = value as decimal?;

        if (intValue == null)
        {
            return null;
        }

        return intValue < 0 || intValue > 23
            ? throw new ArgumentException(message: "Incorrect value hours!", paramName: nameof(value))
            : (object)(intValue < 13
                   ? string.Format(provider: culture, format: "{0}AM", arg0: intValue)
                   : string.Format(provider: culture, format: "{0}PM", arg0: intValue - 12));
    }

    /// <inheritdoc />
    public object? ConvertBack(
        object?     value,
        Type        targetType,
        object?     parameter,
        CultureInfo culture) => throw new NotImplementedException();

}
