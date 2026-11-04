// <copyright file="Money.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Shared.ValueObjects;

/// <summary>
/// Represents an immutable monetary amount associated with a specific 3-letter ISO currency code.
/// </summary>
public sealed record Money
{
    private const int CurrencyDecimalPlaces = 2;

    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> class.
    /// Initializes a new instance of the <see cref="Money"/> record with validated amount and ISO currency code.
    /// </summary>
    /// <param name="amount">The monetary amount, rounded to two decimal places.</param>
    /// <param name="currency">The 3-letter ISO currency code.</param>
    /// <exception cref="ArgumentException">Thrown when currency is null, empty, or not exactly 3 characters.</exception>
    public Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
        {
            throw new ArgumentException("Currency code must be a valid 3-letter ISO code.", nameof(currency));
        }

        Amount = decimal.Round(amount, CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
        Currency = currency.ToUpperInvariant();
    }

    /// <summary>
    /// Gets the numerical monetary amount rounded to two decimal places.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the uppercase 3-letter ISO 4217 currency code (e.g., "EUR", "CHF", "USD").
    /// </summary>
    public string Currency { get; }

    /// <summary>
    /// Creates a new zero-value <see cref="Money"/> instance for the specified currency.
    /// </summary>
    /// <param name="currency">The 3-letter ISO currency code (defaults to "EUR").</param>
    /// <returns>A new <see cref="Money"/> object with an amount of 0.00.</returns>
    public static Money Zero(string currency = "EUR") => new(0m, currency);

    /// <summary>
    /// Adds another monetary amount of the same currency to this instance.
    /// </summary>
    /// <param name="other">The other monetary amount to add.</param>
    /// <returns>A new <see cref="Money"/> instance representing the sum.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="other"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when attempting to add money of a different currency.</exception>
    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtracts another monetary amount of the same currency from this instance.
    /// </summary>
    /// <param name="other">The other monetary amount to subtract.</param>
    /// <returns>A new <see cref="Money"/> instance representing the difference.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="other"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when attempting to subtract money of a different currency.</exception>
    public Money Subtract(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    /// <summary>
    /// Formats the monetary amount and currency code as a human-readable string.
    /// </summary>
    /// <returns>A formatted string such as "120.50 EUR".</returns>
    public override string ToString() => $"{Amount:0.00} {Currency}";

    private void EnsureSameCurrency(Money other)
    {
        if (!string.Equals(Currency, other.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Cannot operate on amounts with different currencies: {Currency} vs {other.Currency}");
        }
    }
}
