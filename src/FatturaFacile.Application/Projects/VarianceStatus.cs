// <copyright file="VarianceStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Projects;

/// <summary>
/// How a WBS node compares with a baseline.
/// </summary>
public enum VarianceStatus
{
    /// <summary>
    /// The node did not change.
    /// </summary>
    Unchanged = 0,

    /// <summary>
    /// The node changed dates or figures.
    /// </summary>
    Changed = 1,

    /// <summary>
    /// The node did not exist in the baseline.
    /// </summary>
    Added = 2,

    /// <summary>
    /// The node was deleted after the baseline.
    /// </summary>
    Removed = 3,
}
