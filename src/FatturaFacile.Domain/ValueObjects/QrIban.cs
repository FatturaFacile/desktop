// <copyright file="QrIban.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.ValueObjects;

/// <summary>
/// Represents an immutable Swiss/Liechtenstein QR-IBAN dedicated to Swiss QR-bill
/// (QR-Rechnung) payments.
/// A QR-IBAN is strictly 21 alphanumeric characters long, begins with the country
/// prefix "CH" or "LI", and contains a special QR Institution Identification
/// (QR-IID) routing number in positions 5 to 9.
/// </summary>
public sealed class QrIban : ValueObject
{
    private const int QrIbanLength = 21;
    private const int QrIidStartIndex = 4;
    private const int QrIidLength = 5;
    private const int QrIidMinimumValue = 30000;
    private const int QrIidMaximumValue = 31999;
    private const int FormattingGroupLength = 4;

    /// <summary>
    /// Initializes a new instance of the <see cref="QrIban"/> class.
    /// </summary>
    /// <param name="value">
    /// The raw QR-IBAN value.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the specified QR-IBAN is invalid.
    /// </exception>
    public QrIban(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = Normalize(value);

        if (!IsValid(normalized))
        {
            throw new ArgumentException(
                $"Invalid Swiss/Liechtenstein QR-IBAN '{value}'. " +
                $"QR-IID must be between {QrIidMinimumValue} and {QrIidMaximumValue}.",
                nameof(value));
        }

        Value = normalized;
    }

    /// <summary>
    /// Gets the normalized QR-IBAN value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Validates whether the specified value is a valid Swiss/Liechtenstein QR-IBAN.
    /// </summary>
    /// <param name="iban">The value to validate.</param>
    /// <returns>
    /// <see langword="true"/> if valid; otherwise <see langword="false"/>.
    /// </returns>
    public static bool IsValid(string iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
        {
            return false;
        }

        var clean = Normalize(iban);

        if (clean.Length != QrIbanLength)
        {
            return false;
        }

        if (!clean.StartsWith("CH", StringComparison.Ordinal) &&
            !clean.StartsWith("LI", StringComparison.Ordinal))
        {
            return false;
        }

        var iidString = clean.Substring(
            QrIidStartIndex,
            QrIidLength);

        return int.TryParse(
                iidString,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var iid) && iid is >= QrIidMinimumValue and
               <= QrIidMaximumValue;
    }

    /// <summary>
    /// Returns the QR-IBAN formatted in groups of four characters.
    /// </summary>
    /// <returns>
    /// A human-readable QR-IBAN.
    /// </returns>
    public string ToFormattedString()
    {
        return string.Join(
            ' ',
            Enumerable.Range(
                    0,
                    (Value.Length + (FormattingGroupLength - 1)) / FormattingGroupLength)
                .Select(index =>
                    Value.Substring(
                        index * FormattingGroupLength,
                        Math.Min(
                            FormattingGroupLength,
                            Value.Length - (index * FormattingGroupLength)))));
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    private static string Normalize(string iban)
    {
        return iban.Replace(
                " ",
                string.Empty,
                StringComparison.Ordinal)
            .Trim()
            .ToUpperInvariant();
    }
}
