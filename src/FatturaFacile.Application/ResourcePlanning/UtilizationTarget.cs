// <copyright file="UtilizationTarget.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// A billable utilization target of a department, cost center or the whole tenant.
/// </summary>
/// <param name="Id">The id.</param>
/// <param name="DepartmentId">The department id.</param>
/// <param name="CostCenterId">The cost center id.</param>
/// <param name="TargetPercent">The target percent.</param>
public sealed record UtilizationTarget(
    Guid Id,
    Guid? DepartmentId,
    Guid? CostCenterId,
    double TargetPercent);
