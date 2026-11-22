// <copyright file="ResourceAllocation.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// A booking of an employee on a project or an opportunity over a period, soft or hard.
/// </summary>
/// <param name="Id">The id.</param>
/// <param name="ProjectId">The project id.</param>
/// <param name="ProjectName">The project name.</param>
/// <param name="WbsNodeId">The wbs node id.</param>
/// <param name="WbsNodeTitle">The wbs node title.</param>
/// <param name="EmployeeId">The employee id.</param>
/// <param name="EmployeeName">The employee name.</param>
/// <param name="RoleId">The role id.</param>
/// <param name="BookingType">The booking type: soft tentative or hard committed.</param>
/// <param name="StartDate">The start date.</param>
/// <param name="EndDate">The end date.</param>
/// <param name="AllocatedHoursPerDay">The allocated hours per day.</param>
/// <param name="TotalAllocatedHours">The total allocated hours.</param>
/// <param name="IsBillable">Whether the hours are billable.</param>
/// <param name="CrmOpportunityId">The crm opportunity id.</param>
/// <param name="CrmOpportunityName">The crm opportunity name.</param>
/// <param name="IsOverloaded">Whether the booking overloads the employee in some week.</param>
/// <param name="PeakLoadPercent">The highest weekly load in percent.</param>
/// <param name="Notes">The notes.</param>
public sealed record ResourceAllocation(
    Guid Id,
    Guid? ProjectId,
    string? ProjectName,
    Guid? WbsNodeId,
    string? WbsNodeTitle,
    Guid EmployeeId,
    string EmployeeName,
    Guid? RoleId,
    ResourceBookingType BookingType,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal AllocatedHoursPerDay,
    decimal TotalAllocatedHours,
    bool IsBillable,
    Guid? CrmOpportunityId,
    string? CrmOpportunityName,
    bool IsOverloaded,
    double PeakLoadPercent,
    string? Notes);
