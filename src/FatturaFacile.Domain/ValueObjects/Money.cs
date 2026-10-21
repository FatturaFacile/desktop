// <copyright file="Money.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.ValueObjects;

/// <summary>
/// Represents a monetary value with currency and precision rounding rules.
/// </summary>
public sealed class Money : ValueObject, IComparable<Money>, IComparable, IEquatable<Money>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> class.
    /// </summary>
    /// <param name="amount">The decimal monetary amount.</param>
    /// <param name="currency">The currency specification.</param>
    public Money(
        decimal amount,
        Currency currency)
    {
        Amount = decimal.Round(
            amount,
            2,
            MidpointRounding.AwayFromZero);
        Currency = currency;
    }

    private Money()
    {
        Amount = 0m;
        Currency = Currency.EUR;
    }

    /// <summary>
    /// Gets the monetary amount rounded to two decimal places.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the currency of the monetary amount.
    /// </summary>
    public Currency Currency { get; }

    /// <summary>
    /// Adds two monetary values sharing the same currency.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>The sum of both amounts.</returns>
    /// <exception cref="InvalidOperationException">Thrown when currencies do not match.</exception>
    public static Money operator +(
        Money left,
        Money right)
    {
        if (left != null && right != null)
        {
            EnsureSameCurrency(
                left,
                right);
        }

        return new Money(
            (left?.Amount ?? 0) + (right?.Amount ?? 0),
            left?.Currency ?? Currency.CHF);
    }

    /// <summary>
    /// Subtracts the right monetary value from the left value sharing the same currency.
    /// </summary>
    /// <param name="left">The minuend.</param>
    /// <param name="right">The subtrahend.</param>
    /// <returns>The difference of both amounts.</returns>
    /// <exception cref="InvalidOperationException">Thrown when currencies do not match.</exception>
    public static Money operator -(
        Money left,
        Money right)
    {
        if (left != null && right != null)
        {
            EnsureSameCurrency(
                left,
                right);
        }

        return new Money(
            (left?.Amount ?? 0) - (right?.Amount ?? 0),
            left?.Currency ?? Currency.CHF);
    }

    /// <summary>
    /// Multiplies the monetary value by a numeric scalar factor.
    /// </summary>
    /// <param name="left">The money value.</param>
    /// <param name="multiplier">The multiplier factor.</param>
    /// <returns>A new scaled <see cref="Money"/> instance.</returns>
    public static Money operator *(
        Money left,
        decimal multiplier) =>
        new(
            (left?.Amount ?? 0) * multiplier,
            left?.Currency ?? Currency.EUR);

    /// <summary>
    /// Determines whether two <see cref="Money"/> instances are equal.
    /// </summary>
    /// <param name="left">The left instance.</param>
    /// <param name="right">The right instance.</param>
    /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(
        Money? left,
        Money? right)
    {
        if (ReferenceEquals(
                left,
                null))
        {
            return ReferenceEquals(
                right,
                null);
        }

        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="Money"/> instances are not equal.
    /// </summary>
    /// <param name="left">The left instance.</param>
    /// <param name="right">The right instance.</param>
    /// <returns><see langword="true"/> if not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(
        Money? left,
        Money? right) => !(left == right);

    /// <summary>
    /// Determines whether the left instance is less than the right instance.
    /// </summary>
    /// <param name="left">The left instance.</param>
    /// <param name="right">The right instance.</param>
    /// <returns><see langword="true"/> if left is less than right; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(
        Money? left,
        Money? right) =>
        ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;

    /// <summary>
    /// Determines whether the left instance is less than or equal to the right instance.
    /// </summary>
    /// <param name="left">The left instance.</param>
    /// <param name="right">The right instance.</param>
    /// <returns><see langword="true"/> if left is less than or equal to right; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(
        Money? left,
        Money? right) =>
        ReferenceEquals(
            left,
            null) || left.CompareTo(right) <= 0;

    /// <summary>
    /// Determines whether the left instance is greater than the right instance.
    /// </summary>
    /// <param name="left">The left instance.</param>
    /// <param name="right">The right instance.</param>
    /// <returns><see langword="true"/> if left is greater than right; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(
        Money? left,
        Money? right) =>
        !ReferenceEquals(
            left,
            null) && left.CompareTo(right) > 0;

    /// <summary>
    /// Determines whether the left instance is greater than or equal to the right instance.
    /// </summary>
    /// <param name="left">The left instance.</param>
    /// <param name="right">The right instance.</param>
    /// <returns><see langword="true"/> if left is greater than or equal to right; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(
        Money? left,
        Money? right) =>
        ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;

    /// <summary>
    /// Creates a new <see cref="Money"/> instance in Euros.
    /// </summary>
    /// <param name="amount">The numeric amount.</param>
    /// <returns>A new <see cref="Money"/> object with EUR currency.</returns>
    public static Money Euro(
        decimal amount) => new(
        amount,
        Currency.EUR);

    /// <summary>
    /// Creates a new <see cref="Money"/> instance in Swiss Francs.
    /// </summary>
    /// <param name="amount">The numeric amount.</param>
    /// <returns>A new <see cref="Money"/> object with CHF currency.</returns>
    public static Money Chf(
        decimal amount) => new(
        amount,
        Currency.CHF);

    /// <summary>
    /// Creates a zero monetary value in the designated currency.
    /// </summary>
    /// <param name="currency">The target currency.</param>
    /// <returns>A new zero <see cref="Money"/> instance.</returns>
    public static Money Zero(
        Currency currency) => new(
        0m,
        currency);

    /// <summary>
    /// Rounds the amount to the nearest Swiss statutory five-centime step (Rappenrundung).
    /// </summary>
    /// <returns>A new <see cref="Money"/> instance rounded according to Swiss cash rules, or the current instance if not CHF.</returns>
    public Money ToSwissCashRounding()
    {
        if (Currency != Currency.CHF)
        {
            return this;
        }

        // Rounds to nearest 0.05 (multiply by 20, round, divide by 20)
        var rounded = Math.Round(
            Amount * 20m,
            0,
            MidpointRounding.AwayFromZero) / 20m;
        return new Money(
            rounded,
            Currency.CHF);
    }

    /// <summary>
    /// Compares the current instance with another <see cref="Money"/> object.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A value that indicates the relative order of the amounts.</returns>
    public int CompareTo(
        Money? other)
    {
        if (other is null)
        {
            return 1;
        }

        EnsureSameCurrency(
            this,
            other);
        return Amount.CompareTo(other.Amount);
    }

    /// <inheritdoc/>
    public int CompareTo(
        object? obj)
    {
        if (obj is null)
        {
            return 1;
        }

        if (obj is Money x)
        {
            return CompareTo(x);
        }

        throw new ArgumentException(
            "Object must be of type Money.",
            nameof(obj));
    }

    /// <inheritdoc/>
    public bool Equals(
        Money? other)
    {
        if (other is null)
        {
            return false;
        }

        return Currency == other.Currency && Amount == other.Amount;
    }

    /// <inheritdoc/>
    public override bool Equals(
        object? obj) => obj is Money other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(
        Amount,
        Currency);

    /// <inheritdoc/>
    public override string ToString() => $"{Amount:N2} {Currency}";

    /// <inheritdoc/>
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
