// <copyright file="DependencyType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// The precedence relation between two WBS nodes (Mod 078).
/// </summary>
public enum DependencyType
{
    /// <summary>
    /// Finish-to-start.
    /// </summary>
    FS = 0,

    /// <summary>
    /// Start-to-start.
    /// </summary>
    SS = 1,

    /// <summary>
    /// Finish-to-finish.
    /// </summary>
    FF = 2,

    /// <summary>
    /// Start-to-finish.
    /// </summary>
    SF = 3,
}
