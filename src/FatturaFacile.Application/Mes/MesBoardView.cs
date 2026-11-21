// <copyright file="MesBoardView.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Mes;

/// <summary>
/// The operator board as the screen shows it: the server's (or the cache's) rows with the offline commands projected onto them.
/// </summary>
/// <param name="WorkCenterId">The work centre.</param>
/// <param name="WorkCenterName">The work centre name.</param>
/// <param name="Rows">The projected rows.</param>
/// <param name="FromCache">Whether the rows come from the local cache because the server was unreachable.</param>
/// <param name="SyncedAtUtc">When the rows were pulled from the server (UTC).</param>
/// <param name="PendingCount">The commands waiting in the outbox.</param>
/// <param name="Conflicts">The commands the server refused.</param>
public sealed record MesBoardView(
    Guid WorkCenterId,
    string WorkCenterName,
    IReadOnlyList<MesPhaseRow> Rows,
    bool FromCache,
    DateTime? SyncedAtUtc,
    int PendingCount,
    IReadOnlyList<MesConflict> Conflicts);
