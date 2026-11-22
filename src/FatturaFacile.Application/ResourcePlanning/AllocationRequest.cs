// <copyright file="AllocationRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The body that creates an allocation.
/// </summary>
/// <param name="ProjectId">The project id.</param>
/// <param name="WbsNodeId">The wbs node id.</param>
/// <param name="EmployeeId">The employee id.</param>
/// <param name="RoleId">The role id.</param>
/// <param name="BookingType">The booking type.</param>
/// <param name="StartDate">The start date.</param>
/// <param name="EndDate">The end date.</param>
/// <param name="AllocatedHoursPerDay">The allocated hours per day.</param>
/// <param name="IsBillable">Whether the hours are billable.</param>
/// <param name="CrmOpportunityId">The crm opportunity id.</param>
/// <param name="Notes">The notes.</param>
/// <param name="AllowOverload">Whether the server may accept an overload; false makes it refuse one.</param>
/// <param name="RequiredSkillIds">The skills the booking needs; the server warns about a lapsed or missing skill or certificate. Null leaves the member out.</param>
public sealed record AllocationRequest(
    Guid? ProjectId,
    Guid? WbsNodeId,
    Guid EmployeeId,
    Guid? RoleId,
    ResourceBookingType BookingType,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal AllocatedHoursPerDay,
    bool IsBillable,
    Guid? CrmOpportunityId,
    string? Notes,
    bool AllowOverload,
    IReadOnlyList<Guid>? RequiredSkillIds = null);
