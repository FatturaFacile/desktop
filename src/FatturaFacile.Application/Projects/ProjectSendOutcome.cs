// <copyright file="ProjectSendOutcome.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Projects;

/// <summary>
/// What happened when an outbox entry was sent to the web-api.
/// </summary>
internal enum ProjectSendOutcome
{
    /// <summary>
    /// The web-api accepted the change.
    /// </summary>
    Sent = 0,

    /// <summary>
    /// The web-api refused the change.
    /// </summary>
    Refused = 1,

    /// <summary>
    /// The change could not be sent and must be tried again later.
    /// </summary>
    Retry = 2,
}
