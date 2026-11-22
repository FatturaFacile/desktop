// <copyright file="ProjectPortfolioStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Projects;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Projects;

/// <summary>
/// <see cref="IProjectPortfolioStore"/> over the SQLite database; the tenant query filters scope every read.
/// </summary>
/// <param name="db">The database context.</param>
/// <param name="tenant">The current tenant.</param>
public sealed class ProjectPortfolioStore(FatturaFacileDbContext db, ICurrentTenantService tenant) : IProjectPortfolioStore
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<Project>> GetProjectsAsync(CancellationToken cancellationToken) =>
        await db.Set<Project>().AsNoTracking().OrderBy(project => project.ProjectCode).ToListAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public Task<Project?> FindProjectAsync(Guid projectId, CancellationToken cancellationToken) =>
        db.Set<Project>().AsNoTracking().FirstOrDefaultAsync(project => project.Id == projectId, cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<WbsNode>> GetNodesAsync(Guid projectId, CancellationToken cancellationToken) =>
        await db.Set<WbsNode>().AsNoTracking().Where(node => node.ProjectId == projectId).OrderBy(node => node.SortOrder).ThenBy(node => node.WbsCode).ToListAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TaskDependency>> GetDependenciesAsync(Guid projectId, CancellationToken cancellationToken) =>
        await db.Set<TaskDependency>().AsNoTracking().Where(link => link.ProjectId == projectId).ToListAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProjectBaseline>> GetBaselinesAsync(Guid projectId, CancellationToken cancellationToken) =>
        await db.Set<ProjectBaseline>().AsNoTracking().Where(baseline => baseline.ProjectId == projectId).OrderByDescending(baseline => baseline.BaselineNumber).ToListAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task SavePlanAsync(Project project, IReadOnlyList<WbsNode> nodes, IReadOnlyList<TaskDependency> dependencies, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(dependencies);

        var row = await db.Set<Project>().FirstOrDefaultAsync(existing => existing.Id == project.Id, cancellationToken).ConfigureAwait(false);
        Upsert(db.Set<Project>(), row, project);

        var oldNodes = await db.Set<WbsNode>().Where(node => node.ProjectId == project.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        Replace(db.Set<WbsNode>(), oldNodes, nodes);

        var oldLinks = await db.Set<TaskDependency>().Where(link => link.ProjectId == project.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        Replace(db.Set<TaskDependency>(), oldLinks, dependencies);

        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task SaveBaselinesAsync(Guid projectId, IReadOnlyList<ProjectBaseline> baselines, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(baselines);

        var old = await db.Set<ProjectBaseline>().Where(baseline => baseline.ProjectId == projectId).ToListAsync(cancellationToken).ConfigureAwait(false);
        Replace(db.Set<ProjectBaseline>(), old, baselines);

        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task AddBaselineAsync(ProjectBaseline baseline, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(baseline);

        _ = db.Set<ProjectBaseline>().Add(baseline);
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task EnqueueAsync(ProjectOutboxEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);

        _ = db.Set<ProjectOutboxEntry>().Add(entry);
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProjectOutboxEntry>> PendingAsync(CancellationToken cancellationToken) =>
        await db.Set<ProjectOutboxEntry>()
            .Where(entry => entry.Status == ProjectOutboxStatus.Pending)
            .OrderBy(entry => entry.OccurredAtUtc)
            .ThenBy(entry => entry.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProjectOutboxEntry>> ConflictsAsync(CancellationToken cancellationToken) =>
        await db.Set<ProjectOutboxEntry>()
            .Where(entry => entry.Status == ProjectOutboxStatus.Conflict)
            .OrderBy(entry => entry.OccurredAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc/>
    public Task<bool> HasUnsentChangesAsync(Guid projectId, CancellationToken cancellationToken) =>
        db.Set<ProjectOutboxEntry>().AnyAsync(
            entry => entry.ProjectId == projectId && (entry.Status == ProjectOutboxStatus.Pending || entry.Status == ProjectOutboxStatus.Conflict),
            cancellationToken);

    /// <inheritdoc/>
    public async Task SaveAsync(CancellationToken cancellationToken) =>
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    private void Replace<T>(DbSet<T> set, List<T> existing, IReadOnlyList<T> incoming)
        where T : class, ITenantScopedEntity
    {
        var keep = incoming.Select(item => item.Id).ToHashSet();
        set.RemoveRange(existing.Where(item => !keep.Contains(item.Id)));
        foreach (var item in incoming)
        {
            Upsert(set, existing.FirstOrDefault(row => row.Id == item.Id), item);
        }
    }

    private void Upsert<T>(DbSet<T> set, T? row, T incoming)
        where T : class, ITenantScopedEntity
    {
        if (row is null)
        {
            incoming.TenantId = tenant.TenantId;
            _ = set.Add(incoming);

            return;
        }

        if (ReferenceEquals(row, incoming))
        {
            return;
        }

        var created = row.CreatedAt;
        db.Entry(row).CurrentValues.SetValues(incoming);
        row.TenantId = tenant.TenantId;
        row.CreatedAt = created ?? incoming.CreatedAt;
    }
}
