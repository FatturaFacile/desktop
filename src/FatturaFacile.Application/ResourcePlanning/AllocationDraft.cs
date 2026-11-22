// <copyright file="AllocationDraft.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// What the planner entered in the form that creates an allocation; empty fields are null.
/// </summary>
/// <param name="EmployeeId">The employee.</param>
/// <param name="BookingType">The booking type.</param>
/// <param name="ProjectId">The project.</param>
/// <param name="WbsNodeId">The WBS node of the project.</param>
/// <param name="CrmOpportunityId">The opportunity of a soft booking.</param>
/// <param name="StartDate">The first day.</param>
/// <param name="EndDate">The last day.</param>
/// <param name="AllocatedHoursPerDay">The hours per day.</param>
/// <param name="IsBillable">Whether the hours are billable.</param>
/// <param name="Notes">The notes.</param>
/// <param name="AllowOverload">Whether the server may accept an overload.</param>
/// <param name="RequiredSkillIds">The skills the booking needs.</param>
public sealed record AllocationDraft(
    Guid? EmployeeId,
    ResourceBookingType BookingType,
    Guid? ProjectId,
    Guid? WbsNodeId,
    Guid? CrmOpportunityId,
    DateOnly? StartDate,
    DateOnly? EndDate,
    decimal? AllocatedHoursPerDay,
    bool IsBillable,
    string? Notes,
    bool AllowOverload,
    IReadOnlyList<Guid> RequiredSkillIds);
