// <copyright file="PlanningResource.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// An employee with an active contract that can be planned.
/// </summary>
/// <param name="EmployeeId">The employee id.</param>
/// <param name="FullName">The full name.</param>
/// <param name="DepartmentId">The department id.</param>
/// <param name="DepartmentName">The department name.</param>
/// <param name="CostCenterId">The cost center id.</param>
/// <param name="CostCenterName">The cost center name.</param>
/// <param name="WeeklyContractHours">The weekly contract hours.</param>
/// <param name="FteRatio">The full-time equivalent ratio.</param>
/// <param name="HourlyCost">The hourly cost.</param>
/// <param name="Country">The country code, CH or IT.</param>
/// <param name="Region">The canton or region, or null.</param>
public sealed record PlanningResource(
    Guid EmployeeId,
    string FullName,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? CostCenterId,
    string? CostCenterName,
    decimal WeeklyContractHours,
    decimal FteRatio,
    decimal HourlyCost,
    string Country,
    string? Region);
