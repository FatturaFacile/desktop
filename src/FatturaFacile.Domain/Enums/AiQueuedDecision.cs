// <copyright file="AiQueuedDecision.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// The decision a user took offline on an AI suggestion, waiting to be pushed to the web-api.
/// </summary>
public enum AiQueuedDecision
{
    /// <summary>
    /// No decision is queued.
    /// </summary>
    None = 0,

    /// <summary>
    /// The suggestion was approved offline.
    /// </summary>
    Approve = 1,

    /// <summary>
    /// The suggestion was rejected offline.
    /// </summary>
    Reject = 2,
}
