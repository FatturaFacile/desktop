// <copyright file="DssSyncResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Dss;

/// <summary>
/// The outcome of a DSS sync attempt.
/// </summary>
/// <param name="Status">How it ended.</param>
/// <param name="SyncedAtUtc">When the snapshot was pulled (only for <see cref="DssSyncStatus.Synced"/>).</param>
/// <param name="Detail">The reason of a failure.</param>
public sealed record DssSyncResult(DssSyncStatus Status, DateTime? SyncedAtUtc = null, string? Detail = null);
