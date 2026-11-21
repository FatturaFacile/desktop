// <copyright file="MesExecuteStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Mes;

/// <summary>
/// What became of a command the operator issued.
/// </summary>
public enum MesExecuteStatus
{
    /// <summary>
    /// The command is invalid in the phase's state or has invalid data; nothing was saved.
    /// </summary>
    Rejected = 0,

    /// <summary>
    /// The command is saved in the outbox and will be sent when the server is reachable.
    /// </summary>
    Queued = 1,

    /// <summary>
    /// The server accepted the command.
    /// </summary>
    Sent = 2,

    /// <summary>
    /// The server refused the command.
    /// </summary>
    Conflict = 3,
}
