// <copyright file="QrIban.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.ValueObjects;

/// <summary>
/// Represents a Swiss/Liechtenstein QR-IBAN used for Swiss QR-bill payments.
/// Must start with CH or LI, have a total length of 21 characters, and contain a QR-IID between 30000 and 31999.
/// </summary>
public sealed class QrIban : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QrIban"/> class.
    /// </summary>
    /// <param name="value">The raw QR-IBAN string representation.</param>
    /// <exception cref="ArgumentException">Thrown when the QR-IBAN does not meet Swiss statutory standards.</exception>
    public QrIban(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "QR-IBAN cannot be empty.",
                nameof(value));
        }

        var normalized = value.Replace(
            " ",
            string.Empty,
            StringComparison.Ordinal).Trim().ToUpperInvariant();
        if (!IsValid(normalized))
        {
            throw new ArgumentException(
                $"Invalid Swiss/Liechtenstein QR-IBAN '{value}'. Must have 21 characters and QR-IID between 30000 and 31999.",
                nameof(value));
        }

        Value = normalized;
    }

    /// <summary>
    /// Gets the normalized 21-character alphanumeric QR-IBAN string.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Validates whether an IBAN candidate conforms to official Swiss QR-IBAN requirements.
    /// </summary>
    /// <param name="iban">The IBAN candidate string.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(
        string iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
        {
            return false;
        }

        var clean = iban.Replace(
            " ",
            string.Empty,
            StringComparison.Ordinal).Trim().ToUpperInvariant();

        if (clean.Length != 21)
        {
            return false;
        }

        if (!clean.StartsWith(
                "CH",
                StringComparison.Ordinal) && !clean.StartsWith(
                "LI",
                StringComparison.Ordinal))
        {
            return false;
        }

        // Characters 5-9 represent the QR-IID
        var iidString = clean.Substring(
            4,
            5);
        return int.TryParse(
                   iidString,
                   System.Globalization.CultureInfo.InvariantCulture,
                   out var iid) &&
               iid is >= 30000 and <= 31999;
    }

    /// <summary>
    /// Formats the QR-IBAN into groups of 4 characters according to banking presentation conventions.
    /// </summary>
    /// <returns>The spaced QR-IBAN string.</returns>
    public string ToFormattedString()
    {
        // Format as groups of 4: CHxx xxxx xxxx xxxx xxxx x
        return string.Join(
            ' ',
            Enumerable.Range(
                    0,
                    (Value.Length + 3) / 4)
                .Select(i => Value.Substring(
                    i * 4,
                    Math.Min(
                        4,
                        Value.Length - (i * 4)))));
    }

    /// <inheritdoc/>
    public override string ToString() => Value;

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
