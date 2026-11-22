// <copyright file="AllocationMutationResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The answer of a create, convert or rebalance: the affected rows, the overload check and the warnings.
/// </summary>
/// <param name="Allocation">The main affected allocation.</param>
/// <param name="Allocations">Every affected allocation (two after a split).</param>
/// <param name="Overload">The overload.</param>
/// <param name="Warnings">The warnings.</param>
public sealed record AllocationMutationResult(
    ResourceAllocation Allocation,
    IReadOnlyList<ResourceAllocation> Allocations,
    OverloadSummary Overload,
    IReadOnlyList<PlanningIssue> Warnings);
