// <copyright file="ReceivablesFlushResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// The outcome of sending the queued receivables actions.
/// </summary>
/// <param name="Sent">The actions the server accepted.</param>
/// <param name="Conflicts">The actions the server refused.</param>
/// <param name="Remaining">The actions still waiting.</param>
/// <param name="Offline">Whether the server could not be reached (or the session is missing).</param>
public sealed record ReceivablesFlushResult(
    int Sent,
    int Conflicts,
    int Remaining,
    bool Offline);
