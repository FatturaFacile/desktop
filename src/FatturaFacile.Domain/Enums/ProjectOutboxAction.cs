// <copyright file="ProjectOutboxAction.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// The change a queued project outbox entry sends (Mod 078).
/// </summary>
public enum ProjectOutboxAction
{
    /// <summary>
    /// Creates or updates a project.
    /// </summary>
    UpsertProject = 0,

    /// <summary>
    /// Creates or updates a WBS node.
    /// </summary>
    UpsertNode = 1,

    /// <summary>
    /// Deletes a WBS node.
    /// </summary>
    DeleteNode = 2,

    /// <summary>
    /// Creates or updates a dependency.
    /// </summary>
    UpsertDependency = 3,

    /// <summary>
    /// Deletes a dependency.
    /// </summary>
    DeleteDependency = 4,

    /// <summary>
    /// Captures a baseline.
    /// </summary>
    CaptureBaseline = 5,

    /// <summary>
    /// Creates a project from a template.
    /// </summary>
    InstantiateTemplate = 6,
}
