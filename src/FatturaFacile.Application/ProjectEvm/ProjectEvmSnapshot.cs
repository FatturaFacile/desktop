// <copyright file="ProjectEvmSnapshot.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectEvm;

/// <summary>
/// Everything the profitability view shows for one project, as last synced.
/// </summary>
/// <param name="ProjectId">The project identifier.</param>
/// <param name="Currency">The currency of the amounts.</param>
/// <param name="SnapshotDate">The date of the server snapshot, or null while the project has none.</param>
/// <param name="Project">The project-level figures, or null while the project has no snapshot.</param>
/// <param name="Reconciliation">The reconciliation with accounting, or null.</param>
/// <param name="Nodes">The top-level WBS nodes.</param>
/// <param name="Waterfall">The profitability waterfall.</param>
/// <param name="Points">The S-curve.</param>
/// <param name="Forecast">The EAC projection.</param>
/// <param name="SyncedAtUtc">When the snapshot was pulled.</param>
public sealed record ProjectEvmSnapshot(
    string ProjectId,
    string Currency,
    DateTime? SnapshotDate,
    ProjectEvmFigures? Project,
    ProjectEvmReconciliation? Reconciliation,
    IReadOnlyList<ProjectEvmNode> Nodes,
    IReadOnlyList<ProjectEvmWaterfallStep> Waterfall,
    IReadOnlyList<ProjectEvmScurvePoint> Points,
    IReadOnlyList<ProjectEvmEacPoint> Forecast,
    DateTime SyncedAtUtc);
