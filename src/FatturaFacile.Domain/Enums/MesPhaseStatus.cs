// <copyright file="MesPhaseStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Lifecycle state of a phase (Italian: fase di lavorazione) on the MES operator board.
/// </summary>
public enum MesPhaseStatus
{
    /// <summary>
    /// Nobody took the operation over yet, so it has no phase on the server.
    /// </summary>
    Available = 0,

    /// <summary>
    /// An operator took the phase over (presa in carico) and work has not started.
    /// </summary>
    Assigned = 1,

    /// <summary>
    /// The setup or changeover is running.
    /// </summary>
    Setup = 2,

    /// <summary>
    /// The machining is running, attended or unattended.
    /// </summary>
    Running = 3,

    /// <summary>
    /// The phase is suspended with a reason code; the time is downtime.
    /// </summary>
    Suspended = 4,

    /// <summary>
    /// The phase is closed and the operation confirmed.
    /// </summary>
    Closed = 5,
}
