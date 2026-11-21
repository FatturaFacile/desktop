// <copyright file="MesMachineMode.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Whether the operator attends the machine while it runs: attended runs count as man time and machine time, unattended
/// (automatic) runs as machine time only.
/// </summary>
public enum MesMachineMode
{
    /// <summary>
    /// The operator attends the machine.
    /// </summary>
    Attended = 0,

    /// <summary>
    /// The machine runs by itself.
    /// </summary>
    Unattended = 1,
}
