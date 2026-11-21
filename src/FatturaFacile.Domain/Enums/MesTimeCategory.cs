// <copyright file="MesTimeCategory.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// What a time interval of a phase measures.
/// </summary>
public enum MesTimeCategory
{
    /// <summary>
    /// Setup or changeover.
    /// </summary>
    Setup = 0,

    /// <summary>
    /// Machining.
    /// </summary>
    Run = 1,

    /// <summary>
    /// Suspension (downtime).
    /// </summary>
    Suspension = 2,
}
