// <copyright file="FieldServiceBoard.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// The dispatcher Gantt and map board: the unassigned queue and a lane per technician for a time window.
/// </summary>
/// <param name="From">The start of the window.</param>
/// <param name="To">The end of the window.</param>
/// <param name="Unassigned">The work orders waiting for a technician, most urgent first.</param>
/// <param name="Lanes">The technician lanes.</param>
public sealed record FieldServiceBoard(DateTimeOffset From, DateTimeOffset To, IReadOnlyList<FieldServiceBlock> Unassigned, IReadOnlyList<FieldServiceLane> Lanes);
