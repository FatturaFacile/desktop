using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.ValueObjects;

public sealed class Money(decimal amount, Currency currency) : ValueObject, IComparable<Money>, IComparable, IEquatable<Money>
{
    public decimal Amount { get; } = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    public Currency Currency { get; } = currency;

    public static Money Euro(decimal amount) => new(amount, Currency.EUR);
    public static Money Chf(decimal amount) => new(amount, Currency.CHF);
    public static Money Zero(Currency currency) => new(0m, currency);

    /// <summary>
    /// Swiss statutory 0.05 cash rounding rule (Rappenrundung).
    /// </summary>
    /// <returns></returns>
    public Money ToSwissCashRounding()
    {
        if (Currency != Currency.CHF)
        {
            return this;
        }

        // Rounds to nearest 0.05 (multiply by 20, round, divide by 20)
        var rounded = Math.Round(Amount * 20m, 0, MidpointRounding.AwayFromZero) / 20m;
        return new Money(rounded, Currency.CHF);
    }

    public static Money operator +(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount - right.Amount, left.Currency);
    }

    public static Money operator *(Money left, decimal multiplier) => new Money(left.Amount * multiplier, left.Currency);

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException($"Cannot operate on different currencies: {left.Currency} and {right.Currency}");
        }
    }

    public int CompareTo(Money? other)
    {
        if (other is null)
        {
            return 1;
        }

        EnsureSameCurrency(this, other);
        return Amount.CompareTo(other.Amount);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:N2} {Currency}";
    public int CompareTo(object obj)
    {
        if (obj == null)
        {
            return 1;
        }

        if (obj is Money x)
        {
            return CompareTo(x);
        }

        throw new ArgumentException("", nameof(obj));
    }

    public bool Equals(Money other) => CompareTo(other) == 0;

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (ReferenceEquals(obj, null))
        {
            return false;
        }

        throw new NotImplementedException();
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }

    public static bool operator ==(Money left, Money right)
    {
        if (ReferenceEquals(left, null))
        {
            return ReferenceEquals(right, null);
        }

        return left.Equals(right);
    }

    public static bool operator !=(Money left, Money right)
    {
        return !(left == right);
    }

    public static bool operator <(Money left, Money right)
    {
        return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
    }

    public static bool operator <=(Money left, Money right)
    {
        return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
    }

    public static bool operator >(Money left, Money right)
    {
        return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
    }

    public static bool operator >=(Money left, Money right)
    {
        return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
    }
}
