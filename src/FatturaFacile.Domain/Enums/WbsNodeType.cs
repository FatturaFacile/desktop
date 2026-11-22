// <copyright file="WbsNodeType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// The kind of a WBS node (Mod 078).
/// </summary>
public enum WbsNodeType
{
    /// <summary>
    /// A phase grouping deliverables.
    /// </summary>
    Phase = 0,

    /// <summary>
    /// A deliverable grouping work packages.
    /// </summary>
    Deliverable = 1,

    /// <summary>
    /// A leaf work package that carries effort.
    /// </summary>
    WorkPackage = 2,

    /// <summary>
    /// A zero-duration milestone.
    /// </summary>
    Milestone = 3,
}
