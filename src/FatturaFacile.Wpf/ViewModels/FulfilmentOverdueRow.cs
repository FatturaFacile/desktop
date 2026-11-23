// <copyright file="FulfilmentOverdueRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// An overdue order of the overdue list.
/// </summary>
/// <param name="OrderId">The order.</param>
/// <param name="Number">The order number.</param>
/// <param name="SupplierName">The supplier.</param>
/// <param name="PromisedText">The promised delivery date.</param>
/// <param name="DaysText">The days overdue.</param>
/// <param name="RemindersText">The reminders sent.</param>
/// <param name="LastReminderText">When the last reminder was sent.</param>
/// <param name="NextDueText">The next reminder due, as text.</param>
/// <param name="IsReminderDue">Whether the next reminder is due.</param>
internal sealed record FulfilmentOverdueRow(
    Guid OrderId,
    string Number,
    string SupplierName,
    string PromisedText,
    string DaysText,
    string RemindersText,
    string LastReminderText,
    string NextDueText,
    bool IsReminderDue);
