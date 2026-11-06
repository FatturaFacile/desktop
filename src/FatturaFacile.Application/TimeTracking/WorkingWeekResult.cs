// <copyright file="WorkingWeekResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.TimeTracking;

/// <summary>
/// The evaluation of one calendar week (Monday to Sunday) inside the period.
/// </summary>
/// <param name="WeekStart">The Monday of the week.</param>
/// <param name="WorkedMinutes">The worked minutes.</param>
/// <param name="LegalMaximumMinutes">The legal maximum weekly working time (45 or 50 hours).</param>
/// <param name="StatutoryOvertimeMinutes">The work beyond the legal maximum (lavoro straordinario, art. 12 LL).</param>
public sealed record WorkingWeekResult(DateTime WeekStart, int WorkedMinutes, int LegalMaximumMinutes, int StatutoryOvertimeMinutes);
