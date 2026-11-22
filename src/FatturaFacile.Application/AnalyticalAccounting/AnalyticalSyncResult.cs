// <copyright file="AnalyticalSyncResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AnalyticalAccounting;

/// <summary>
/// The outcome of a sync of the analytical cost accounting module.
/// </summary>
/// <param name="Status">How the attempt ended.</param>
/// <param name="SyncedAtUtc">When the cache was refreshed, for a successful sync.</param>
/// <param name="SentOperations">How many queued entries the server accepted.</param>
/// <param name="FailedOperations">How many queued entries the server refused.</param>
/// <param name="Detail">The failure detail, when there is one.</param>
public sealed record AnalyticalSyncResult(
    AnalyticalSyncStatus Status,
    DateTime? SyncedAtUtc = null,
    int SentOperations = 0,
    int FailedOperations = 0,
    string? Detail = null);
