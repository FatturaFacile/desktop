// <copyright file="ProjectStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// The lifecycle status of a project (Mod 078).
/// </summary>
public enum ProjectStatus
{
    /// <summary>
    /// Being planned.
    /// </summary>
    Planning = 0,

    /// <summary>
    /// In progress.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Paused.
    /// </summary>
    OnHold = 2,

    /// <summary>
    /// Finished.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Cancelled.
    /// </summary>
    Cancelled = 4,
}
