// <copyright file="ProjectPortfolioStoreTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Projects;
using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// SQLite tests of the offline project portfolio store: round trip, replacement of children, outbox and tenant isolation.
/// </summary>
public sealed class ProjectPortfolioStoreTests : IDisposable
{
    private static readonly Guid TenantB = new(0xBBBBBBBB, 0xBBBB, 0xBBBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB);

    private readonly SqliteConnection connection = new("DataSource=:memory:");
    private readonly CurrentTenantService tenant = new();
    private readonly FatturaFacileDbContext db;
    private readonly ProjectPortfolioStore store;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectPortfolioStoreTests"/> class.
    /// </summary>
    public ProjectPortfolioStoreTests()
    {
        connection.Open();
        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new TenantSaveChangesInterceptor(tenant))
            .Options;
        db = new FatturaFacileDbContext(options, tenant, new AesGcmFieldEncryptor(new byte[32]));
        _ = db.Database.EnsureCreated();
        tenant.SetCurrentTenant(ProjectTestData.Tenant, "Alpha", "CH", "CHF");
        store = new ProjectPortfolioStore(db, tenant);
    }

    /// <summary>
    /// A saved plan comes back with its project, nodes and dependencies.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SavePlan_RoundTripsProjectNodesAndDependencies()
    {
        var project = ProjectTestData.Project();
        var first = ProjectTestData.Node("1", 1, 3, project.Id, hours: 8m);
        var second = ProjectTestData.Node("2", 4, 6, project.Id, hours: 4m);
        first.EarlyStart = ProjectTestData.Day(1);
        first.IsCritical = true;
        var link = ProjectTestData.Link(first, second, DependencyType.SS, 1);

        await store.SavePlanAsync(project, [first, second], [link], CancellationToken.None);

        var stored = await store.FindProjectAsync(project.Id, CancellationToken.None);
        Assert.Equal("P-1", stored?.ProjectCode);
        Assert.Equal(ProjectTestData.Day(1), stored?.StartDate);
        var nodes = await store.GetNodesAsync(project.Id, CancellationToken.None);
        Assert.Equal(2, nodes.Count);
        var loaded = nodes.Single(node => node.WbsCode == "1");
        Assert.Equal(8m, loaded.PlannedHours);
        Assert.Equal(ProjectTestData.Day(1), loaded.EarlyStart);
        Assert.True(loaded.IsCritical);
        var links = await store.GetDependenciesAsync(project.Id, CancellationToken.None);
        Assert.Equal(DependencyType.SS, Assert.Single(links).DependencyType);
        Assert.Single(await store.GetProjectsAsync(CancellationToken.None));
    }

    /// <summary>
    /// Saving again updates the rows that are in the lists and deletes the ones that are not.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SavePlan_Twice_ReplacesTheChildren()
    {
        var project = ProjectTestData.Project();
        var keep = ProjectTestData.Node("1", 1, 3, project.Id, hours: 8m);
        var drop = ProjectTestData.Node("2", 4, 6, project.Id);
        await store.SavePlanAsync(project, [keep, drop], [ProjectTestData.Link(keep, drop)], CancellationToken.None);

        var changed = ProjectTestData.Node("1", 1, 3, project.Id, hours: 20m);
        changed.Id = keep.Id;
        var renamed = ProjectTestData.Project("P-1", project.Id);
        renamed.Name = "Renamed";
        await store.SavePlanAsync(renamed, [changed], [], CancellationToken.None);

        var nodes = await store.GetNodesAsync(project.Id, CancellationToken.None);
        var node = Assert.Single(nodes);
        Assert.Equal(20m, node.PlannedHours);
        Assert.Empty(await store.GetDependenciesAsync(project.Id, CancellationToken.None));
        Assert.Equal("Renamed", (await store.FindProjectAsync(project.Id, CancellationToken.None))?.Name);
        Assert.Single(await store.GetProjectsAsync(CancellationToken.None));
    }

    /// <summary>
    /// Baselines are listed newest first and replaced as a set.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Baselines_AreAddedAndReplaced()
    {
        var projectId = Guid.NewGuid();
        await store.AddBaselineAsync(new ProjectBaseline { ProjectId = projectId, BaselineNumber = 1, Name = "One" }, CancellationToken.None);
        await store.AddBaselineAsync(new ProjectBaseline { ProjectId = projectId, BaselineNumber = 2, Name = "Two" }, CancellationToken.None);

        var listed = await store.GetBaselinesAsync(projectId, CancellationToken.None);
        Assert.Equal(["Two", "One"], listed.Select(baseline => baseline.Name).ToArray());

        await store.SaveBaselinesAsync(projectId, [new ProjectBaseline { ProjectId = projectId, BaselineNumber = 3, Name = "Three" }], CancellationToken.None);

        Assert.Equal("Three", Assert.Single(await store.GetBaselinesAsync(projectId, CancellationToken.None)).Name);
    }

    /// <summary>
    /// The outbox lists pending entries in order and tells whether a project has undelivered changes.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Outbox_ListsPendingInOrderAndFlagsUnsentProjects()
    {
        var projectId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var at = new DateTime(2027, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        await store.EnqueueAsync(Entry(projectId, at.AddMinutes(2), ProjectOutboxAction.UpsertNode), CancellationToken.None);
        await store.EnqueueAsync(Entry(projectId, at.AddMinutes(1), ProjectOutboxAction.UpsertProject), CancellationToken.None);
        var sent = Entry(otherId, at, ProjectOutboxAction.UpsertProject);
        sent.Status = ProjectOutboxStatus.Sent;
        await store.EnqueueAsync(sent, CancellationToken.None);

        var pending = await store.PendingAsync(CancellationToken.None);

        Assert.Equal([ProjectOutboxAction.UpsertProject, ProjectOutboxAction.UpsertNode], pending.Select(entry => entry.Action).ToArray());
        Assert.True(await store.HasUnsentChangesAsync(projectId, CancellationToken.None));
        Assert.False(await store.HasUnsentChangesAsync(otherId, CancellationToken.None));

        pending[0].Status = ProjectOutboxStatus.Conflict;
        await store.SaveAsync(CancellationToken.None);

        Assert.Single(await store.ConflictsAsync(CancellationToken.None));
        Assert.True(await store.HasUnsentChangesAsync(projectId, CancellationToken.None));
    }

    /// <summary>
    /// A tenant never sees the projects or the queue of another one.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Tenants_AreIsolated()
    {
        var project = ProjectTestData.Project();
        await store.SavePlanAsync(project, [ProjectTestData.Node("1", 1, 2, project.Id)], [], CancellationToken.None);
        await store.EnqueueAsync(Entry(project.Id, DateTime.UtcNow, ProjectOutboxAction.UpsertProject), CancellationToken.None);

        tenant.SetCurrentTenant(TenantB, "Beta", "CH", "CHF");

        Assert.Empty(await store.GetProjectsAsync(CancellationToken.None));
        Assert.Null(await store.FindProjectAsync(project.Id, CancellationToken.None));
        Assert.Empty(await store.GetNodesAsync(project.Id, CancellationToken.None));
        Assert.Empty(await store.PendingAsync(CancellationToken.None));
        Assert.False(await store.HasUnsentChangesAsync(project.Id, CancellationToken.None));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        db.Dispose();
        connection.Dispose();
    }

    private static ProjectOutboxEntry Entry(Guid projectId, DateTime at, ProjectOutboxAction action) => new()
    {
        TenantId = ProjectTestData.Tenant,
        ProjectId = projectId,
        EntityId = projectId,
        ProjectCode = "P-1",
        Action = action,
        OccurredAtUtc = at,
    };
}
