using System.Text.RegularExpressions;
using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.ValueObjects;

public sealed partial class FiscalCode : ValueObject
{
    public string Value { get; }

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

    public static bool IsValid(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        code = code.Trim().ToUpperInvariant();

        // 16 chars standard person or 11 digits entity
        if (code.Length == 16)
        {
            return StandardCfRegex().IsMatch(code);
        }

        return code.Length == 11 && NumericCfRegex().IsMatch(code);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[A-Z]{6}[0-9LMNPQRSTUV]{2}[A-EHLMPR-T][0-9LMNPQRSTUV]{2}[A-Z][0-9LMNPQRSTUV]{3}[A-Z]$")]
    private static partial Regex StandardCfRegex();

    [GeneratedRegex(@"^\d{11}$")]
    private static partial Regex NumericCfRegex();
}
