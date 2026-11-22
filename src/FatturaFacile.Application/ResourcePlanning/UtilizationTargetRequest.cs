// <copyright file="UtilizationTargetRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The body that sets a utilization target (both scopes null is the tenant default).
/// </summary>
/// <param name="DepartmentId">The department id.</param>
/// <param name="CostCenterId">The cost center id.</param>
/// <param name="TargetPercent">The target percent.</param>
public sealed record UtilizationTargetRequest(
    Guid? DepartmentId,
    Guid? CostCenterId,
    double TargetPercent);
