// <copyright file="FiscalCode.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.RegularExpressions;

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.ValueObjects;

/// <summary>
/// Represents an immutable Italian Fiscal Code (Codice Fiscale).
/// Supports both:
/// - 16-character personal fiscal codes.
/// - 11-digit legal entity fiscal codes.
/// </summary>
public sealed partial class FiscalCode : ValueObject
{
    /// <summary>
    /// Standard Italian fiscal code length.
    /// </summary>
    private const int StandardFiscalCodeLength = 16;

    /// <summary>
    /// Numeric fiscal code length for legal entities.
    /// </summary>
    private const int NumericFiscalCodeLength = 11;

    private const int RegexTimeoutMilliseconds = 1000;

    /// <summary>
    /// Initializes a new instance of the <see cref="FiscalCode"/> class.
    /// </summary>
    /// <param name="value">Fiscal code value.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the fiscal code is invalid.
    /// </exception>
    public FiscalCode(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = Normalize(value);

        if (!IsValid(normalized))
        {
            throw new ArgumentException(
                $"Invalid fiscal code '{value}'.",
                nameof(value));
        }

        Value = normalized;
    }

    /// <summary>
    /// Gets the normalized fiscal code.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets a value indicating whether the fiscal code belongs
    /// to a legal entity.
    /// </summary>
    public bool IsLegalEntity =>
        Value.Length == NumericFiscalCodeLength;

    /// <summary>
    /// Gets a value indicating whether the fiscal code belongs
    /// to a natural person.
    /// </summary>
    public bool IsNaturalPerson =>
        Value.Length == StandardFiscalCodeLength;

    /// <summary>
    /// Validates whether the specified normalized fiscal code conforms to the statutory format of Italian Codice Fiscale.
    /// </summary>
    /// <returns>A regular expression for validating standard fiscal codes.</returns>
    [GeneratedRegex(
        @"^[A-Z]{6}[0-9LMNPQRSTUV]{2}[A-EHLMPR-T][0-9LMNPQRSTUV]{2}[A-Z][0-9LMNPQRSTUV]{3}[A-Z]$",
        RegexOptions.None,
        RegexTimeoutMilliseconds)]
    private static partial Regex StandardFiscalCodeRegex { get; }

    /// <summary>
    /// Validates whether the specified normalized fiscal code consists of exactly 11 digits.
    /// </summary>
    /// <returns>A regular expression for validating numeric fiscal codes.</returns>
    [GeneratedRegex(
        @"^\d{11}$",
        RegexOptions.None,
        RegexTimeoutMilliseconds)]
    private static partial Regex NumericFiscalCodeRegex { get; }

    /// <summary>
    /// Explicit conversion from string.
    /// </summary>
    /// <param name="value">Fiscal code string.</param>
    public static explicit operator FiscalCode(string value) => new(value);

    /// <summary>
    /// Implicit conversion to string.
    /// </summary>
    /// <param name="fiscalCode">Fiscal code instance.</param>
    public static implicit operator string(FiscalCode fiscalCode)
    {
        ArgumentNullException.ThrowIfNull(fiscalCode);

        return fiscalCode.Value;
    }

    /// <summary>
    /// Validates a fiscal code.
    /// </summary>
    /// <param name="code">Code to validate.</param>
    /// <returns>
    /// True if valid; otherwise false.
    /// </returns>
    public static bool IsValid(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        var normalized = Normalize(code);

        return normalized.Length switch
        {
            StandardFiscalCodeLength =>
                StandardFiscalCodeRegex.IsMatch(normalized),

            NumericFiscalCodeLength =>
                NumericFiscalCodeRegex.IsMatch(normalized),

            _ => false,
        };
    }

    /// <summary>
    /// Creates a fiscal code instance.
    /// </summary>
    /// <param name="value">Fiscal code value.</param>
    /// <returns>A fiscal code value object.</returns>
    public static FiscalCode Create(string value) => new(value);

    /// <summary>
    /// Converts the fiscal code to string.
    /// </summary>
    /// <returns>The normalized fiscal code.</returns>
    public override string ToString() => Value;

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Normalizes input values.
    /// </summary>
    /// <param name="value">Input value.</param>
    /// <returns>Normalized value.</returns>
    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
}
