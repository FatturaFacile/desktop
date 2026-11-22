// <copyright file="TaskDependency.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A precedence link between two WBS nodes of a project (Mod 078): finish-to-start, start-to-start, finish-to-finish or start-to-finish, with lag or lead.
/// </summary>
public class TaskDependency : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the dependency identifier.
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
    /// Gets or sets the predecessor node identifier.
    /// </summary>
    public Guid PredecessorNodeId { get; set; }

    /// <summary>
    /// Gets or sets the successor node identifier.
    /// </summary>
    public Guid SuccessorNodeId { get; set; }

    /// <summary>
    /// Gets or sets the dependency type.
    /// </summary>
    public DependencyType DependencyType { get; set; }

    /// <summary>
    /// Gets or sets the lag in days (negative for a lead).
    /// </summary>
    public int LagDays { get; set; }

    /// <summary>
    /// Gets or sets the creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
