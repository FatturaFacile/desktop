// <copyright file="ScheduleResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing.Scheduling;

/// <summary>
/// The outcome of a finite-capacity scheduling run.
/// </summary>
/// <param name="Tasks">The scheduled tasks.</param>
/// <param name="LastFinish">The last finish.</param>
public sealed record ScheduleResult(IReadOnlyList<ScheduledTask> Tasks, DateTime? LastFinish);
