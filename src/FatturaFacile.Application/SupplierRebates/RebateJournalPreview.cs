// <copyright file="RebateJournalPreview.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// The journal entry an accrual run books: debit accrued income, credit supplier bonus income (the sides swap for a negative
/// accrual). Nothing is booked when the agreement has no accrual and income accounts.
/// </summary>
/// <param name="IsPosted">Whether the run books an entry.</param>
/// <param name="Amount">The absolute amount of both lines.</param>
/// <param name="DebitsAccruedIncome">Whether the accrued income account is debited (positive accrual) or credited.</param>
public sealed record RebateJournalPreview(bool IsPosted, decimal Amount, bool DebitsAccruedIncome);
