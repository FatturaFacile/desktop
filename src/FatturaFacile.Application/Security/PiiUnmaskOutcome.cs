// <copyright file="PiiUnmaskOutcome.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Sync;

namespace FatturaFacile.Application.Security;

/// <summary>
/// Outcome of an unmask request.
/// </summary>
/// <param name="Result">The classified result.</param>
/// <param name="Value">The revealed value on success.</param>
/// <param name="Error">Diagnostic message for failures.</param>
public sealed record PiiUnmaskOutcome(SyncSendResult Result, string? Value = null, string? Error = null);
