// <copyright file="OverdueOrderView.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// An open order past its promised delivery date.
/// </summary>
/// <param name="Order">The order.</param>
/// <param name="DaysOverdue">The days past the date.</param>
/// <param name="OverdueLineIds">The late lines.</param>
/// <param name="ReminderCount">The reminders sent so far.</param>
/// <param name="LastReminderAt">When the last reminder was sent (UTC).</param>
/// <param name="NextReminderDue">Whether the next reminder is due.</param>
public sealed record OverdueOrderView(
    PurchaseOrderView Order,
    int DaysOverdue,
    IReadOnlyList<Guid> OverdueLineIds,
    int ReminderCount,
    DateTime? LastReminderAt,
    bool NextReminderDue);
