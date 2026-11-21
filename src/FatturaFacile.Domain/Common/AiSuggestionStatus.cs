// <copyright file="AiSuggestionStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Status values of an AI suggestion as the web-api reports them.
/// </summary>
public static class AiSuggestionStatus
{
    /// <summary>
    /// Waiting for a human decision.
    /// </summary>
    public const string Pending = "pending";

    /// <summary>
    /// The execution of the proposal failed; the item is back in the open queue.
    /// </summary>
    public const string Failed = "failed";

    /// <summary>
    /// Determines whether a status belongs to the open queue (pending and failed).
    /// </summary>
    /// <param name="status">The status value.</param>
    /// <returns>True when the suggestion still needs a decision.</returns>
    public static bool IsOpen(string? status) =>
        string.Equals(status, Pending, StringComparison.Ordinal) || string.Equals(status, Failed, StringComparison.Ordinal);
}
