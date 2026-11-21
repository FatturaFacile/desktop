// <copyright file="AuditQueueSummary.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Security;

/// <summary>
/// Counters of the local audit queue.
/// </summary>
/// <param name="Pending">Entries waiting for upload.</param>
/// <param name="Synced">Entries acknowledged by the server.</param>
/// <param name="LastSequence">The highest sequence number (0 when empty).</param>
/// <param name="LastSyncedAt">The last acknowledgement instant (UTC), if any.</param>
public sealed record AuditQueueSummary(int Pending, int Synced, long LastSequence, DateTime? LastSyncedAt);
