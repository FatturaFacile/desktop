// <copyright file="WbsNode.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A node of the work breakdown structure of a project: phase, deliverable, work package or milestone (Mod 078), with its CPM schedule results.
/// </summary>
public class WbsNode : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the node identifier.
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
    /// Gets or sets the parent node identifier, or null for a top-level node.
    /// </summary>
    public Guid? ParentNodeId { get; set; }

    /// <summary>
    /// Gets or sets the hierarchical WBS code (1, 1.1, 1.1.1).
    /// </summary>
    public string WbsCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the node title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the node type.
    /// </summary>
    public WbsNodeType NodeType { get; set; }

    /// <summary>
    /// Gets or sets the planned hours (rolled up for a summary node).
    /// </summary>
    public decimal PlannedHours { get; set; }

    /// <summary>
    /// Gets or sets the actual hours (rolled up for a summary node).
    /// </summary>
    public decimal ActualHours { get; set; }

    /// <summary>
    /// Gets or sets the planned cost (rolled up for a summary node).
    /// </summary>
    public decimal PlannedCost { get; set; }

    /// <summary>
    /// Gets or sets the actual cost (rolled up for a summary node).
    /// </summary>
    public decimal ActualCost { get; set; }

    /// <summary>
    /// Gets or sets the planned revenue (rolled up for a summary node).
    /// </summary>
    public decimal PlannedRevenue { get; set; }

    /// <summary>
    /// Gets or sets the planned start date.
    /// </summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the planned end date (inclusive).
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Gets or sets the percent complete (0-100).
    /// </summary>
    public decimal PercentComplete { get; set; }

    /// <summary>
    /// Gets or sets the order among the siblings.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the CPM early start.
    /// </summary>
    public DateOnly? EarlyStart { get; set; }

    /// <summary>
    /// Gets or sets the CPM early finish.
    /// </summary>
    public DateOnly? EarlyFinish { get; set; }

    /// <summary>
    /// Gets or sets the CPM late start.
    /// </summary>
    public DateOnly? LateStart { get; set; }

    /// <summary>
    /// Gets or sets the CPM late finish.
    /// </summary>
    public DateOnly? LateFinish { get; set; }

    /// <summary>
    /// Gets or sets the CPM total float in days.
    /// </summary>
    public int? TotalFloatDays { get; set; }

    /// <summary>
    /// Gets or sets the CPM free float in days.
    /// </summary>
    public int? FreeFloatDays { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the node is on the critical path.
    /// </summary>
    public bool IsCritical { get; set; }

    /// <summary>
    /// Gets or sets the creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
