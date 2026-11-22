// <copyright file="HeatmapRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The capacity cells of one employee.
/// </summary>
/// <param name="EmployeeId">The employee id.</param>
/// <param name="EmployeeName">The employee name.</param>
/// <param name="DepartmentId">The department id.</param>
/// <param name="DepartmentName">The department name.</param>
/// <param name="CostCenterId">The cost center id.</param>
/// <param name="WeeklyContractHours">The weekly contract hours.</param>
/// <param name="Cells">The cells in the order of the periods.</param>
public sealed record HeatmapRow(
    Guid EmployeeId,
    string EmployeeName,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? CostCenterId,
    decimal WeeklyContractHours,
    IReadOnlyList<HeatmapCell> Cells);
