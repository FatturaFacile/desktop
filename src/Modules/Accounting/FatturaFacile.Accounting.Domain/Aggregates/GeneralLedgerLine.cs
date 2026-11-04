// <copyright file="GeneralLedgerLine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Primitives;

namespace FatturaFacile.Accounting.Domain.Aggregates;

/// <summary>
/// Represents an individual debit or credit transaction line within a general ledger accounting entry.
/// </summary>
public sealed class GeneralLedgerLine : Entity<Guid>, IHasTenant
{
    private const int CurrencyDecimalPlaces = 2;
    private const decimal MinimumAmount = 0m;

    private GeneralLedgerLine()
    {
    }

    /// <inheritdoc />
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Gets the foreign key identifier of the parent <see cref="GeneralLedgerEntry" />.
    /// </summary>
    public Guid GeneralLedgerEntryId { get;  }

    /// <summary>
    /// Gets the unique identifier of the charted account affected by this line.
    /// </summary>
    public Guid AccountId { get; private set; }

    /// <summary>
    /// Gets the chart-of-accounts code (e.g., "1020", "2000").
    /// </summary>
    public string AccountCode { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the descriptive narrative for this journal line.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets whether this line represents a debit or credit entry.
    /// </summary>
    public EntryType Type { get; private set; }

    /// <summary>
    /// Gets the line amount rounded to the configured accounting precision.
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Factory method to create a validated <see cref="GeneralLedgerLine" />.
    /// </summary>
    /// <param name="tenantId">The tenant identifier owning this journal entry.</param>
    /// <param name="accountId">The chart-of-accounts identifier.</param>
    /// <param name="accountCode">The charted account code.</param>
    /// <param name="description">The transaction line description.</param>
    /// <param name="type">The entry type (Debit or Credit).</param>
    /// <param name="amount">The monetary line amount, must be positive.</param>
    /// <returns>A new <see cref="GeneralLedgerLine" /> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the amount is zero or negative.
    /// </exception>
    public static GeneralLedgerLine Create(
        Guid tenantId,
        Guid accountId,
        string accountCode,
        string description,
        EntryType type,
        decimal amount)
    {
        if (amount <= MinimumAmount)
        {
            throw new ArgumentException(
                "Amount must be positive.",
                nameof(amount));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(accountCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new GeneralLedgerLine
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AccountId = accountId,
            AccountCode = accountCode.Trim(),
            Description = description.Trim(),
            Type = type,
            Amount = decimal.Round(
                amount,
                CurrencyDecimalPlaces,
                MidpointRounding.AwayFromZero),
        };
    }
}
