// <copyright file="SelectedImageConverter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Windows.Data;

namespace FatturaFacile.Converters;

/// <summary>
/// Converts item selections to corresponding asset image paths.
/// </summary>
public class SelectedImageConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value == null ? null : (object)@"Assets/Employee.png";
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
