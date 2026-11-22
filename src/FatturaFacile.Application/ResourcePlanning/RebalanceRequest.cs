// <copyright file="RebalanceRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The body that rebalances an allocation by shifting, reassigning, splitting or resizing it.
/// </summary>
/// <param name="Strategy">The strategy.</param>
/// <param name="EmployeeId">The colleague that takes the allocation (or its second part), or null.</param>
/// <param name="StartDate">The start date.</param>
/// <param name="EndDate">The end date.</param>
/// <param name="SplitDate">The day the second part starts when splitting.</param>
/// <param name="ShiftDays">The signed number of days to move the allocation.</param>
/// <param name="AllocatedHoursPerDay">The allocated hours per day.</param>
public sealed record RebalanceRequest(
    RebalanceStrategy Strategy,
    Guid? EmployeeId,
    DateOnly? StartDate,
    DateOnly? EndDate,
    DateOnly? SplitDate,
    int? ShiftDays,
    decimal? AllocatedHoursPerDay);
