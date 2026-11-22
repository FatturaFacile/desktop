// <copyright file="ProjectEvmSyncResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectEvm;

/// <summary>
/// The outcome of a profitability sync.
/// </summary>
/// <param name="Status">How the attempt ended.</param>
/// <param name="SyncedAtUtc">When the cache was refreshed, for a successful sync.</param>
/// <param name="Detail">The failure detail, when there is one.</param>
public sealed record ProjectEvmSyncResult(ProjectEvmSyncStatus Status, DateTime? SyncedAtUtc = null, string? Detail = null);
