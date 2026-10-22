// <copyright file="AbsoluteToLocalConverter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows.Data;

namespace FatturaFacile.Wpf.Common.Converters;

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
        object                           value,
        Type                             targetType,
        object                           parameter,
        System.Globalization.CultureInfo culture) =>
        !(value is double) ? value : (double)value * (MaxValue - MinValue) + MinValue;

    /// <inheritdoc />
    public object ConvertBack(
        object                           value,
        Type                             targetType,
        object                           parameter,
        System.Globalization.CultureInfo culture) => throw new NotImplementedException();

}
