// <copyright file="ProjectBaseline.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A frozen copy of the plan of a project (Mod 078), kept to measure schedule and budget variance.
/// </summary>
public class ProjectBaseline : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the baseline identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the progressive baseline number of the project.
    /// </summary>
    public int BaselineNumber { get; set; }

    /// <summary>
    /// Gets or sets the baseline name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the instant (UTC) the baseline was captured.
    /// </summary>
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the frozen WBS snapshot as JSON.
    /// </summary>
    public string SnapshotJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
