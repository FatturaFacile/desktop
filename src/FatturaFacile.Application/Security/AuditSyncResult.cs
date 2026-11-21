// <copyright file="AuditSyncResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Security;

/// <summary>
/// Result of one audit queue synchronization.
/// </summary>
/// <param name="Uploaded">Entries acknowledged during the run.</param>
/// <param name="Remaining">Entries still pending.</param>
/// <param name="ChainBroken">Whether the run was refused because the local chain failed verification.</param>
/// <param name="AuthRequired">Whether the user must sign in again.</param>
/// <param name="Error">Diagnostic message of a non-fatal failure, if any.</param>
public sealed record AuditSyncResult(int Uploaded, int Remaining, bool ChainBroken, bool AuthRequired, string? Error);
