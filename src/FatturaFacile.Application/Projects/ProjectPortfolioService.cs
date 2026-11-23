// <copyright file="ProjectPortfolioService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Projects;

/// <summary>
/// <see cref="IProjectPortfolioService"/> over the local store and the web-api: local first, queue, replay with idempotency keys.
/// </summary>
/// <param name="api">The web-api calls.</param>
/// <param name="store">The local database.</param>
/// <param name="tenant">The current tenant.</param>
/// <param name="time">The clock.</param>
public sealed class ProjectPortfolioService(IProjectApiClient api, IProjectPortfolioStore store, ICurrentTenantService tenant, TimeProvider time) : IProjectPortfolioService
{
    private DateTime _lastStamp = DateTime.MinValue;

    /// <inheritdoc/>
    public async Task<ProjectPortfolio> LoadPortfolioAsync(bool refresh, CancellationToken cancellationToken)
    {
        _ = await FlushAsync(cancellationToken).ConfigureAwait(false);

        var pull = refresh ? await PullAsync(cancellationToken).ConfigureAwait(false) : null;
        var projects = await store.GetProjectsAsync(cancellationToken).ConfigureAwait(false);
        var pending = await store.PendingAsync(cancellationToken).ConfigureAwait(false);
        var conflicts = await store.ConflictsAsync(cancellationToken).ConfigureAwait(false);

        return new ProjectPortfolio(
            [.. projects.OrderBy(project => project.ProjectCode, StringComparer.OrdinalIgnoreCase)],
            [.. pending.Select(ToPending)],
            [.. conflicts.Select(ToPending)],
            FromCache: pull?.Offline ?? true,
            pull is { Offline: false } ? Now() : null);
    }

    /// <inheritdoc/>
    public async Task<ProjectPlan?> LoadPlanAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var project = await store.FindProjectAsync(projectId, cancellationToken).ConfigureAwait(false);
        if (project is null)
        {
            return null;
        }

        var nodes = await store.GetNodesAsync(projectId, cancellationToken).ConfigureAwait(false);
        var dependencies = await store.GetDependenciesAsync(projectId, cancellationToken).ConfigureAwait(false);
        var baselines = await store.GetBaselinesAsync(projectId, cancellationToken).ConfigureAwait(false);
        var schedule = CriticalPathEngine.Calculate(nodes, dependencies, StartOf(project, nodes));

