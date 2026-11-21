// <copyright file="MesSendOutcome.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Mes;

/// <summary>
/// What became of one command of the outbox when it was sent.
/// </summary>
internal enum MesSendOutcome
{
    /// <summary>
    /// The server accepted the command.
    /// </summary>
    Sent = 0,

    /// <summary>
    /// The server refused the command for good.
    /// </summary>
    Refused = 1,

    /// <summary>
    /// The server could not be reached or asked to try again: the command stays pending.
    /// </summary>
    Retry = 2,
}
