// <copyright file="RebalanceDraft.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// What a planner entered to rebalance an allocation; only the fields of the chosen strategy are read.
/// </summary>
/// <param name="Strategy">The strategy.</param>
/// <param name="EmployeeId">The colleague, or null.</param>
/// <param name="StartDate">The new start day, or null.</param>
/// <param name="EndDate">The new end day, or null.</param>
/// <param name="SplitDate">The day the second part starts, or null.</param>
/// <param name="ShiftDays">The signed days to move, or null.</param>
/// <param name="AllocatedHoursPerDay">The new hours per day, or null.</param>
public sealed record RebalanceDraft(
    RebalanceStrategy Strategy,
    Guid? EmployeeId,
    DateOnly? StartDate,
    DateOnly? EndDate,
    DateOnly? SplitDate,
    int? ShiftDays,
    decimal? AllocatedHoursPerDay);
