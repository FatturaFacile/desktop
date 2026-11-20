// <copyright file="FieldMasker.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;

namespace FatturaFacile.Application.Common.Security.Iam;

/// <summary>
/// Renders masked values: amounts as •••, IBANs keeping the country and the last four characters. The raw value of
/// a masked or hidden field is never kept.
/// </summary>
public static class FieldMasker
{
    /// <summary>
    /// The mask of a monetary value.
    /// </summary>
    public const string AmountMask = "•••";

    /// <summary>
    /// The kind of an IBAN value.
    /// </summary>
    public const string IbanKind = "iban";

    /// <summary>
    /// Renders a value according to its visibility.
    /// </summary>
    /// <param name="value">The raw or already masked value.</param>
    /// <param name="visibility">The visibility the policies give the field.</param>
    /// <param name="kind">The kind of value (amount or iban).</param>
    /// <exception cref="ArgumentOutOfRangeException">The visibility is not a defined value.</exception>
    /// <returns>The text to show, or null for a hidden field.</returns>
    public static string? Render(object? value, FieldVisibility visibility, string kind) => visibility switch
    {
        FieldVisibility.Hidden => null,
        FieldVisibility.Masked => string.Equals(kind, IbanKind, StringComparison.OrdinalIgnoreCase) ? MaskIban(value) : AmountMask,
        FieldVisibility.Visible => Convert.ToString(value, CultureInfo.CurrentCulture),
        _ => throw new ArgumentOutOfRangeException(nameof(visibility), visibility, message: null),
    };

    /// <summary>
    /// Masks an IBAN keeping the two country letters and the last four characters, grouped by four.
    /// </summary>
    /// <param name="iban">The IBAN, with or without spaces.</param>
    /// <returns>For example CH** **** **** **** *295 7; the amount mask when the value is not an IBAN.</returns>
    public static string MaskIban(object? iban)
    {
        var compact = new string(
            [.. (Convert.ToString(iban, CultureInfo.InvariantCulture) ?? string.Empty).Where(static c => !char.IsWhiteSpace(c))])
            .ToUpperInvariant();

        if (compact.Length < 8 || !char.IsAsciiLetterUpper(compact[0]) || !char.IsAsciiLetterUpper(compact[1]))
        {
            return AmountMask;
        }

        var masked = compact[..2] + new string('*', compact.Length - 6) + compact[^4..];
        var builder = new StringBuilder(masked.Length + (masked.Length / 4));
        for (var i = 0; i < masked.Length; i++)
        {
            if (i > 0 && i % 4 == 0)
            {
                _ = builder.Append(' ');
            }

            _ = builder.Append(masked[i]);
        }

        return builder.ToString();
    }
}
