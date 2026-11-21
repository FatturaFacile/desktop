// <copyright file="AiRefreshResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AiCopilot;

/// <summary>
/// Result of refreshing the cache (queued decisions are flushed first).
/// </summary>
/// <param name="Status">The refresh status.</param>
/// <param name="Flush">The result of the queue flush that preceded the refresh.</param>
/// <param name="Snapshot">The cache after the refresh.</param>
public sealed record AiRefreshResult(AiRefreshStatus Status, AiFlushResult Flush, AiCopilotSnapshot Snapshot);
