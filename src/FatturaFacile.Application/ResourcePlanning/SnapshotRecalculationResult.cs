// <copyright file="SnapshotRecalculationResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The outcome of a snapshot recalculation.
/// </summary>
/// <param name="Weeks">The number of weeks recalculated.</param>
/// <param name="Snapshots">The number of snapshots written.</param>
public sealed record SnapshotRecalculationResult(
    int Weeks,
    int Snapshots);
