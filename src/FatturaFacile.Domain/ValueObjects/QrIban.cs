using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.ValueObjects;

/// <summary>
/// Swiss QR-IBAN. Must start with CH or LI, length 21, and have an IID between 30000 and 31999.
/// </summary>
public sealed class QrIban : ValueObject
{
    public string Value { get; }

    public QrIban(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("QR-IBAN cannot be empty.", nameof(value));
        }

        var normalized = value.Replace(" ", string.Empty, StringComparison.Ordinal).Trim().ToUpperInvariant();
        if (!IsValid(normalized))
        {
            throw new ArgumentException($"Invalid Swiss/Liechtenstein QR-IBAN '{value}'. Must have 21 characters and QR-IID between 30000 and 31999.", nameof(value));
        }

        Value = normalized;
    }

    public static bool IsValid(string iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
        {
            return false;
        }

        var clean = iban.Replace(" ", string.Empty, StringComparison.Ordinal).Trim().ToUpperInvariant();

        if (clean.Length != 21)
        {
            return false;
        }

        if (!clean.StartsWith("CH", StringComparison.Ordinal) && !clean.StartsWith("LI", StringComparison.Ordinal))
        {
            return false;
        }

        // Characters 5-9 represent the QR-IID
        var iidString = clean.Substring(4, 5);
        return int.TryParse(iidString, System.Globalization.CultureInfo.InvariantCulture, out var iid) && iid is >= 30000 and <= 31999;
    }

    public string ToFormattedString()
    {
        // Format as groups of 4: CHxx xxxx xxxx xxxx xxxx x
        return string.Join(' ', Enumerable.Range(0, (Value.Length + 3) / 4)
            .Select(i => Value.Substring(i * 4, Math.Min(4, Value.Length - (i * 4)))));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
