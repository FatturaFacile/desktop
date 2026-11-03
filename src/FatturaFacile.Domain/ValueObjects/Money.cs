// <copyright file="Money.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.ValueObjects;

/// <summary>
/// Represents an immutable monetary amount paired with an ISO currency.
/// </summary>
public sealed class Money : ValueObject, IComparable<Money>, IComparable, IEquatable<Money>
{
    private const int AmountPrecision = 2;
    private const int ZeroPrecision = 0;

    private const decimal ZeroAmount = 0m;
    private const decimal SwissCashFactor = 20m;

    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> class.
    /// </summary>
    /// <param name="amount">Monetary amount.</param>
    /// <param name="currency">Currency.</param>
    public Money(
        decimal amount,
        Currency currency)
    {
        Amount = decimal.Round(
            amount,
            AmountPrecision,
            MidpointRounding.AwayFromZero);

        Currency = currency;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> class.
    /// Required by EF Core.
    /// </summary>
    private Money()
    {
        Amount = ZeroAmount;
        Currency = Currency.EUR;
    }

    /// <summary>
    /// Gets the monetary amount.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the currency.
    /// </summary>
    public Currency Currency { get; }

    /// <summary>
    /// Adds two <see cref="Money"/> instances with the same currency, returning a new <see cref="Money"/> instance.
    /// </summary>
    /// <param name="left">The first <see cref="Money"/> instance to add.</param>
    /// <param name="right">The second <see cref="Money"/> instance to add.</param>
    /// <returns>A new <see cref="Money"/> instance representing the sum of the two instances.</returns>
    public static Money operator +(
        Money left,
        Money right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        EnsureSameCurrency(left, right);

        return new Money(
            left.Amount + right.Amount,
            left.Currency);
    }

    /// <summary>
    /// Subtracts the second <see cref="Money"/> instance from the first, returning a new <see cref="Money"/> instance with the same currency.
    /// </summary>
    /// <param name="left">The first <see cref="Money"/> instance.</param>
    /// <param name="right">The second <see cref="Money"/> instance to subtract.</param>
    /// <returns>A new <see cref="Money"/> instance representing the difference between the two instances.</returns>
    public static Money operator -(
        Money left,
        Money right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        EnsureSameCurrency(left, right);

        return new Money(
            left.Amount - right.Amount,
            left.Currency);
    }

    /// <summary>
    /// Multiplies a monetary amount by a decimal multiplier, returning a new <see cref="Money"/> instance with the same currency.
    /// </summary>
    /// <param name="value">The monetary amount to multiply.</param>
    /// <param name="multiplier">The decimal multiplier.</param>
    /// <returns>A new <see cref="Money"/> instance with the multiplied amount and the same currency.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="value"/> is null.</exception>
    public static Money operator *(
        Money value,
        decimal multiplier)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new Money(
            value.Amount * multiplier,
            value.Currency);
    }

    /// <summary>
    /// Compares two <see cref="Money"/> instances for equality, returning true if both are null or if they have the same amount and currency.
    /// </summary>
    /// <param name="left">The first <see cref="Money"/> instance to compare.</param>
    /// <param name="right">The second <see cref="Money"/> instance to compare.</param>
    /// <returns>True if both instances are equal; otherwise, false.</returns>
    public static bool operator ==(
        Money? left,
        Money? right) =>
        Equals(left, right);

    /// <summary>
    /// Compares two <see cref="Money"/> instances for inequality, returning true if they are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="Money"/> instance to compare.</param>
    /// <param name="right">The second <see cref="Money"/> instance to compare.</param>
    /// <returns>True if both instances are not equal; otherwise, false.</returns>
    public static bool operator !=(
        Money? left,
        Money? right) =>
        !(left == right);

    /// <summary>
    /// Compares two <see cref="Money"/> instances, returning true if the left instance is less than the right instance.
    /// </summary>
    /// <param name="left">The first <see cref="Money"/> instance to compare.</param>
    /// <param name="right">The second <see cref="Money"/> instance to compare.</param>
    /// <returns>True if the left instance is less than the right instance; otherwise, false.</returns>
    public static bool operator <(
        Money? left,
        Money? right)
    {
        return left is null
            ? right is not null
            : left.CompareTo(right) < 0;
    }

    /// <summary>
    /// Compares two <see cref="Money"/> instances, returning true if the left instance is less than or equal to the right instance.
    /// </summary>
    /// <param name="left">The first <see cref="Money"/> instance to compare.</param>
    /// <param name="right">The second <see cref="Money"/> instance to compare.</param>
    /// <returns>True if the left instance is less than or equal to the right instance; otherwise, false.</returns>
    public static bool operator <=(
        Money? left,
        Money? right) =>
        left is null || left.CompareTo(right) <= 0;

    /// <summary>
    /// Compares two <see cref="Money"/> instances, returning true if the left instance is greater than the right instance.
    /// </summary>
    /// <param name="left">The first <see cref="Money"/> instance to compare.</param>
    /// <param name="right">The second <see cref="Money"/> instance to compare.</param>
    /// <returns>True if the left instance is greater than the right instance; otherwise, false.</returns>
    public static bool operator >(
        Money? left,
        Money? right) =>
        left?.CompareTo(right) > 0;

