// <copyright file="ProjectTemplateInstantiator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Projects;

/// <summary>
/// Creates a project from a template or from any existing project (#186): the WBS and the dependencies are copied with new
/// identifiers, every date shifts so the earliest one lands on the requested start, and progress and actuals start from zero.
/// </summary>
public static class ProjectTemplateInstantiator
{
    /// <summary>
    /// Builds the new project, its nodes and its dependencies.
    /// </summary>
    /// <param name="template">The template project.</param>
    /// <param name="nodes">The nodes of the template.</param>
    /// <param name="dependencies">The dependencies of the template.</param>
    /// <param name="request">The choices of the user.</param>
    /// <param name="tenantId">The tenant that owns the new project.</param>
    /// <returns>The new plan; call the roll-up and the critical path on it before saving.</returns>
    public static ProjectInstance Instantiate(
        Project template,
        IReadOnlyList<WbsNode> nodes,
        IReadOnlyList<TaskDependency> dependencies,
        TemplateInstantiationRequest request,
        Guid tenantId)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(dependencies);
        ArgumentNullException.ThrowIfNull(request);

        var anchor = template.StartDate ?? nodes.Where(node => node.StartDate.HasValue).Select(node => node.StartDate!.Value).DefaultIfEmpty(request.StartDate).Min();
        var shift = request.StartDate.DayNumber - anchor.DayNumber;
        var now = DateTime.UtcNow;

        var project = new Project
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            LegalEntityId = request.LegalEntityId ?? template.LegalEntityId,
            ProjectCode = request.ProjectCode.Trim(),
            Name = request.Name.Trim(),
            CustomerId = request.CustomerId ?? template.CustomerId,
            ProjectManagerId = request.ProjectManagerId ?? template.ProjectManagerId,
            ProjectType = template.ProjectType,
            BillingModel = template.BillingModel,
            CostCenterId = template.CostCenterId,
            StartDate = request.StartDate,
            TargetEndDate = Shift(template.TargetEndDate, shift),
            Status = ProjectStatus.Planning,
            CupCode = Clean(request.CupCode),
            CigCode = Clean(request.CigCode),
            IsTemplate = false,
            TemplateSourceId = template.Id,
            CreatedAt = now,
        };

        var map = nodes.ToDictionary(node => node.Id, _ => Guid.NewGuid());
        var copies = nodes
            .Select(node => new WbsNode
            {
                Id = map[node.Id],
                TenantId = tenantId,
                ProjectId = project.Id,
                ParentNodeId = node.ParentNodeId is { } parent && map.TryGetValue(parent, out var mapped) ? mapped : null,
                WbsCode = node.WbsCode,
                Title = node.Title,
                NodeType = node.NodeType,
                PlannedHours = node.PlannedHours,
                PlannedCost = node.PlannedCost,
                PlannedRevenue = node.PlannedRevenue,
                StartDate = Shift(node.StartDate, shift),
                EndDate = Shift(node.EndDate, shift),
                SortOrder = node.SortOrder,
                CreatedAt = now,
            })
            .ToList();

        var links = dependencies
            .Where(link => map.ContainsKey(link.PredecessorNodeId) && map.ContainsKey(link.SuccessorNodeId))
            .Select(link => new TaskDependency
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProjectId = project.Id,
                PredecessorNodeId = map[link.PredecessorNodeId],
                SuccessorNodeId = map[link.SuccessorNodeId],
                DependencyType = link.DependencyType,
                LagDays = link.LagDays,
                CreatedAt = now,
            })
            .ToList();

        return new ProjectInstance(project, copies, links);
    }

    private static DateOnly? Shift(DateOnly? date, int days) => date?.AddDays(days);

    private static string? Clean(string? code) => string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();
}
