// <copyright file="FiscalCode.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.RegularExpressions;

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.ValueObjects;

/// <summary>
/// Represents an immutable Italian Tax Identification Code (Codice Fiscale)
/// for natural persons or legal entities.
/// </summary>
public sealed partial class FiscalCode : ValueObject
{
    /// <summary>
    /// Length of a standard personal fiscal code.
    /// </summary>
    private const int StandardFiscalCodeLength = 16;

    /// <summary>
    /// Length of a numeric legal-entity fiscal code.
    /// </summary>
    private const int NumericFiscalCodeLength = 11;

    /// <summary>
    /// Initializes a new instance of the <see cref="FiscalCode"/> class.
    /// </summary>
    /// <param name="value">Fiscal code value.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the fiscal code is null, empty or invalid.
    /// </exception>
    public FiscalCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Fiscal code cannot be empty.",
                nameof(value));
        }

        var normalized = value.Trim().ToUpperInvariant();

        if (!IsValid(normalized))
        {
            throw new ArgumentException(
                $"Invalid Italian fiscal code format: '{value}'.",
                nameof(value));
        }

        Value = normalized;
    }

    /// <summary>
    /// Gets the normalized fiscal code value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the regular expression used to validate
    /// standard 16-character Italian fiscal codes.
    /// </summary>
    [GeneratedRegex(
        @"^[A-Z]{6}[0-9LMNPQRSTUV]{2}[A-EHLMPR-T][0-9LMNPQRSTUV]{2}[A-Z][0-9LMNPQRSTUV]{3}[A-Z]$")]
    private static partial Regex StandardFiscalCodeRegex();

    /// <summary>
    /// Gets the regular expression used to validate
    /// 11-digit numeric fiscal codes.
    /// </summary>
    [GeneratedRegex(@"^\d{11}$")]
    private static partial Regex NumericFiscalCodeRegex();

    /// <summary>
    /// Determines whether the specified fiscal code
    /// has a valid Italian fiscal code format.
    /// </summary>
    /// <param name="code">Fiscal code to validate.</param>
    /// <returns>
    /// <see langword="true"/> when the value is valid;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public static bool IsValid(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        code = code.Trim().ToUpperInvariant();

        return code.Length switch
        {
            StandardFiscalCodeLength =>
                StandardFiscalCodeRegex().IsMatch(code),

            NumericFiscalCodeLength =>
                NumericFiscalCodeRegex().IsMatch(code),

            _ => false,
        };
    }

    /// <summary>
    /// Returns the normalized fiscal code value.
    /// </summary>
    /// <returns>
    /// The normalized fiscal code.
    /// </returns>
    public override string ToString()
    {
        return Value;
    }

    /// <summary>
    /// Returns the equality components used by the value object.
    /// </summary>
    /// <returns>
    /// The components participating in equality comparison.
    /// </returns>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
