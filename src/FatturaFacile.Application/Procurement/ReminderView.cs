// <copyright file="ReminderView.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// One reminder sent for an overdue order.
/// </summary>
/// <param name="Id">The reminder identifier.</param>
/// <param name="Level">The escalation level.</param>
/// <param name="Channel">The channel.</param>
/// <param name="Trigger">What triggered it (manual or automatic).</param>
/// <param name="DaysOverdue">The days overdue when it was sent.</param>
/// <param name="SentAt">When it was sent (UTC).</param>
public sealed record ReminderView(
    Guid Id,
    int Level,
    string Channel,
    string Trigger,
    int DaysOverdue,
    DateTime SentAt);
