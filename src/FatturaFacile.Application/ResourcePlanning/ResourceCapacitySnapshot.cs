// <copyright file="ResourceCapacitySnapshot.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The stored weekly capacity of an employee.
/// </summary>
/// <param name="Id">The id.</param>
/// <param name="EmployeeId">The employee id.</param>
/// <param name="WeekStartDate">The week start date.</param>
/// <param name="ContractualHours">The contractual hours.</param>
/// <param name="LeaveAndHolidayHours">The leave and holiday hours.</param>
/// <param name="NetAvailableHours">The net available hours.</param>
/// <param name="BillableBookedHours">The billable booked hours.</param>
/// <param name="InternalBookedHours">The internal booked hours.</param>
/// <param name="UtilizationRatePercent">The utilization rate percent.</param>
/// <param name="DepartmentId">The department id.</param>
/// <param name="CostCenterId">The cost center id.</param>
public sealed record ResourceCapacitySnapshot(
    Guid Id,
    Guid EmployeeId,
    DateOnly WeekStartDate,
    decimal ContractualHours,
    decimal LeaveAndHolidayHours,
    decimal NetAvailableHours,
    decimal BillableBookedHours,
    decimal InternalBookedHours,
    double UtilizationRatePercent,
    Guid? DepartmentId,
    Guid? CostCenterId);
