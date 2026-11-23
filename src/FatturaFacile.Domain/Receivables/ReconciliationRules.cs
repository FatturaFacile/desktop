// <copyright file="ReconciliationRules.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// The rules of the reconciliation workflow: partial or full settlement of an item by payment, credit note, discount or
/// write-off, or a debit adjustment that raises the open amount.
/// </summary>
public static class ReconciliationRules
{
    /// <summary>
    /// The localization key of an amount that is not positive.
    /// </summary>
    public const string AmountKey = "Receivables_Err_Amount";

    /// <summary>
    /// The localization key of an amount over the open amount.
    /// </summary>
    public const string ExceedsKey = "Receivables_Err_Exceeds";

    /// <summary>
    /// The localization key of an item with nothing left to settle.
    /// </summary>
    public const string ClosedKey = "Receivables_Err_Closed";

    /// <summary>
    /// The localization key of an amount over what the chosen payment still has.
    /// </summary>
    public const string PaymentExceedsKey = "Receivables_Err_PaymentExceeds";

    /// <summary>
    /// Validates an allocation against an item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="type">The kind of allocation.</param>
    /// <param name="amount">The amount.</param>
    /// <param name="payment">The unreconciled payment the amount is taken from, when any.</param>
    /// <returns>The localization key of the problem, or null when the allocation is acceptable.</returns>
    public static string? Validate(ReceivableItem item, AllocationType type, decimal amount, UnreconciledPayment? payment = null)
    {
        ArgumentNullException.ThrowIfNull(item);

        return amount <= 0m ? AmountKey : CheckKind(item, type, amount, payment);
    }

    /// <summary>
    /// Previews an item after an allocation.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="type">The kind of allocation.</param>
    /// <param name="amount">The amount.</param>
    /// <returns>The open amount and the status after the allocation.</returns>
    public static ReconciliationPreview Preview(ReceivableItem item, AllocationType type, decimal amount)
    {
        ArgumentNullException.ThrowIfNull(item);

        var open = ReceivablesMath.Round(type == AllocationType.DebitAdjustment ? item.OpenAmount + amount : Math.Max(0m, item.OpenAmount - amount));

        return new ReconciliationPreview(Math.Max(0m, open), open <= 0m ? ClosedStatus(type) : OpenStatus(item, open));
    }

    private static string? CheckKind(ReceivableItem item, AllocationType type, decimal amount, UnreconciledPayment? payment) =>
        type == AllocationType.DebitAdjustment ? null : CheckCredit(item, type, amount, payment);

    private static string? CheckCredit(ReceivableItem item, AllocationType type, decimal amount, UnreconciledPayment? payment) =>
        !item.IsOpen ? ClosedKey : CheckAmount(item, type, amount, payment);

    private static string? CheckAmount(ReceivableItem item, AllocationType type, decimal amount, UnreconciledPayment? payment) =>
        amount > item.OpenAmount ? ExceedsKey : CheckPayment(type, amount, payment);

    private static string? CheckPayment(AllocationType type, decimal amount, UnreconciledPayment? payment) =>
        type == AllocationType.Payment && payment is not null && amount > payment.Unallocated ? PaymentExceedsKey : null;

    private static ReceivableStatus ClosedStatus(AllocationType type) => type == AllocationType.WriteOff ? ReceivableStatus.WrittenOff : ReceivableStatus.Paid;

    private static ReceivableStatus OpenStatus(ReceivableItem item, decimal open) => item.IsDisputed ? ReceivableStatus.Disputed : PaidStatus(open, item.OriginalAmount);

    private static ReceivableStatus PaidStatus(decimal open, decimal original) => open < original ? ReceivableStatus.PartiallyPaid : ReceivableStatus.Open;
}
