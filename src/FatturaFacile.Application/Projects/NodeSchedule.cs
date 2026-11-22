// <copyright file="NodeSchedule.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Projects;

/// <summary>
/// The CPM result of one WBS node. Finish dates are inclusive (the last working day); a milestone starts and finishes on the same day.
/// </summary>
/// <param name="NodeId">The node identifier.</param>
/// <param name="EarlyStart">The earliest start.</param>
/// <param name="EarlyFinish">The earliest finish.</param>
/// <param name="LateStart">The latest start that does not delay the project.</param>
/// <param name="LateFinish">The latest finish that does not delay the project.</param>
/// <param name="TotalFloatDays">The days the node can slip without delaying the project.</param>
/// <param name="FreeFloatDays">The days the node can slip without delaying any successor.</param>
/// <param name="IsCritical">Whether the node has no total float.</param>
public sealed record NodeSchedule(
    Guid NodeId,
    DateOnly EarlyStart,
    DateOnly EarlyFinish,
    DateOnly LateStart,
    DateOnly LateFinish,
    int TotalFloatDays,
    int FreeFloatDays,
    bool IsCritical);
