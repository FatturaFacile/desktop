// <copyright file="QueueAnalyticalEntryResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AnalyticalAccounting;

/// <summary>
/// The outcome of queuing a manual analytical entry.
/// </summary>
/// <param name="OperationId">The identifier of the queued operation, or null when the draft was refused.</param>
/// <param name="Errors">The error codes (resource keys) of a refused draft.</param>
public sealed record QueueAnalyticalEntryResult(Guid? OperationId, IReadOnlyList<string> Errors)
{
    /// <summary>
    /// Gets a value indicating whether the entry was queued.
    /// </summary>
    public bool IsQueued => OperationId is not null;
}