    /// <summary>
    /// Compares two <see cref="Money"/> instances, returning true if the left instance is greater than or equal to the right instance.
    /// </summary>
    /// <param name="left">The first <see cref="Money"/> instance to compare.</param>
    /// <param name="right">The second <see cref="Money"/> instance to compare.</param>
    /// <returns>True if the left instance is greater than or equal to the right instance; otherwise, false.</returns>
    public static bool operator >=(
        Money? left,
        Money? right)
    {
        return left is null
            ? right is null
            : left.CompareTo(right) >= 0;
    }

    /// <summary>
    /// Creates a new <see cref="Money"/> instance representing the specified amount in euros.
    /// </summary>
    /// <param name="amount">The amount of money in euros.</param>
    /// <returns>A new <see cref="Money"/> instance representing the specified amount in euros.</returns>
    public static Money Euro(decimal amount)
    {
        return new Money(
            amount,
            Currency.EUR);
    }

    /// <summary>
    /// Creates a new <see cref="Money"/> instance representing the specified amount in Swiss francs.
    /// </summary>
    /// <param name="amount">The amount of money in Swiss francs.</param>
    /// <returns>A new <see cref="Money"/> instance representing the specified amount in Swiss francs.</returns>
    public static Money Chf(decimal amount)
    {
        return new Money(
            amount,
            Currency.CHF);
    }

    /// <summary>
    /// Creates a new <see cref="Money"/> instance representing a zero amount in the specified currency.
    /// </summary>
    /// <param name="currency">The currency of the zero amount.</param>
    /// <returns>A new <see cref="Money"/> instance representing a zero amount in the specified currency.</returns>
    public static Money Zero(Currency currency)
    {
        return new Money(
            ZeroAmount,
            currency);
    }

    /// <summary>
    /// Applies Swiss 5-rappen cash rounding.
    /// </summary>
    /// <returns>A rounded value.</returns>
    public Money ToSwissCashRounding()
    {
        if (Currency != Currency.CHF)
        {
            return this;
        }

        var rounded = Math.Round(
            Amount * SwissCashFactor,
            ZeroPrecision,
            MidpointRounding.AwayFromZero)
            / SwissCashFactor;

        return new Money(
            rounded,
            Currency.CHF);
    }

    /// <summary>
    /// Compares the current <see cref="Money"/> instance with another <see cref="Money"/> instance, returning an integer that indicates their relative order.
    /// </summary>
    /// <param name="other">The other <see cref="Money"/> instance to compare.</param>
    /// <returns>
    /// A value less than zero if the current instance is less than <paramref name="other"/>.
    /// Zero if the current instance is equal to <paramref name="other"/>.
    /// A value greater than zero if the current instance is greater than <paramref name="other"/>.
    /// </returns>
    public int CompareTo(Money? other)
    {
        if (other is null)
        {
            return 1;
        }

        EnsureSameCurrency(this, other);

        return Amount.CompareTo(other.Amount);
    }

    /// <summary>
    /// Compares the current <see cref="Money"/> instance with another object, returning an integer that indicates their relative order.
    /// </summary>
    /// <param name="obj">The object to compare with the current <see cref="Money"/> instance.</param>
    /// <returns>
    /// A value less than zero if the current instance is less than <paramref name="obj"/>.
    /// Zero if the current instance is equal to <paramref name="obj"/>.
    /// A value greater than zero if the current instance is greater than <paramref name="obj"/>.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="obj"/> is not of type <see cref="Money"/>.</exception>
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            not Money => throw new ArgumentException(
                "Object must be of type Money.",
                nameof(obj)),
            _ => CompareTo((Money?)obj)
        };
    }

    /// <summary>
    /// Determines whether the current <see cref="Money"/> instance is equal to another <see cref="Money"/> instance.
    /// </summary>
    /// <param name="other">The other <see cref="Money"/> instance to compare.</param>
    /// <returns><see langword="true"/> if the current instance is equal to the <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
    public bool Equals(Money? other)
    {
        return other is not null
               && Amount == other.Amount
               && Currency == other.Currency;
    }

    /// <summary>
    /// Determines whether the current <see cref="Money"/> instance is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare with the current <see cref="Money"/> instance.</param>
    /// <returns><see langword="true"/> if the current instance is equal to the <paramref name="obj"/>; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is Money other
               && Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current <see cref="Money"/> instance, based on its amount and currency.
    /// </summary>
    /// <returns>A hash code for the current <see cref="Money"/> instance.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            Amount,
            Currency);
    }

    /// <summary>
    /// Returns the standard string representation of the monetary amount and currency, formatted to two decimal places.
    /// </summary>
    /// <returns>A string representation of the current <see cref="Money"/> instance.</returns>
    public override string ToString() => $"{Amount:N2} {Currency}";

    /// <summary>
    /// Returns the constituent components used for value object equality evaluation.
    /// </summary>
    /// <returns>An enumeration of the components that contribute to the equality of the value object.</returns>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    private static void EnsureSameCurrency(
        Money left,
        Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException(
                $"Cannot operate on different currencies: {left.Currency} and {right.Currency}");
        }
    }
}
