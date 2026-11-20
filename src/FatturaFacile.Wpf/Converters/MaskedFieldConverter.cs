// <copyright file="MaskedFieldConverter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Data;
using FatturaFacile.Application.Common.Security.Iam;

namespace FatturaFacile.Wpf.Converters;

/// <summary>
/// Renders a protected value according to the field policies of the signed-in user: a masked amount as a bullet mask, a
/// masked IBAN keeping country and last four characters, and nothing for a hidden field. The converter parameter is
/// resource|field|kind, for example hr.employee|base_annual_salary|amount.
/// </summary>
/// <param name="authorization">The authorization service holding the field policies.</param>
[SuppressMessage("Design", "MA0182:Unused internal type", Justification = "Used from XAML.")]
internal sealed class MaskedFieldConverter(IAuthorizationService authorization) : IValueConverter
{
    /// <summary>
    /// The separator of the parts of the converter parameter.
    /// </summary>
    private const char Separator = '|';

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var parts = (parameter as string)?.Split(Separator) ?? [];
        if (parts.Length < 2)
        {
            return value;
        }

        var kind = parts.Length > 2 ? parts[2] : "amount";
        var visibility = authorization.VisibilityOf(parts[0], parts[1]);

        return FieldMasker.Render(value, visibility, kind);
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("A masked value is read only.");
}
