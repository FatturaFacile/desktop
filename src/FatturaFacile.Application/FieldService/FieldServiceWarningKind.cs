// <copyright file="FieldServiceWarningKind.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Why a work order cannot (or should not) be dispatched to a technician. The names mirror the blocker keys of the web-api
/// (`schedule`, `skills`, `van_stock`).
/// </summary>
public enum FieldServiceWarningKind
{
    /// <summary>
    /// The window overlaps another work order of the technician, or the technician is not available.
    /// </summary>
    Schedule = 0,

    /// <summary>
    /// The technician lacks one or more required skills.
    /// </summary>
    Skills = 1,

    /// <summary>
    /// The technician has no van, or the van lacks the required parts.
    /// </summary>
    VanStock = 2,

    /// <summary>
    /// Any other reason reported by the server.
    /// </summary>
    Other = 3,
}
