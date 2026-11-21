// <copyright file="AiFlushResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AiCopilot;

/// <summary>
/// Summary of pushing the queued decisions.
/// </summary>
/// <param name="Sent">Decisions accepted by the server.</param>
/// <param name="Dropped">Decisions dropped because the suggestion was already decided or is gone (409, 404).</param>
/// <param name="Refused">Decisions the server refused (422 and others): the suggestion is pending again.</param>
/// <param name="Remaining">Decisions still queued.</param>
/// <param name="AuthRequired">Whether the user must sign in again.</param>
public sealed record AiFlushResult(int Sent, int Dropped, int Refused, int Remaining, bool AuthRequired)
{
    /// <summary>
    /// Gets a result for a flush that did nothing.
    /// </summary>
    public static AiFlushResult Empty { get; } = new(Sent: 0, Dropped: 0, Refused: 0, Remaining: 0, AuthRequired: false);
}
