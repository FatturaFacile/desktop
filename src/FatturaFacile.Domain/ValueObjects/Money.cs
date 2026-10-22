// <copyright file="Money.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.ValueObjects;

/// <summary>
/// Represents an immutable monetary amount paired with an ISO currency code, enforcing financial precision rules.
/// Encapsulates statutory half-up rounding to two decimal places (<see cref="MidpointRounding.AwayFromZero"/>),
/// strict ISO currency matching on arithmetic and comparison operations, and Swiss cash rounding (Rappenrundung).
/// </summary>
public sealed class Money : ValueObject, IComparable<Money>, IComparable, IEquatable<Money>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> class with a specified amount and currency.
    /// The amount is automatically rounded to two decimal places using standard financial <see cref="MidpointRounding.AwayFromZero"/>.
    /// </summary>
    /// <param name="amount">The decimal monetary value before rounding.</param>
    /// <param name="currency">The ISO currency specification (e.g., <see cref="Currency.EUR"/> or <see cref="Currency.CHF"/>).</param>
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
    /// <param name="left">The first monetary operand.</param>
    /// <param name="right">The second monetary operand.</param>
    /// <returns>A new <see cref="Money"/> instance representing the sum of both amounts in their shared currency.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="left"/> and <paramref name="right"/> have mismatched currencies.</exception>
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
    /// <param name="left">The minuend monetary value.</param>
    /// <param name="right">The subtrahend monetary value to subtract.</param>
    /// <returns>A new <see cref="Money"/> instance representing the difference in their shared currency.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="left"/> and <paramref name="right"/> have mismatched currencies.</exception>
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
    /// Multiplies the monetary value by a numeric scalar factor, preserving the currency and rounding to two decimals.
    /// </summary>
    /// <param name="left">The base monetary operand.</param>
    /// <param name="multiplier">The scalar factor (e.g., quantity, tax multiplier, or discount coefficient).</param>
    /// <returns>A new scaled <see cref="Money"/> instance.</returns>
    public static Money operator *(
        Money left,
        decimal multiplier) =>
        new(
            (left?.Amount ?? 0) * multiplier,
            left?.Currency ?? Currency.EUR);

    /// <summary>
    /// Determines whether two <see cref="Money"/> instances represent identical amounts and currencies.
    /// </summary>
    /// <param name="left">The first monetary operand to compare.</param>
    /// <param name="right">The second monetary operand to compare.</param>
    /// <returns><see langword="true"/> if both instances have identical currencies and amounts; otherwise, <see langword="false"/>.</returns>
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
    /// Determines whether two <see cref="Money"/> instances differ in amount or currency.
    /// </summary>
    /// <param name="left">The first monetary operand to compare.</param>
    /// <param name="right">The second monetary operand to compare.</param>
    /// <returns><see langword="true"/> if the instances differ in amount or currency; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(
        Money? left,
        Money? right) => !(left == right);

    /// <summary>
    /// Determines whether the left monetary operand is strictly less than the right operand of the same currency.
    /// </summary>
    /// <param name="left">The first monetary operand to evaluate.</param>
    /// <param name="right">The second monetary operand to evaluate.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when non-null instances have different currencies.</exception>
    public static bool operator <(
        Money? left,
        Money? right) =>
        ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;

    /// <summary>
    /// Determines whether the left monetary operand is less than or equal to the right operand of the same currency.
    /// </summary>
    /// <param name="left">The first monetary operand to evaluate.</param>
    /// <param name="right">The second monetary operand to evaluate.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when non-null instances have different currencies.</exception>
    public static bool operator <=(
        Money? left,
        Money? right) =>
        ReferenceEquals(
            left,
            null) || left.CompareTo(right) <= 0;

    /// <summary>
    /// Determines whether the left monetary operand is strictly greater than the right operand of the same currency.
    /// </summary>
    /// <param name="left">The first monetary operand to evaluate.</param>
    /// <param name="right">The second monetary operand to evaluate.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when non-null instances have different currencies.</exception>
    public static bool operator >(
        Money? left,
        Money? right) =>
        !ReferenceEquals(
            left,
            null) && left.CompareTo(right) > 0;

    /// <summary>
    /// Determines whether the left monetary operand is greater than or equal to the right operand of the same currency.
    /// </summary>
    /// <param name="left">The first monetary operand to evaluate.</param>
    /// <param name="right">The second monetary operand to evaluate.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when non-null instances have different currencies.</exception>
    public static bool operator >=(
        Money? left,
        Money? right) =>
        ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;

    /// <summary>
    /// Creates a new <see cref="Money"/> instance in Euros (<see cref="Currency.EUR"/>).
    /// </summary>
    /// <param name="amount">The numeric monetary amount.</param>
    /// <returns>A new <see cref="Money"/> instance denominated in EUR rounded to two decimal places.</returns>
    public static Money Euro(
        decimal amount) => new(
        amount,
        Currency.EUR);

    /// <summary>
    /// Creates a new <see cref="Money"/> instance in Swiss Francs (<see cref="Currency.CHF"/>).
    /// </summary>
    /// <param name="amount">The numeric monetary amount.</param>
    /// <returns>A new <see cref="Money"/> instance denominated in CHF rounded to two decimal places.</returns>
    public static Money Chf(
        decimal amount) => new(
        amount,
        Currency.CHF);

    /// <summary>
    /// Creates a zero monetary value in the designated currency.
    /// </summary>
    /// <param name="currency">The target currency specification.</param>
    /// <returns>A new zero <see cref="Money"/> instance in the given currency.</returns>
    public static Money Zero(
        Currency currency) => new(
        0m,
        currency);

    /// <summary>
    /// Rounds the amount to the nearest Swiss statutory five-centime step (Rappenrundung / Arrondi au 5 centimes).
    /// Used for physical cash settlements and statutory Swiss invoicing compliance.
    /// </summary>
    /// <returns>A new <see cref="Money"/> instance rounded to the nearest CHF 0.05, or the current instance unchanged if not CHF.</returns>
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
    /// Compares the current instance with another strongly typed <see cref="Money"/> instance.
    /// </summary>
    /// <param name="other">The other <see cref="Money"/> instance to compare.</param>
    /// <returns>A negative value if this instance is less than <paramref name="other"/>, zero if equal, or a positive value if greater.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="other"/> has a different currency.</exception>
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

    /// <summary>
    /// Compares the current instance with an arbitrary object.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>A negative value if this instance is less than <paramref name="obj"/>, zero if equal, or a positive value if greater.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="obj"/> is not an instance of <see cref="Money"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="obj"/> has a different currency.</exception>
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

    /// <summary>
    /// Indicates whether the current instance is equal to another <see cref="Money"/> instance.
    /// </summary>
    /// <param name="other">A <see cref="Money"/> instance to compare with this instance.</param>
    /// <returns><see langword="true"/> if both instances share the same <see cref="Currency"/> and <see cref="Amount"/>; otherwise, <see langword="false"/>.</returns>
    public bool Equals(
        Money? other)
    {
        if (other is null)
        {
            return false;
        }

        return Currency == other.Currency && Amount == other.Amount;
    }

    /// <summary>
    /// Determines whether the specified object is a <see cref="Money"/> instance equal to the current instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="Money"/> instance equal to this instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(
        object? obj) => obj is Money other && Equals(other);

    /// <summary>
    /// Returns a hash code calculated from the amount and currency.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(
        Amount,
        Currency);

    /// <summary>
    /// Formats the monetary amount and currency code according to standard financial presentation.
    /// </summary>
    /// <returns>A string representation formatted as "<c>{Amount:N2} {Currency}</c>".</returns>
    public override string ToString() => $"{Amount:N2} {Currency}";

    /// <summary>
    /// Returns the constituent components used for value object equality evaluation.
    /// </summary>
    /// <returns>An enumerable sequence containing <see cref="Amount"/> and <see cref="Currency"/>.</returns>
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
