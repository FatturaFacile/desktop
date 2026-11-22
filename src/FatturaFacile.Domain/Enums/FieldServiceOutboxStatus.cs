// <copyright file="FieldServiceOutboxStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// State of a field service operation in the local outbox.
/// </summary>
public enum FieldServiceOutboxStatus
{
    /// <summary>
    /// Waiting to be sent, or to be retried.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// The server applied it, or recognised its client identifier as already applied.
    /// </summary>
    Sent = 1,

    /// <summary>
    /// The server rejected it; the dispatcher has to look at it.
    /// </summary>
    Conflict = 2,

    /// <summary>
    /// The user dismissed the conflict.
    /// </summary>
    Dismissed = 3,
}
