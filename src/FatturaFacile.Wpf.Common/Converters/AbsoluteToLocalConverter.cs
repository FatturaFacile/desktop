// <copyright file="AbsoluteToLocalConverter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace FatturaFacile.Converters;

/// <summary>
/// Converts a normalized ratio (0.0 to 1.0) into a local scale value between <see cref="MinValue"/> and <see cref="MaxValue"/>.
/// </summary>
public class AbsoluteToLocalConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets the maximum scale value.
    /// </summary>
    public double MaxValue { get; set; }

    /// <summary>
    /// Gets or sets the minimum scale value.
    /// </summary>
    public double MinValue { get; set; }

    /// <inheritdoc />
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        System.Globalization.CultureInfo culture)
    {
        if (!(value is double))
        {
            return value;
        }

        return (double)value * (MaxValue - MinValue) + MinValue;
    }

    /// <inheritdoc />
    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        System.Globalization.CultureInfo culture) => throw new NotImplementedException();
}
