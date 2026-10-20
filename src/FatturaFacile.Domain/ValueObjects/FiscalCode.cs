using System.Text.RegularExpressions;
using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.ValueObjects;

/// <summary>
/// Represents an Italian Fiscal Code (Codice Fiscale) for individuals or legal entities.
/// </summary>
public sealed partial class FiscalCode : ValueObject
{
    /// <summary>
    /// Gets the normalized alphanumeric fiscal code string.
    /// </summary>
    public string Value { get; }

/// <summary>
    /// Initializes a new instance of the <see cref="FiscalCode"/> class.
    /// </summary>
    /// <param name="value">The raw fiscal code input.</param>
    /// <exception cref="ArgumentException">Thrown when the fiscal code format is invalid.</exception>
    public FiscalCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Fiscal code cannot be empty.", nameof(value));
        }

        var normalized = value.Trim().ToUpperInvariant();
        if (!IsValid(normalized))
        {
            throw new ArgumentException($"Invalid Italian Fiscal Code format: '{value}'", nameof(value));
        }

        Value = normalized;
    }

/// <summary>
    /// Validates whether the given string conforms to standard Italian Fiscal Code formats.
    /// </summary>
    /// <param name="code">The fiscal code candidate string.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        code = code.Trim().ToUpperInvariant();

        // 16 chars standard person or 11 digits entity
        return code.Length == 16 ? StandardCfRegex.IsMatch(code) : code.Length == 11 && NumericCfRegex.IsMatch(code);
    }

/// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

/// <inheritdoc/>
    public override string ToString() => Value;

[GeneratedRegex(@"^[A-Z]{6}[0-9LMNPQRSTUV]{2}[A-EHLMPR-T][0-9LMNPQRSTUV]{2}[A-Z][0-9LMNPQRSTUV]{3}[A-Z]$", RegexOptions.None, 2000)]
    private static partial Regex StandardCfRegex { get; }

[GeneratedRegex(@"^\d{11}$", RegexOptions.None, 2000)]
    private static partial Regex NumericCfRegex { get; }
}
