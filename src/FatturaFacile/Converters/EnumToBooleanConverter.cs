// <copyright file="EnumToBooleanConverter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;

namespace FatturaFacile.Converters;

/// <summary>
/// Converts enum values to boolean flags for radio buttons and toggle controls.
/// </summary>
public class EnumToBooleanConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets the target enum type.
    /// </summary>
    public Type? EnumType { get; set; }

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is string enumString && EnumType != null && value != null)
        {
            if (Enum.IsDefined(EnumType, value))
            {
                var enumValue = Enum.Parse(EnumType, enumString);
                return enumValue.Equals(value);
            }
        }

        return false;
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is string enumString && EnumType != null)
        {
            return Enum.Parse(EnumType, enumString);
        }

        return Binding.DoNothing;
    }
}
