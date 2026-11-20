// <copyright file="SyncSendOutcome.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Sync;

/// <summary>
/// Result of replaying one write.
/// </summary>
/// <param name="Result">The classified result.</param>
/// <param name="Error">Diagnostic message for failures.</param>
public sealed record SyncSendOutcome(SyncSendResult Result, string? Error = null);
