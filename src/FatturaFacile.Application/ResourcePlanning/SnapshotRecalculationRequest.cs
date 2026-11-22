// <copyright file="SnapshotRecalculationRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The body that recalculates the capacity snapshots of a window.
/// </summary>
/// <param name="From">The from.</param>
/// <param name="To">The to.</param>
/// <param name="EmployeeId">The employee id.</param>
public sealed record SnapshotRecalculationRequest(
    DateOnly From,
    DateOnly To,
    Guid? EmployeeId);
