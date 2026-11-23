// <copyright file="RebateAccrualRun.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// A periodic accrual run of a rebate agreement.
/// </summary>
/// <param name="Id">The run identifier.</param>
/// <param name="PeriodStart">The first day of the period.</param>
/// <param name="PeriodEnd">The last day of the period.</param>
/// <param name="Status">The status: posted or reversed.</param>
/// <param name="BaseAmount">The qualifying spend the run is based on.</param>
/// <param name="CumulativeRebate">The rebate earned to the end of the period.</param>
/// <param name="DeltaAmount">The accrual of the period.</param>
/// <param name="JournalEntryId">The journal entry the run booked, or null when no accounts are configured.</param>
/// <param name="ReversalReason">The reason the run was reversed, when it was.</param>
/// <param name="Lines">The invoices that contributed to the run.</param>
public sealed record RebateAccrualRun(
    string Id,
    DateOnly? PeriodStart,
    DateOnly? PeriodEnd,
    string Status,
    decimal BaseAmount,
    decimal CumulativeRebate,
    decimal DeltaAmount,
    string? JournalEntryId,
    string? ReversalReason,
    IReadOnlyList<RebateAccrualLine> Lines)
{
    /// <summary>
    /// Gets a value indicating whether the run is posted (a reversed run cannot be reversed again).
    /// </summary>
    public bool IsPosted => string.Equals(Status, RebateCodes.RunPosted, StringComparison.Ordinal);

    /// <summary>
    /// Gets the journal entry preview of the run for an agreement.
    /// </summary>
    /// <param name="agreement">The agreement the run belongs to.</param>
    /// <returns>The preview.</returns>
    public RebateJournalPreview JournalPreview(RebateAgreement agreement)
    {
        ArgumentNullException.ThrowIfNull(agreement);

        return RebateAccrualCalculator.Preview(DeltaAmount, agreement.HasLedgerAccounts || JournalEntryId is not null);
    }
}