        return new ProjectPlan(project, [.. WbsTreeOrder.Flatten(nodes).Select(item => item.Node)], dependencies, [.. baselines.OrderByDescending(baseline => baseline.BaselineNumber)], schedule);
    }

    /// <inheritdoc/>
    public async Task<BaselineVariance?> LoadVarianceAsync(Guid projectId, Guid baselineId, CancellationToken cancellationToken)
    {
        var project = await store.FindProjectAsync(projectId, cancellationToken).ConfigureAwait(false);
        var baselines = await store.GetBaselinesAsync(projectId, cancellationToken).ConfigureAwait(false);
        var baseline = baselines.FirstOrDefault(candidate => candidate.Id == baselineId);
        var snapshot = baseline is null ? null : BaselineSnapshotSerializer.Deserialize(baseline.SnapshotJson);
        if (project is null || baseline is null || snapshot is null)
        {
            return null;
        }

        var nodes = await store.GetNodesAsync(projectId, cancellationToken).ConfigureAwait(false);

        return BaselineVarianceCalculator.Compare(baseline, snapshot, project, nodes);
    }

    /// <inheritdoc/>
    public Task<ProjectActionResult> SaveProjectAsync(Project project, Guid commandId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(project);

        return SaveProjectCoreAsync(project, commandId, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<ProjectActionResult> SaveNodeAsync(WbsNode node, Guid commandId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(node);

        return SaveNodeCoreAsync(node, commandId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ProjectActionResult> DeleteNodeAsync(Guid projectId, Guid nodeId, Guid commandId, CancellationToken cancellationToken)
    {
        var project = await store.FindProjectAsync(projectId, cancellationToken).ConfigureAwait(false);
        var nodes = (await store.GetNodesAsync(projectId, cancellationToken).ConfigureAwait(false)).ToList();
        if (project is null || nodes.TrueForAll(node => node.Id != nodeId))
        {
            return Rejected(ProjectValidator.NotFoundKey);
        }

        var doomed = Descendants(nodeId, nodes);
        var dependencies = (await store.GetDependenciesAsync(projectId, cancellationToken).ConfigureAwait(false))
            .Where(link => !doomed.Contains(link.PredecessorNodeId) && !doomed.Contains(link.SuccessorNodeId))
            .ToList();
        var remaining = nodes.Where(node => !doomed.Contains(node.Id)).ToList();

        Recalculate(project, remaining, dependencies);
        await store.SavePlanAsync(project, remaining, dependencies, cancellationToken).ConfigureAwait(false);

        // The children go first, so a server without cascade never meets a parent that still has children.
        ProjectOutboxEntry? root = null;
        foreach (var id in doomed.Where(id => id != nodeId).Append(nodeId))
        {
            var entry = NewEntry(ProjectOutboxAction.DeleteNode, id == nodeId ? commandId : Guid.NewGuid(), project, id, []);
            if (id == nodeId)
            {
                root = entry;
            }

            await store.EnqueueAsync(entry, cancellationToken).ConfigureAwait(false);
        }

        _ = await FlushAsync(cancellationToken).ConfigureAwait(false);

        return Outcome(root!, entityId: null);
    }

    /// <inheritdoc/>
    public Task<ProjectActionResult> SaveDependencyAsync(TaskDependency dependency, Guid commandId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dependency);

        return SaveDependencyCoreAsync(dependency, commandId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ProjectActionResult> DeleteDependencyAsync(Guid projectId, Guid dependencyId, Guid commandId, CancellationToken cancellationToken)
    {
        var project = await store.FindProjectAsync(projectId, cancellationToken).ConfigureAwait(false);
        var dependencies = (await store.GetDependenciesAsync(projectId, cancellationToken).ConfigureAwait(false)).ToList();
        if (project is null || dependencies.TrueForAll(link => link.Id != dependencyId))
        {
            return Rejected(ProjectValidator.NotFoundKey);
        }

        var nodes = (await store.GetNodesAsync(projectId, cancellationToken).ConfigureAwait(false)).ToList();
        var remaining = dependencies.Where(link => link.Id != dependencyId).ToList();
        Recalculate(project, nodes, remaining);
        await store.SavePlanAsync(project, nodes, remaining, cancellationToken).ConfigureAwait(false);

        return await QueueAsync(NewEntry(ProjectOutboxAction.DeleteDependency, commandId, project, dependencyId, []), project.Id, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ProjectActionResult> CaptureBaselineAsync(Guid projectId, string name, Guid commandId, CancellationToken cancellationToken)
    {
        var project = await store.FindProjectAsync(projectId, cancellationToken).ConfigureAwait(false);
        if (project is null)
        {
            return Rejected(ProjectValidator.NotFoundKey);
        }

        ArgumentNullException.ThrowIfNull(name);

        var title = name.Trim();
        if (title.Length > 200)
        {
            return Rejected(ProjectValidator.BaselineNameKey);
        }

        var nodes = await store.GetNodesAsync(projectId, cancellationToken).ConfigureAwait(false);
        var existing = await store.GetBaselinesAsync(projectId, cancellationToken).ConfigureAwait(false);
        var number = existing.Count == 0 ? 1 : existing.Max(baseline => baseline.BaselineNumber) + 1;
        var baseline = new ProjectBaseline
        {
            Id = commandId,
            TenantId = tenant.TenantId,
            ProjectId = projectId,
            BaselineNumber = number,
            Name = title.Length == 0 ? "Baseline " + number.ToString(CultureInfo.InvariantCulture) : title,
            CapturedAt = Now(),
            SnapshotJson = BaselineSnapshotSerializer.Serialize(BaselineSnapshot.Capture(project, nodes)),
        };

        await store.AddBaselineAsync(baseline, cancellationToken).ConfigureAwait(false);

        var entry = NewEntry(ProjectOutboxAction.CaptureBaseline, Guid.NewGuid(), project, baseline.Id, ProjectPayloadMapper.ToJson(baseline));

        return WithEntity(await QueueAsync(entry, projectId, cancellationToken).ConfigureAwait(false), baseline.Id);
    }

    /// <inheritdoc/>
    public Task<ProjectActionResult> InstantiateTemplateAsync(TemplateInstantiationRequest request, Guid commandId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return InstantiateTemplateCoreAsync(request, commandId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ProjectFlushResult> FlushAsync(CancellationToken cancellationToken)
    {
        var entries = await store.PendingAsync(cancellationToken).ConfigureAwait(false);
        if (entries.Count == 0)
        {
            return new ProjectFlushResult(0, 0, 0, Offline: false);
        }

        if (!api.IsAuthenticated)
        {
            return new ProjectFlushResult(0, 0, entries.Count, Offline: true);
        }

        var sent = 0;
        var conflicts = 0;
        var offline = false;
        foreach (var entry in entries)
        {
            if (entry.NextAttemptAt > Now())
            {
                offline = true;
                break;
            }

            var outcome = await SendAsync(entry, cancellationToken).ConfigureAwait(false);
            await store.SaveAsync(cancellationToken).ConfigureAwait(false);
            if (outcome == ProjectSendOutcome.Sent)
            {
                sent++;
            }
            else if (outcome == ProjectSendOutcome.Refused)
            {
                conflicts++;
            }
            else
            {
                // The network is most likely down: keep the order of the remaining entries.
                offline = true;
                break;
            }
        }

        var remaining = (await store.PendingAsync(cancellationToken).ConfigureAwait(false)).Count;

        return new ProjectFlushResult(sent, conflicts, remaining, offline);
    }

    /// <inheritdoc/>
    public async Task<ProjectPullResult> PullAsync(CancellationToken cancellationToken)
    {
        if (!api.IsAuthenticated)
        {
            return new ProjectPullResult(0, Offline: true);
        }

        try
        {
            var remote = ProjectPayloadMapper.ParseProjects(await api.GetProjectsAsync(cancellationToken).ConfigureAwait(false));
            var refreshed = 0;
            foreach (var project in remote)
            {
                if (await store.HasUnsentChangesAsync(project.Id, cancellationToken).ConfigureAwait(false))
                {
                    continue;
                }

                var nodes = ProjectPayloadMapper.ParseNodes(await api.GetNodesAsync(project.Id, cancellationToken).ConfigureAwait(false), project.Id);
                var dependencies = ProjectPayloadMapper.ParseDependencies(await api.GetDependenciesAsync(project.Id, cancellationToken).ConfigureAwait(false), project.Id);
                var baselines = ProjectPayloadMapper.ParseBaselines(await api.GetBaselinesAsync(project.Id, cancellationToken).ConfigureAwait(false), project.Id);

                project.TenantId = tenant.TenantId;
                foreach (var node in nodes)
                {
                    node.TenantId = tenant.TenantId;
                }

                foreach (var link in dependencies)
                {
                    link.TenantId = tenant.TenantId;
                }

                foreach (var baseline in baselines)
                {
                    baseline.TenantId = tenant.TenantId;
                }

                await store.SavePlanAsync(project, nodes, dependencies, cancellationToken).ConfigureAwait(false);
                await store.SaveBaselinesAsync(project.Id, baselines, cancellationToken).ConfigureAwait(false);
                refreshed++;
            }

            return new ProjectPullResult(refreshed, Offline: false);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            // Offline or a bad answer: the local copy stays as it is.
            return new ProjectPullResult(0, Offline: true);
        }
    }

    /// <inheritdoc/>
    public async Task DismissConflictAsync(Guid entryId, CancellationToken cancellationToken)
    {
        var conflicts = await store.ConflictsAsync(cancellationToken).ConfigureAwait(false);
        var entry = conflicts.FirstOrDefault(candidate => candidate.Id == entryId);
        if (entry is null)
        {
            return;
        }

        entry.Status = ProjectOutboxStatus.Dismissed;
        entry.UpdatedAt = Now();
        await store.SaveAsync(cancellationToken).ConfigureAwait(false);
    }

    private static ProjectActionResult Rejected(string key) => new(ProjectActionStatus.Rejected, key);

    private static ProjectActionResult WithEntity(ProjectActionResult result, Guid entityId) => result with { EntityId = entityId };

    private static string? Upper(string? code) => string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();

    private static ProjectPendingChange ToPending(ProjectOutboxEntry entry) =>
        new(entry.Id, entry.Action, entry.ProjectId, entry.ProjectCode, DateTime.SpecifyKind(entry.OccurredAtUtc, DateTimeKind.Utc), entry.LastError);

    private static HashSet<Guid> Descendants(Guid rootId, List<WbsNode> nodes)
    {
        var found = new HashSet<Guid> { rootId };
        var added = true;
        while (added)
        {
            added = false;
            foreach (var node in nodes.Where(node => node.ParentNodeId is { } parent && found.Contains(parent)))
            {
                added |= found.Add(node.Id);
            }
        }

        return found;
    }

    private static string? CheckStructure(WbsNode node, List<WbsNode> nodes, List<TaskDependency> dependencies)
    {
        if (node.ParentNodeId is not { } parentId)
        {
            return null;
        }

        var parent = nodes.FirstOrDefault(other => other.Id == parentId);
        if (parent is null || parent.Id == node.Id || parent.NodeType == WbsNodeType.Milestone || Descendants(node.Id, nodes).Contains(parentId))
        {
            return ProjectValidator.ParentKey;
        }

        // A node with dependencies is a schedulable leaf: it cannot become a summary.
        return dependencies.Exists(link => link.PredecessorNodeId == parentId || link.SuccessorNodeId == parentId) ? ProjectValidator.SummaryLinkKey : null;
    }

    private static string Route(ProjectOutboxEntry entry) => entry.Action switch
    {
        ProjectOutboxAction.UpsertProject => ProjectRoutes.Project(entry.ProjectId),
        ProjectOutboxAction.UpsertNode or ProjectOutboxAction.DeleteNode => ProjectRoutes.Node(entry.ProjectId, entry.EntityId),
        ProjectOutboxAction.UpsertDependency or ProjectOutboxAction.DeleteDependency => ProjectRoutes.Dependency(entry.ProjectId, entry.EntityId),
        ProjectOutboxAction.CaptureBaseline => ProjectRoutes.Baselines(entry.ProjectId),
        ProjectOutboxAction.InstantiateTemplate => ProjectRoutes.Instantiate(TemplateOf(entry)),
        _ => throw new ArgumentOutOfRangeException(nameof(entry)),
    };

    private static HttpMethod Method(ProjectOutboxAction action) => action switch
    {
        ProjectOutboxAction.UpsertProject or ProjectOutboxAction.UpsertNode or ProjectOutboxAction.UpsertDependency => HttpMethod.Put,
        ProjectOutboxAction.DeleteNode or ProjectOutboxAction.DeleteDependency => HttpMethod.Delete,
        ProjectOutboxAction.CaptureBaseline or ProjectOutboxAction.InstantiateTemplate or _ => HttpMethod.Post,
    };

    private static Guid TemplateOf(ProjectOutboxEntry entry) =>
        Guid.TryParse(JsonNode.Parse(entry.PayloadJson)?["template_id"]?.GetValue<string>(), out var id) ? id : entry.ProjectId;

    private static string? Body(ProjectOutboxEntry entry)
    {
        if (entry.Action is ProjectOutboxAction.DeleteNode or ProjectOutboxAction.DeleteDependency)
        {
            return null;
        }

        var body = JsonNode.Parse(entry.PayloadJson)?.AsObject() ?? [];
        body["idempotency_key"] = entry.IdempotencyKey.ToString("D", CultureInfo.InvariantCulture);

        return body.ToJsonString();
    }

    private static string ErrorText(string body, HttpStatusCode status)
    {
        try
        {
            if (JsonNode.Parse(body) is JsonObject root)
            {
                if (root["errors"] is JsonObject errors)
                {
                    var first = errors.Select(field => field.Value).OfType<JsonArray>().SelectMany(list => list).FirstOrDefault();
                    if (first is not null)
                    {
                        return first.ToString();
                    }
                }

                if (root["message"] is JsonValue message)
                {
                    return message.ToString();
                }
            }
        }
        catch (JsonException)
        {
            // Not JSON: the status below describes it.
        }

        return "HTTP " + ((int)status).ToString(CultureInfo.InvariantCulture);
    }

    private static bool IsTransient(HttpStatusCode status) =>
        status is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests || (int)status is 425 or >= 500;

    private static ProjectActionResult Outcome(ProjectOutboxEntry entry, Guid? entityId) => entry.Status switch
    {
        ProjectOutboxStatus.Sent => new ProjectActionResult(ProjectActionStatus.Sent, EntityId: entityId),
        ProjectOutboxStatus.Conflict => new ProjectActionResult(ProjectActionStatus.Conflict, entry.LastError, entityId),
        ProjectOutboxStatus.Pending or ProjectOutboxStatus.Dismissed or _ => new ProjectActionResult(ProjectActionStatus.Queued, EntityId: entityId),
    };

    private static DateOnly StartOf(Project project, IReadOnlyList<WbsNode> nodes, DateOnly today) =>
        project.StartDate ?? nodes.Where(node => node.StartDate.HasValue).Select(node => node.StartDate!.Value).DefaultIfEmpty(today).Min();

    private DateOnly StartOf(Project project, IReadOnlyList<WbsNode> nodes) => StartOf(project, nodes, DateOnly.FromDateTime(Now()));

    private DateTime Now() => time.GetUtcNow().UtcDateTime;

    private DateTime NextStamp()
    {
        var stamp = Now();
        _lastStamp = stamp <= _lastStamp ? _lastStamp.AddTicks(1) : stamp;

        return _lastStamp;
    }

    private void Recalculate(Project project, List<WbsNode> nodes, List<TaskDependency> dependencies)
    {
        WbsRollupCalculator.Apply(project, nodes);
        CriticalPathEngine.Apply(nodes, CriticalPathEngine.Calculate(nodes, dependencies, StartOf(project, nodes)));
    }

    private ProjectOutboxEntry NewEntry(ProjectOutboxAction action, Guid commandId, Project project, Guid entityId, JsonObject body) => new()
    {
        Id = commandId,
        TenantId = tenant.TenantId,
        Action = action,
        ProjectId = project.Id,
        EntityId = entityId,
        ProjectCode = project.ProjectCode,
        PayloadJson = body.ToJsonString(),
        IdempotencyKey = commandId,
        OccurredAtUtc = NextStamp(),
        NextAttemptAt = Now(),
    };

    private async Task<ProjectActionResult> QueueAsync(ProjectOutboxEntry entry, Guid entityId, CancellationToken cancellationToken)
    {
        await store.EnqueueAsync(entry, cancellationToken).ConfigureAwait(false);
        _ = await FlushAsync(cancellationToken).ConfigureAwait(false);

        return Outcome(entry, entityId);
    }

    private async Task<ProjectSendOutcome> SendAsync(ProjectOutboxEntry entry, CancellationToken cancellationToken)
    {
        SyncApiReply reply;
        try
        {
            reply = await api.SendAsync(Method(entry.Action), Route(entry), Body(entry), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is HttpRequestException || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            return Retry(entry, ex.Message);
        }

        // Deleting what the server no longer has is a success: the delete is idempotent.
        var gone = reply.Status == HttpStatusCode.NotFound && entry.Action is ProjectOutboxAction.DeleteNode or ProjectOutboxAction.DeleteDependency;
        if (reply.IsSuccess || gone)
        {
            entry.Status = ProjectOutboxStatus.Sent;
            entry.ResultJson = reply.Text;
            entry.LastError = null;
            entry.UpdatedAt = Now();

            return ProjectSendOutcome.Sent;
        }

        if (reply.Status == HttpStatusCode.Unauthorized || IsTransient(reply.Status))
        {
            return Retry(entry, ErrorText(reply.Text, reply.Status));
        }

        entry.Status = ProjectOutboxStatus.Conflict;
        entry.LastError = ErrorText(reply.Text, reply.Status);
        entry.UpdatedAt = Now();

        return ProjectSendOutcome.Refused;
    }

    private ProjectSendOutcome Retry(ProjectOutboxEntry entry, string error)
    {
        entry.Attempts++;
        entry.LastError = error;
        entry.UpdatedAt = Now();
        if (!SyncRetryPolicy.CanRetry(entry.Attempts))
        {
            entry.Status = ProjectOutboxStatus.Conflict;

            return ProjectSendOutcome.Refused;
        }

        entry.NextAttemptAt = Now() + SyncRetryPolicy.NextDelay(entry.Attempts);

        return ProjectSendOutcome.Retry;
    }

    private async Task<ProjectActionResult> SaveProjectCoreAsync(Project project, Guid commandId, CancellationToken cancellationToken)
    {
        var invalid = ProjectValidator.ValidateProject(project);
        if (invalid is not null)
        {
            return Rejected(invalid);
        }

        project.ProjectCode = project.ProjectCode.Trim();
        project.Name = project.Name.Trim();
        project.CupCode = Upper(project.CupCode);
        project.CigCode = Upper(project.CigCode);
        project.TenantId = tenant.TenantId;

        var all = await store.GetProjectsAsync(cancellationToken).ConfigureAwait(false);
        if (all.Any(other => other.Id != project.Id && string.Equals(other.ProjectCode, project.ProjectCode, StringComparison.OrdinalIgnoreCase)))
        {
            return Rejected(ProjectValidator.DuplicateCodeKey);
        }

        var nodes = (await store.GetNodesAsync(project.Id, cancellationToken).ConfigureAwait(false)).ToList();
        var dependencies = (await store.GetDependenciesAsync(project.Id, cancellationToken).ConfigureAwait(false)).ToList();
        Recalculate(project, nodes, dependencies);
        await store.SavePlanAsync(project, nodes, dependencies, cancellationToken).ConfigureAwait(false);

        return await QueueAsync(NewEntry(ProjectOutboxAction.UpsertProject, commandId, project, project.Id, ProjectPayloadMapper.ToJson(project)), project.Id, cancellationToken).ConfigureAwait(false);
    }

    private async Task<ProjectActionResult> SaveNodeCoreAsync(WbsNode node, Guid commandId, CancellationToken cancellationToken)
    {
        var project = await store.FindProjectAsync(node.ProjectId, cancellationToken).ConfigureAwait(false);
        if (project is null)
        {
            return Rejected(ProjectValidator.NotFoundKey);
        }

        var invalid = ProjectValidator.ValidateNode(node);
        if (invalid is not null)
        {
            return Rejected(invalid);
        }

        var nodes = (await store.GetNodesAsync(project.Id, cancellationToken).ConfigureAwait(false)).ToList();
        var dependencies = (await store.GetDependenciesAsync(project.Id, cancellationToken).ConfigureAwait(false)).ToList();
        var structure = CheckStructure(node, nodes, dependencies);
        if (structure is not null)
        {
            return Rejected(structure);
        }

        node.TenantId = tenant.TenantId;
        node.Title = node.Title.Trim();
        if (node.NodeType == WbsNodeType.Milestone)
        {
            node.PlannedHours = 0m;
            node.ActualHours = 0m;
            node.EndDate = node.StartDate ?? node.EndDate;
        }

        var siblings = nodes.Where(other => other.Id != node.Id && other.ParentNodeId == node.ParentNodeId).ToList();
        if (string.IsNullOrWhiteSpace(node.WbsCode))
        {
            var parentCode = node.ParentNodeId is { } parentId ? nodes.First(other => other.Id == parentId).WbsCode : null;
            node.WbsCode = WbsCodeGenerator.NextChildCode(parentCode, siblings.Select(other => other.WbsCode));
        }

        node.WbsCode = node.WbsCode.Trim();
        if (nodes.Exists(other => other.Id != node.Id && string.Equals(other.WbsCode, node.WbsCode, StringComparison.Ordinal)))
        {
            return Rejected(ProjectValidator.DuplicateCodeKey);
        }

        if (node.SortOrder <= 0)
        {
            node.SortOrder = siblings.Count == 0 ? 1 : siblings.Max(other => other.SortOrder) + 1;
        }

        var index = nodes.FindIndex(other => other.Id == node.Id);
        if (index >= 0)
        {
            nodes[index] = node;
        }
        else
        {
            nodes.Add(node);
        }

        Recalculate(project, nodes, dependencies);
        await store.SavePlanAsync(project, nodes, dependencies, cancellationToken).ConfigureAwait(false);

        var entry = NewEntry(ProjectOutboxAction.UpsertNode, commandId, project, node.Id, ProjectPayloadMapper.ToJson(node));

        return WithEntity(await QueueAsync(entry, project.Id, cancellationToken).ConfigureAwait(false), node.Id);
    }

    private async Task<ProjectActionResult> SaveDependencyCoreAsync(TaskDependency dependency, Guid commandId, CancellationToken cancellationToken)
    {
        var project = await store.FindProjectAsync(dependency.ProjectId, cancellationToken).ConfigureAwait(false);
        if (project is null)
        {
            return Rejected(ProjectValidator.NotFoundKey);
        }

        var invalid = ProjectValidator.ValidateDependency(dependency);
        if (invalid is not null)
        {
            return Rejected(invalid);
        }

        var nodes = (await store.GetNodesAsync(project.Id, cancellationToken).ConfigureAwait(false)).ToList();
        var dependencies = (await store.GetDependenciesAsync(project.Id, cancellationToken).ConfigureAwait(false)).ToList();
        var parents = nodes.Where(node => node.ParentNodeId.HasValue).Select(node => node.ParentNodeId!.Value).ToHashSet();
        var known = nodes.Select(node => node.Id).ToHashSet();
        if (!known.Contains(dependency.PredecessorNodeId) || !known.Contains(dependency.SuccessorNodeId)
            || parents.Contains(dependency.PredecessorNodeId) || parents.Contains(dependency.SuccessorNodeId))
        {
            return Rejected(ProjectValidator.DependencyKey);
        }

        var others = dependencies.Where(link => link.Id != dependency.Id).ToList();
        if (others.Exists(link => link.PredecessorNodeId == dependency.PredecessorNodeId && link.SuccessorNodeId == dependency.SuccessorNodeId))
        {
            return Rejected(ProjectValidator.DependencyKey);
        }

        if (DependencyGraph.WouldCreateCycle(others, dependency.PredecessorNodeId, dependency.SuccessorNodeId))
        {
            return Rejected(ProjectValidator.CycleKey);
        }

        dependency.TenantId = tenant.TenantId;
        others.Add(dependency);
        Recalculate(project, nodes, others);
        await store.SavePlanAsync(project, nodes, others, cancellationToken).ConfigureAwait(false);

        var entry = NewEntry(ProjectOutboxAction.UpsertDependency, commandId, project, dependency.Id, ProjectPayloadMapper.ToJson(dependency));

        return WithEntity(await QueueAsync(entry, project.Id, cancellationToken).ConfigureAwait(false), dependency.Id);
    }

    private async Task<ProjectActionResult> InstantiateTemplateCoreAsync(TemplateInstantiationRequest request, Guid commandId, CancellationToken cancellationToken)
    {
        var template = await store.FindProjectAsync(request.TemplateId, cancellationToken).ConfigureAwait(false);
        if (template is null)
        {
            return Rejected(ProjectValidator.NotFoundKey);
        }

        var probe = new Project { ProjectCode = request.ProjectCode, Name = request.Name, CupCode = request.CupCode, CigCode = request.CigCode };
        var invalid = ProjectValidator.ValidateProject(probe);
        if (invalid is not null)
        {
            return Rejected(invalid);
        }

        var all = await store.GetProjectsAsync(cancellationToken).ConfigureAwait(false);
        if (all.Any(other => string.Equals(other.ProjectCode, request.ProjectCode.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return Rejected(ProjectValidator.DuplicateCodeKey);
        }

        var nodes = await store.GetNodesAsync(template.Id, cancellationToken).ConfigureAwait(false);
        var dependencies = await store.GetDependenciesAsync(template.Id, cancellationToken).ConfigureAwait(false);
        var instance = ProjectTemplateInstantiator.Instantiate(template, nodes, dependencies, request, tenant.TenantId);
        var copiedNodes = instance.Nodes.ToList();
        var copiedLinks = instance.Dependencies.ToList();

        Recalculate(instance.Project, copiedNodes, copiedLinks);
        await store.SavePlanAsync(instance.Project, copiedNodes, copiedLinks, cancellationToken).ConfigureAwait(false);

        var body = new JsonObject
        {
            ["template_id"] = template.Id.ToString("D", CultureInfo.InvariantCulture),
            ["project"] = ProjectPayloadMapper.ToJson(instance.Project),
            ["wbs_nodes"] = new JsonArray([.. copiedNodes.Select(node => (JsonNode?)ProjectPayloadMapper.ToJson(node))]),
            ["dependencies"] = new JsonArray([.. copiedLinks.Select(link => (JsonNode?)ProjectPayloadMapper.ToJson(link))]),
        };
        var entry = NewEntry(ProjectOutboxAction.InstantiateTemplate, commandId, instance.Project, instance.Project.Id, body);

        return WithEntity(await QueueAsync(entry, instance.Project.Id, cancellationToken).ConfigureAwait(false), instance.Project.Id);
    }
}
