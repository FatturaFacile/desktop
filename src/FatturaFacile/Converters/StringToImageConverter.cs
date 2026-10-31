// <copyright file="StringToImageConverter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;

namespace FatturaFacile.Converters;

/// <summary>
/// Maps status descriptions to visual image indicator resources.
/// </summary>
public class StringToImageConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value != null)
        {
            var str = value.ToString();

            if (str == "Sufficient")
            {
                return @"Assets/Sufficient.png";
            }
            else if (str == "Insufficient")
            {
                return @"Assets/Insufficient.png";
            }
            else if (str == "Perfect")
            {
                return @"Assets/Perfect.png";
            }
        }

        return null;
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
