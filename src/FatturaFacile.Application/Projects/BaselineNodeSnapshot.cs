// <copyright file="BaselineNodeSnapshot.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Projects;

/// <summary>
/// The frozen figures of one WBS node inside a baseline.
/// </summary>
/// <param name="NodeId">The node identifier.</param>
/// <param name="WbsCode">The WBS code at capture time.</param>
/// <param name="Title">The title at capture time.</param>
/// <param name="NodeType">The node type.</param>
/// <param name="StartDate">The planned start.</param>
/// <param name="EndDate">The planned end.</param>
/// <param name="PlannedHours">The planned hours.</param>
/// <param name="PlannedCost">The planned cost.</param>
/// <param name="PlannedRevenue">The planned revenue.</param>
public sealed record BaselineNodeSnapshot(
    Guid NodeId,
    string WbsCode,
    string Title,
    WbsNodeType NodeType,
    DateOnly? StartDate,
    DateOnly? EndDate,
    decimal PlannedHours,
    decimal PlannedCost,
    decimal PlannedRevenue);
