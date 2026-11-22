// <copyright file="HelpdeskSendOutcome.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// The result of one attempt to send an outbox entry.
/// </summary>
internal enum HelpdeskSendOutcome
{
    /// <summary>
    /// The server accepted the entry.
    /// </summary>
    Sent = 0,

    /// <summary>
    /// The server refused the entry or it ran out of retries.
    /// </summary>
    Refused = 1,

    /// <summary>
    /// The entry stays queued for another attempt.
    /// </summary>
    Retry = 2,
}
