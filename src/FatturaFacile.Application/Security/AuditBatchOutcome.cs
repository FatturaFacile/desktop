// <copyright file="AuditBatchOutcome.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Sync;

namespace FatturaFacile.Application.Security;

/// <summary>
/// Outcome of a batch upload.
/// </summary>
/// <param name="Result">The classified result.</param>
/// <param name="ReceivedAt">The server timestamp (UTC) of the batch, if reported.</param>
/// <param name="Entries">The per-entry acknowledgements, if reported.</param>
/// <param name="Error">Diagnostic message for failures.</param>
public sealed record AuditBatchOutcome(
    SyncSendResult Result,
    DateTime? ReceivedAt = null,
    IReadOnlyList<AuditAcknowledgement>? Entries = null,
    string? Error = null);
