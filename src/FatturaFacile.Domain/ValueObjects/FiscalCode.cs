// <copyright file="FiscalCode.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.RegularExpressions;

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.ValueObjects;

/// <summary>
/// Represents an immutable Italian Tax Identification Code (Codice Fiscale) for natural persons or legal entities.
/// Encapsulates the 16-character alphanumeric code for individuals (encoding surname, name, birth date, gender,
/// cadastral municipality code, and Luhn-variant check character, including omocodia letter substitutions)
/// or the 11-digit numeric code assigned to non-individual legal entities or numerical tax codes under Italian tax regulations.
/// </summary>
public sealed partial class FiscalCode : ValueObject
{

    /// <summary>
    /// Initializes a new instance of the <see cref="FiscalCode"/> class after validating structural and syntax rules.
    /// </summary>
    /// <param name="value">The raw Codice Fiscale string to normalize and validate.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is empty, whitespace,
    /// or does not conform to 16-character alphanumeric or 11-digit Italian tax code specifications.</exception>
    public FiscalCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value: value))
        {
            throw new ArgumentException(
                                        message: "Fiscal code cannot be empty.",
                                        paramName: nameof(value));
        }

        var normalized = value.Trim().ToUpperInvariant();

        if (!IsValid(code: normalized))
        {
            throw new ArgumentException(
                                        message: $"Invalid Italian Fiscal Code format: '{value}'",
                                        paramName: nameof(value));
        }

        Value = normalized;
    }

    /// <summary>
    /// Gets the normalized uppercase Italian Codice Fiscale string (16 alphanumeric characters or 11 numeric digits).
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the compiled regular expression for validating standard 16-character alphanumeric Italian Fiscal Codes with omocodia support.
    /// </summary>
    [GeneratedRegex(
                       pattern: @"^[A-Z]{6}[0-9LMNPQRSTUV]{2}[A-EHLMPR-T][0-9LMNPQRSTUV]{2}[A-Z][0-9LMNPQRSTUV]{3}[A-Z]$",
                       options: RegexOptions.None,
                       matchTimeoutMilliseconds: 2000)]
    private static partial Regex StandardCfRegex { get; }

    /// <summary>
    /// Gets the compiled regular expression for validating 11-digit numeric temporary or entity Fiscal Codes.
    /// </summary>
    [GeneratedRegex(
                       pattern: @"^\d{11}$",
                       options: RegexOptions.None,
                       matchTimeoutMilliseconds: 2000)]
    private static partial Regex NumericCfRegex { get; }

    /// <summary>
    /// Validates whether the specified string conforms to the syntax rules of an Italian Fiscal Code (Codice Fiscale).
    /// Evaluates either the 16-character alphanumeric pattern with standard omocodia character
    /// sets or the 11-digit legal entity tax identifier.
    /// </summary>
    /// <param name="code">The candidate fiscal code string to validate.</param>
    /// <returns><see langword="true"/> if <paramref name="code"/> matches valid
    /// Italian Codice Fiscale formats; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(string code)
    {
        if (string.IsNullOrWhiteSpace(value: code))
        {
            return false;
        }

        code = code.Trim().ToUpperInvariant();

        // 16 chars standard person or 11 digits entity
        return code.Length == 16 ? StandardCfRegex.IsMatch(input: code) : code.Length == 11 && NumericCfRegex.IsMatch(input: code);
    }

    /// <summary>
    /// Returns the normalized fiscal code string.
    /// </summary>
    /// <returns>The underlying 16-character or 11-digit uppercase Codice Fiscale string.</returns>
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
