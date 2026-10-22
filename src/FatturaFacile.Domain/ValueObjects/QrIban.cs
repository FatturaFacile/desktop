// <copyright file="QrIban.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.ValueObjects;

/// <summary>
/// Represents an immutable Swiss/Liechtenstein QR-IBAN dedicated to Swiss QR-bill (<c>QR-Rechnung</c>) payments.
/// A QR-IBAN is strictly 21 alphanumeric characters long, begins with the country prefix "CH" or "LI", 
/// and contains a special QR Institution Identification (QR-IID) routing number in character positions 5 to 9 (indices 4 to 8) 
/// falling within the statutory range 30000 to 31999 as specified by SIX Interbank Clearing.
/// </summary>
public sealed class QrIban : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QrIban"/> class after normalizing and validating SIX Swiss QR-bill rules.
    /// </summary>
    /// <param name="value">The raw QR-IBAN string (spaces permitted, case-insensitive).</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is empty, does not equal 21 characters, lacks a "CH"/"LI" country prefix, or contains a QR-IID outside the statutory 30000–31999 range.</exception>
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
    /// Gets the normalized 21-character alphanumeric QR-IBAN string without whitespace.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Validates whether a candidate IBAN string satisfies the statutory specifications of a Swiss/Liechtenstein QR-IBAN.
    /// Verifies 21-character length, "CH" or "LI" prefix, and a numeric QR-IID between 30000 and 31999 inclusive.
    /// </summary>
    /// <param name="iban">The IBAN candidate string to validate.</param>
    /// <returns><see langword="true"/> if <paramref name="iban"/> is a valid QR-IBAN; otherwise, <see langword="false"/>.</returns>
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
    /// Formats the QR-IBAN into standard banking electronic presentation groups of four characters (e.g., "CH44 3199 9123 4567 8").
    /// </summary>
    /// <returns>A space-separated 4-character grouped QR-IBAN representation.</returns>
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

    /// <summary>
    /// Returns the unformatted 21-character uppercase QR-IBAN string.
    /// </summary>
    /// <returns>The raw normalized alphanumeric QR-IBAN string.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Returns the constituent components used for value object equality evaluation.
    /// </summary>
    /// <returns>An enumerable sequence containing <see cref="Value"/>.</returns>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
