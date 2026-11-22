// <copyright file="ProjectPortfolioServiceTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using FatturaFacile.Application.Projects;
using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Projects;
using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the offline-first project portfolio service over a real SQLite store and a fake web-api.
/// </summary>
public sealed class ProjectPortfolioServiceTests : IDisposable
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();

    private readonly SqliteConnection connection = new("DataSource=:memory:");
    private readonly CurrentTenantService tenant = new();
    private readonly FatturaFacileDbContext db;
    private readonly ProjectPortfolioStore store;
    private readonly ManualTimeProvider clock = new();
    private readonly Mock<IProjectApiClient> api = new();
    private readonly List<(HttpMethod Method, string Route, string? Body)> sent = [];
    private readonly ProjectPortfolioService service;

    private Func<(HttpMethod Method, string Route), SyncApiReply> reply = _ => Reply(HttpStatusCode.OK, "{\"success\":true}");

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectPortfolioServiceTests"/> class.
    /// </summary>
    public ProjectPortfolioServiceTests()
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

        _ = api.SetupGet(client => client.IsAuthenticated).Returns(value: true);
        _ = api
            .Setup(client => client.SendAsync(It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<string?>(), Any))
            .ReturnsAsync((HttpMethod method, string route, string? body, CancellationToken _) =>
            {
                sent.Add((method, route, body));

                return reply((method, route));
            });
        service = new ProjectPortfolioService(api.Object, store, tenant, clock);
    }

    /// <summary>
    /// Offline, a project is saved locally, queued and nothing is sent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveProject_Offline_QueuesAndSendsNothing()
    {
        _ = api.SetupGet(client => client.IsAuthenticated).Returns(value: false);
        var project = ProjectTestData.Project("P-OFF");

        var result = await service.SaveProjectAsync(project, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ProjectActionStatus.Queued, result.Status);
        Assert.Equal(project.Id, result.EntityId);
        Assert.Empty(sent);
        Assert.Equal("P-OFF", (await store.FindProjectAsync(project.Id, CancellationToken.None))?.ProjectCode);
        var entry = Assert.Single(await store.PendingAsync(CancellationToken.None));
        Assert.Equal(ProjectOutboxAction.UpsertProject, entry.Action);
        Assert.Equal(ProjectTestData.Tenant, entry.TenantId);
    }

    /// <summary>
    /// Online, the change is sent with PUT, the idempotency key and the project body, and the entry is delivered.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveProject_Online_SendsAndMarksTheEntryDelivered()
    {
        var project = ProjectTestData.Project("P-ON");
        project.CupCode = " b12c34d56e78f90 ";
        var commandId = Guid.NewGuid();

        var result = await service.SaveProjectAsync(project, commandId, CancellationToken.None);

        Assert.Equal(ProjectActionStatus.Sent, result.Status);
        var call = Assert.Single(sent);
        Assert.Equal(HttpMethod.Put, call.Method);
        Assert.Equal("projects/" + project.Id.ToString("D"), call.Route);
        var body = JsonNode.Parse(call.Body!)!.AsObject();
        Assert.Equal(commandId.ToString("D"), body["idempotency_key"]?.GetValue<string>());
        Assert.Equal("P-ON", body["project_code"]?.GetValue<string>());
        Assert.Equal("B12C34D56E78F90", body["cup_code"]?.GetValue<string>());
        Assert.Empty(await store.PendingAsync(CancellationToken.None));
    }

    /// <summary>
    /// Invalid fields are rejected with a localization key and nothing is stored or queued.
    /// </summary>
    /// <param name="code">The project code.</param>
    /// <param name="cup">The CUP.</param>
    /// <param name="cig">The CIG.</param>
    /// <param name="expected">The expected key.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData("", null, null, ProjectValidator.CodeKey)]
    [InlineData("P", "SHORT", null, ProjectValidator.CupKey)]
    [InlineData("P", null, "TOO-LONG-CIG", ProjectValidator.CigKey)]
    public async Task SaveProject_Invalid_IsRejected(string code, string? cup, string? cig, string expected)
    {
        var project = ProjectTestData.Project(code);
        project.ProjectCode = code;
        project.CupCode = cup;
        project.CigCode = cig;

        var result = await service.SaveProjectAsync(project, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ProjectActionStatus.Rejected, result.Status);
        Assert.Equal(expected, result.Detail);
        Assert.Empty(await store.GetProjectsAsync(CancellationToken.None));
        Assert.Empty(await store.PendingAsync(CancellationToken.None));
    }

    /// <summary>
    /// A project cannot end before it starts and two projects cannot share a code.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveProject_BadDatesOrDuplicateCode_IsRejected()
    {
        var late = ProjectTestData.Project("LATE");
        late.TargetEndDate = ProjectTestData.Day(1).AddDays(-1);
        Assert.Equal(ProjectValidator.DatesKey, (await service.SaveProjectAsync(late, Guid.NewGuid(), CancellationToken.None)).Detail);

        _ = await service.SaveProjectAsync(ProjectTestData.Project("DUP"), Guid.NewGuid(), CancellationToken.None);
        var duplicate = await service.SaveProjectAsync(ProjectTestData.Project("dup"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ProjectActionStatus.Rejected, duplicate.Status);
        Assert.Equal(ProjectValidator.DuplicateCodeKey, duplicate.Detail);
    }

    /// <summary>
    /// Saving nodes assigns hierarchical codes, rolls the figures up and schedules the project.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveNode_AssignsCodesAndRollsUp()
    {
        var project = await SeedProjectAsync();
        var phase = Node(project, "Phase", WbsNodeType.Phase);
        _ = await service.SaveNodeAsync(phase, Guid.NewGuid(), CancellationToken.None);
        var first = Node(project, "First", parent: phase.Id, hours: 8m, cost: 80m, start: 1, end: 3);
        var second = Node(project, "Second", parent: phase.Id, hours: 4m, cost: 40m, start: 4, end: 5);

        var firstResult = await service.SaveNodeAsync(first, Guid.NewGuid(), CancellationToken.None);
        _ = await service.SaveNodeAsync(second, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ProjectActionStatus.Sent, firstResult.Status);
        Assert.Equal(first.Id, firstResult.EntityId);
        var plan = await service.LoadPlanAsync(project.Id, CancellationToken.None);
        Assert.NotNull(plan);
        Assert.Equal(["1", "1.1", "1.2"], plan.Nodes.Select(node => node.WbsCode).ToArray());
        Assert.Equal(12m, plan.Project.PlannedHours);
        Assert.Equal(120m, plan.Project.PlannedCost);
        Assert.Equal(12m, plan.Nodes[0].PlannedHours);
        Assert.Equal(ProjectTestData.Day(1), plan.Nodes[0].StartDate);
        Assert.Equal(ProjectTestData.Day(5), plan.Nodes[0].EndDate);
        Assert.Contains(sent, call => call.Method == HttpMethod.Put && call.Route == "projects/" + project.Id.ToString("D") + "/wbs-nodes/" + first.Id.ToString("D"));
    }

    /// <summary>
    /// A node is rejected when its fields or its place in the tree are not valid.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveNode_InvalidFieldsOrParent_AreRejected()
    {
        var project = await SeedProjectAsync();
        var milestone = Node(project, "Gate", WbsNodeType.Milestone);
        _ = await service.SaveNodeAsync(milestone, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ProjectValidator.TitleKey, (await service.SaveNodeAsync(Node(project, " "), Guid.NewGuid(), CancellationToken.None)).Detail);

        var badPercent = Node(project, "Task");
        badPercent.PercentComplete = 101m;
        Assert.Equal(ProjectValidator.PercentKey, (await service.SaveNodeAsync(badPercent, Guid.NewGuid(), CancellationToken.None)).Detail);

        var negative = Node(project, "Task", hours: -1m);
        Assert.Equal(ProjectValidator.AmountKey, (await service.SaveNodeAsync(negative, Guid.NewGuid(), CancellationToken.None)).Detail);

        var underMilestone = Node(project, "Child", parent: milestone.Id);
        Assert.Equal(ProjectValidator.ParentKey, (await service.SaveNodeAsync(underMilestone, Guid.NewGuid(), CancellationToken.None)).Detail);

        var orphan = Node(project, "Orphan", parent: Guid.NewGuid());
        Assert.Equal(ProjectValidator.ParentKey, (await service.SaveNodeAsync(orphan, Guid.NewGuid(), CancellationToken.None)).Detail);

        var stranger = Node(ProjectTestData.Project("GHOST"), "Lost");
        Assert.Equal(ProjectValidator.NotFoundKey, (await service.SaveNodeAsync(stranger, Guid.NewGuid(), CancellationToken.None)).Detail);
    }

    /// <summary>
    /// A node cannot be moved under one of its own descendants.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveNode_MovingUnderADescendant_IsRejected()
    {
        var project = await SeedProjectAsync();
        var parent = Node(project, "Parent", WbsNodeType.Phase);
        _ = await service.SaveNodeAsync(parent, Guid.NewGuid(), CancellationToken.None);
        var child = Node(project, "Child", parent: parent.Id);
        _ = await service.SaveNodeAsync(child, Guid.NewGuid(), CancellationToken.None);

        var cyclic = Clone(parent);
        cyclic.ParentNodeId = child.Id;
        var result = await service.SaveNodeAsync(cyclic, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ProjectValidator.ParentKey, result.Detail);
    }

    /// <summary>
    /// A milestone keeps no hours and ends the day it starts.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveNode_Milestone_IsNormalized()
    {
        var project = await SeedProjectAsync();
        var gate = Node(project, "Gate", WbsNodeType.Milestone, hours: 5m, start: 10, end: 12);

        _ = await service.SaveNodeAsync(gate, Guid.NewGuid(), CancellationToken.None);

        var stored = Assert.Single((await service.LoadPlanAsync(project.Id, CancellationToken.None))!.Nodes);
        Assert.Equal(0m, stored.PlannedHours);
        Assert.Equal(ProjectTestData.Day(10), stored.EndDate);
    }

    /// <summary>
    /// A dependency drives the schedule of the plan: the successor starts after the predecessor and both are critical.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveDependency_DrivesTheCriticalPath()
    {
        var project = await SeedProjectAsync();
        var first = Node(project, "First", start: 1, end: 3);
        var second = Node(project, "Second", start: 1, end: 2);
        _ = await service.SaveNodeAsync(first, Guid.NewGuid(), CancellationToken.None);
        _ = await service.SaveNodeAsync(second, Guid.NewGuid(), CancellationToken.None);
        var link = Link(project, first, second, DependencyType.FS, lag: 1);

        var result = await service.SaveDependencyAsync(link, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ProjectActionStatus.Sent, result.Status);
        var plan = (await service.LoadPlanAsync(project.Id, CancellationToken.None))!;
        var schedule = plan.Schedule.Nodes.Single(node => node.NodeId == second.Id);
        Assert.Equal(ProjectTestData.Day(5), schedule.EarlyStart);
        Assert.True(schedule.IsCritical);
        Assert.Equal([first.Id, second.Id], plan.Schedule.CriticalPath.ToArray());
        var stored = plan.Nodes.Single(node => node.Id == second.Id);
        Assert.Equal(ProjectTestData.Day(5), stored.EarlyStart);
        Assert.True(stored.IsCritical);
        Assert.Contains(sent, call => call.Route.EndsWith("/dependencies/" + link.Id.ToString("D"), StringComparison.Ordinal));
    }

    /// <summary>
    /// A dependency that closes a cycle, links a node to itself, repeats a link or touches a summary node is rejected.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveDependency_InvalidLinks_AreRejected()
    {
        var project = await SeedProjectAsync();
        var phase = Node(project, "Phase", WbsNodeType.Phase);
        _ = await service.SaveNodeAsync(phase, Guid.NewGuid(), CancellationToken.None);
        var first = Node(project, "First", parent: phase.Id, start: 1, end: 2);
        var second = Node(project, "Second", parent: phase.Id, start: 1, end: 2);
        _ = await service.SaveNodeAsync(first, Guid.NewGuid(), CancellationToken.None);
        _ = await service.SaveNodeAsync(second, Guid.NewGuid(), CancellationToken.None);
        _ = await service.SaveDependencyAsync(Link(project, first, second), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ProjectValidator.CycleKey, (await service.SaveDependencyAsync(Link(project, second, first), Guid.NewGuid(), CancellationToken.None)).Detail);
        Assert.Equal(ProjectValidator.DependencyKey, (await service.SaveDependencyAsync(Link(project, first, first), Guid.NewGuid(), CancellationToken.None)).Detail);
        Assert.Equal(ProjectValidator.DependencyKey, (await service.SaveDependencyAsync(Link(project, first, second), Guid.NewGuid(), CancellationToken.None)).Detail);
        Assert.Equal(ProjectValidator.DependencyKey, (await service.SaveDependencyAsync(Link(project, phase, second), Guid.NewGuid(), CancellationToken.None)).Detail);
        Assert.Equal(ProjectValidator.LagKey, (await service.SaveDependencyAsync(Link(project, first, second, lag: 400), Guid.NewGuid(), CancellationToken.None)).Detail);
        Assert.Single((await service.LoadPlanAsync(project.Id, CancellationToken.None))!.Dependencies);
    }

    /// <summary>
    /// A node that already has dependencies cannot become a summary node by receiving children.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveNode_ChildUnderALinkedNode_IsRejected()
    {
        var project = await SeedProjectAsync();
        var first = Node(project, "First", start: 1, end: 2);
        var second = Node(project, "Second", start: 1, end: 2);
        _ = await service.SaveNodeAsync(first, Guid.NewGuid(), CancellationToken.None);
        _ = await service.SaveNodeAsync(second, Guid.NewGuid(), CancellationToken.None);
        _ = await service.SaveDependencyAsync(Link(project, first, second), Guid.NewGuid(), CancellationToken.None);

        var child = Node(project, "Child", parent: first.Id);
        var result = await service.SaveNodeAsync(child, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ProjectValidator.SummaryLinkKey, result.Detail);
    }

    /// <summary>
    /// Deleting a node removes its descendants and the dependencies that touch them, and queues the deletes children first.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task DeleteNode_RemovesDescendantsAndLinksAndQueuesDeletesChildrenFirst()
    {
        var project = await SeedProjectAsync();
        var phase = Node(project, "Phase", WbsNodeType.Phase);
        _ = await service.SaveNodeAsync(phase, Guid.NewGuid(), CancellationToken.None);
        var child = Node(project, "Child", parent: phase.Id, start: 1, end: 2, hours: 3m);
        var other = Node(project, "Other", start: 1, end: 2);
        _ = await service.SaveNodeAsync(child, Guid.NewGuid(), CancellationToken.None);
        _ = await service.SaveNodeAsync(other, Guid.NewGuid(), CancellationToken.None);
        _ = await service.SaveDependencyAsync(Link(project, child, other), Guid.NewGuid(), CancellationToken.None);
        sent.Clear();

        var result = await service.DeleteNodeAsync(project.Id, phase.Id, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ProjectActionStatus.Sent, result.Status);
        var plan = (await service.LoadPlanAsync(project.Id, CancellationToken.None))!;
        Assert.Equal(other.Id, Assert.Single(plan.Nodes).Id);
        Assert.Empty(plan.Dependencies);
        Assert.Equal(0m, plan.Project.PlannedHours);
        Assert.Equal(2, sent.Count);
        Assert.All(sent, call => Assert.Equal(HttpMethod.Delete, call.Method));
        Assert.EndsWith(child.Id.ToString("D"), sent[0].Route, StringComparison.Ordinal);
        Assert.EndsWith(phase.Id.ToString("D"), sent[1].Route, StringComparison.Ordinal);
        Assert.Null(sent[0].Body);

        Assert.Equal(ProjectValidator.NotFoundKey, (await service.DeleteNodeAsync(project.Id, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None)).Detail);
    }

    /// <summary>
    /// Deleting a dependency recomputes the schedule and queues a DELETE; a 404 from the server still counts as delivered.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task DeleteDependency_RecomputesAndTreatsNotFoundAsDelivered()
    {
        var project = await SeedProjectAsync();
        var first = Node(project, "First", start: 1, end: 3);
        var second = Node(project, "Second", start: 1, end: 2);
        _ = await service.SaveNodeAsync(first, Guid.NewGuid(), CancellationToken.None);
        _ = await service.SaveNodeAsync(second, Guid.NewGuid(), CancellationToken.None);
        var link = Link(project, first, second);
        _ = await service.SaveDependencyAsync(link, Guid.NewGuid(), CancellationToken.None);
        reply = _ => Reply(HttpStatusCode.NotFound, "{}");

        var result = await service.DeleteDependencyAsync(project.Id, link.Id, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ProjectActionStatus.Sent, result.Status);
        var plan = (await service.LoadPlanAsync(project.Id, CancellationToken.None))!;
        Assert.Empty(plan.Dependencies);
        Assert.Equal(ProjectTestData.Day(1), plan.Nodes.Single(node => node.Id == second.Id).EarlyStart);
        Assert.Empty(await store.PendingAsync(CancellationToken.None));
    }

    /// <summary>
    /// A refusal from the server keeps the local change, reports a conflict with the server message and can be dismissed.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ServerRefusal_IsAConflictThatCanBeDismissed()
    {
        reply = _ => Reply((HttpStatusCode)422, "{\"message\":\"The project code is taken.\"}");
        var project = ProjectTestData.Project("P-422");

        var result = await service.SaveProjectAsync(project, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ProjectActionStatus.Conflict, result.Status);
        Assert.Equal("The project code is taken.", result.Detail);
        Assert.NotNull(await store.FindProjectAsync(project.Id, CancellationToken.None));
        var portfolio = await service.LoadPortfolioAsync(refresh: false, CancellationToken.None);
        var conflict = Assert.Single(portfolio.Conflicts);
        Assert.Equal("P-422", conflict.ProjectCode);

        await service.DismissConflictAsync(conflict.EntryId, CancellationToken.None);

        Assert.Empty((await service.LoadPortfolioAsync(refresh: false, CancellationToken.None)).Conflicts);
        Assert.False(await store.HasUnsentChangesAsync(project.Id, CancellationToken.None));
    }

    /// <summary>
    /// A transient server error leaves the entry queued with a retry time; the next flush after that time delivers it.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task TransientError_StaysQueuedUntilTheRetryTime()
    {
        reply = _ => Reply(HttpStatusCode.ServiceUnavailable, "{}");
        var project = ProjectTestData.Project("P-503");

        var result = await service.SaveProjectAsync(project, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ProjectActionStatus.Queued, result.Status);
        var entry = Assert.Single(await store.PendingAsync(CancellationToken.None));
        Assert.Equal(1, entry.Attempts);
        Assert.Equal("HTTP 503", entry.LastError);

        reply = _ => Reply(HttpStatusCode.OK, "{}");
        var tooEarly = await service.FlushAsync(CancellationToken.None);
        Assert.True(tooEarly.Offline);
        Assert.Equal(1, tooEarly.Remaining);

        clock.Advance(TimeSpan.FromMinutes(1));
        var flushed = await service.FlushAsync(CancellationToken.None);

        Assert.Equal(1, flushed.Sent);
        Assert.Equal(0, flushed.Remaining);
        Assert.False(flushed.Offline);
    }

    /// <summary>
    /// A network failure keeps the order of the queue and a later flush replays it in the order the changes were made.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_AfterBeingOffline_ReplaysInOrder()
    {
        _ = api.SetupGet(client => client.IsAuthenticated).Returns(value: false);
        var project = await SeedProjectAsync();
        var first = Node(project, "First", start: 1, end: 2);
        var second = Node(project, "Second", start: 1, end: 2);
        _ = await service.SaveNodeAsync(first, Guid.NewGuid(), CancellationToken.None);
        _ = await service.SaveNodeAsync(second, Guid.NewGuid(), CancellationToken.None);
        _ = await service.SaveDependencyAsync(Link(project, first, second), Guid.NewGuid(), CancellationToken.None);
        Assert.Empty(sent);

        _ = api.SetupGet(client => client.IsAuthenticated).Returns(value: true);
        var result = await service.FlushAsync(CancellationToken.None);

        Assert.Equal(4, result.Sent);
        Assert.Equal(0, result.Remaining);
        Assert.Equal(
            [HttpMethod.Put, HttpMethod.Put, HttpMethod.Put, HttpMethod.Put],
            sent.Select(call => call.Method).ToArray());
        Assert.EndsWith("projects/" + project.Id.ToString("D"), sent[0].Route, StringComparison.Ordinal);
        Assert.Contains("/wbs-nodes/" + first.Id.ToString("D"), sent[1].Route, StringComparison.Ordinal);
        Assert.Contains("/wbs-nodes/" + second.Id.ToString("D"), sent[2].Route, StringComparison.Ordinal);
        Assert.Contains("/dependencies/", sent[3].Route, StringComparison.Ordinal);
    }

    /// <summary>
    /// A network exception during the flush keeps the entry queued and reports the service as offline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_NetworkException_KeepsTheQueue()
    {
        _ = api
            .Setup(client => client.SendAsync(It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<string?>(), Any))
            .ThrowsAsync(new HttpRequestException("down"));

        var result = await service.SaveProjectAsync(ProjectTestData.Project("P-NET"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ProjectActionStatus.Queued, result.Status);
        var entry = Assert.Single(await store.PendingAsync(CancellationToken.None));
        Assert.Equal("down", entry.LastError);
    }

    /// <summary>
    /// A baseline freezes the plan; later changes show up in the variance.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Baseline_CapturesThePlanAndComparesLaterChanges()
    {
        var project = await SeedProjectAsync();
        var work = Node(project, "Work", start: 1, end: 5, hours: 10m, cost: 100m);
        _ = await service.SaveNodeAsync(work, Guid.NewGuid(), CancellationToken.None);

        var captured = await service.CaptureBaselineAsync(project.Id, "  Kick-off  ", Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ProjectActionStatus.Sent, captured.Status);
        Assert.NotNull(captured.EntityId);
        var edited = Clone(work);
        edited.EndDate = ProjectTestData.Day(8);
        edited.PlannedHours = 14m;
        _ = await service.SaveNodeAsync(edited, Guid.NewGuid(), CancellationToken.None);
        var second = await service.CaptureBaselineAsync(project.Id, string.Empty, Guid.NewGuid(), CancellationToken.None);

        var plan = (await service.LoadPlanAsync(project.Id, CancellationToken.None))!;
        Assert.Equal(["Baseline 2", "Kick-off"], plan.Baselines.Select(baseline => baseline.Name).ToArray());
        var variance = await service.LoadVarianceAsync(project.Id, captured.EntityId!.Value, CancellationToken.None);
        Assert.NotNull(variance);
        Assert.Equal(1, variance.BaselineNumber);
        Assert.Equal(4m, variance.HoursVariance);
        Assert.Equal(3, variance.FinishVarianceDays);
        var row = Assert.Single(variance.Nodes);
        Assert.Equal(VarianceStatus.Changed, row.Status);
        Assert.Equal(3, row.FinishVarianceDays);
        Assert.Contains(sent, call => call.Method == HttpMethod.Post && call.Route.EndsWith("/baselines", StringComparison.Ordinal));
        Assert.Equal(ProjectActionStatus.Sent, second.Status);
        Assert.Null(await service.LoadVarianceAsync(project.Id, Guid.NewGuid(), CancellationToken.None));
        Assert.Equal(ProjectValidator.BaselineNameKey, (await service.CaptureBaselineAsync(project.Id, new string('x', 201), Guid.NewGuid(), CancellationToken.None)).Detail);
    }

    /// <summary>
    /// A project is created from a template: the copy is scheduled, saved, queued with its whole plan and keeps the template untouched.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task InstantiateTemplate_CopiesThePlanAndQueuesIt()
    {
        var template = ProjectTestData.Project("TPL");
        template.IsTemplate = true;
        _ = await service.SaveProjectAsync(template, Guid.NewGuid(), CancellationToken.None);
        var first = Node(template, "Design", start: 1, end: 3, hours: 8m, cost: 80m);
        var second = Node(template, "Build", start: 4, end: 9, hours: 16m, cost: 160m);
        _ = await service.SaveNodeAsync(first, Guid.NewGuid(), CancellationToken.None);
        _ = await service.SaveNodeAsync(second, Guid.NewGuid(), CancellationToken.None);
        _ = await service.SaveDependencyAsync(Link(template, first, second), Guid.NewGuid(), CancellationToken.None);
        sent.Clear();

        var request = new TemplateInstantiationRequest(template.Id, "NEW-1", "Customer rollout", new DateOnly(2027, 3, 1));
        var result = await service.InstantiateTemplateAsync(request, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ProjectActionStatus.Sent, result.Status);
        var created = (await service.LoadPlanAsync(result.EntityId!.Value, CancellationToken.None))!;
        Assert.Equal("NEW-1", created.Project.ProjectCode);
        Assert.False(created.Project.IsTemplate);
        Assert.Equal(template.Id, created.Project.TemplateSourceId);
        Assert.Equal(24m, created.Project.PlannedHours);
        Assert.Equal(2, created.Nodes.Count);
        Assert.Single(created.Dependencies);
        Assert.Equal(new DateOnly(2027, 3, 1), created.Nodes[0].StartDate);
        Assert.Equal(new DateOnly(2027, 3, 4), created.Nodes[1].EarlyStart);
        var call = Assert.Single(sent);
        Assert.Equal(HttpMethod.Post, call.Method);
        Assert.Equal("projects/" + template.Id.ToString("D") + "/instantiate", call.Route);
        var body = JsonNode.Parse(call.Body!)!.AsObject();
        Assert.Equal(2, body["wbs_nodes"]!.AsArray().Count);
        Assert.Single(body["dependencies"]!.AsArray());
        Assert.Equal("NEW-1", body["project"]!["project_code"]!.GetValue<string>());
        Assert.Equal(2, (await service.LoadPlanAsync(template.Id, CancellationToken.None))!.Nodes.Count);

        Assert.Equal(ProjectValidator.DuplicateCodeKey, (await service.InstantiateTemplateAsync(request, Guid.NewGuid(), CancellationToken.None)).Detail);
        Assert.Equal(ProjectValidator.NotFoundKey, (await service.InstantiateTemplateAsync(request with { TemplateId = Guid.NewGuid(), ProjectCode = "OTHER" }, Guid.NewGuid(), CancellationToken.None)).Detail);
        Assert.Equal(ProjectValidator.CupKey, (await service.InstantiateTemplateAsync(request with { ProjectCode = "OTHER", CupCode = "BAD" }, Guid.NewGuid(), CancellationToken.None)).Detail);
    }

    /// <summary>
    /// Pulling stores the projects, nodes, dependencies and baselines of the server, but never overwrites a project with undelivered changes.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Pull_StoresRemoteProjectsAndKeepsLocalPendingOnes()
    {
        var remote = ProjectTestData.Project("REMOTE");
        var remoteNode = ProjectTestData.Node("1", 1, 4, remote.Id, hours: 6m);
        var remoteNode2 = ProjectTestData.Node("2", 5, 6, remote.Id, hours: 2m);
        var remoteLink = ProjectTestData.Link(remoteNode, remoteNode2);
        var local = ProjectTestData.Project("LOCAL");
        _ = api.SetupGet(client => client.IsAuthenticated).Returns(value: false);
        _ = await service.SaveProjectAsync(local, Guid.NewGuid(), CancellationToken.None);
        _ = api.SetupGet(client => client.IsAuthenticated).Returns(value: true);
        var serverLocal = ProjectTestData.Project("LOCAL-SERVER", local.Id);
        _ = api.Setup(client => client.GetProjectsAsync(Any)).ReturnsAsync(Json(remote, serverLocal));
        _ = api.Setup(client => client.GetNodesAsync(remote.Id, Any)).ReturnsAsync("[" + ProjectPayloadMapper.ToJson(remoteNode).ToJsonString() + "," + ProjectPayloadMapper.ToJson(remoteNode2).ToJsonString() + "]");
        _ = api.Setup(client => client.GetDependenciesAsync(remote.Id, Any)).ReturnsAsync("[" + ProjectPayloadMapper.ToJson(remoteLink).ToJsonString() + "]");
        _ = api.Setup(client => client.GetBaselinesAsync(remote.Id, Any)).ReturnsAsync("[]");
        reply = _ => Reply(HttpStatusCode.ServiceUnavailable, "{}");

        var pull = await service.PullAsync(CancellationToken.None);

        Assert.False(pull.Offline);
        Assert.Equal(1, pull.Projects);
        var plan = (await service.LoadPlanAsync(remote.Id, CancellationToken.None))!;
        Assert.Equal(2, plan.Nodes.Count);
        Assert.Single(plan.Dependencies);
        Assert.Equal(ProjectTestData.Tenant, plan.Project.TenantId);
        Assert.Equal("LOCAL", (await store.FindProjectAsync(local.Id, CancellationToken.None))?.ProjectCode);
        api.Verify(client => client.GetNodesAsync(local.Id, Any), Times.Never);
    }

    /// <summary>
    /// Offline or with a bad answer the pull leaves the local copy as it is and reports offline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Pull_OfflineOrBadAnswer_KeepsTheLocalCopy()
    {
        var project = await SeedProjectAsync();
        _ = api.Setup(client => client.GetProjectsAsync(Any)).ThrowsAsync(new HttpRequestException("down"));

        var failed = await service.PullAsync(CancellationToken.None);

        Assert.True(failed.Offline);
        Assert.NotNull(await store.FindProjectAsync(project.Id, CancellationToken.None));

        _ = api.Setup(client => client.GetProjectsAsync(Any)).ReturnsAsync("{");
        Assert.True((await service.PullAsync(CancellationToken.None)).Offline);

        _ = api.SetupGet(client => client.IsAuthenticated).Returns(value: false);
        Assert.True((await service.PullAsync(CancellationToken.None)).Offline);

        var portfolio = await service.LoadPortfolioAsync(refresh: true, CancellationToken.None);
        Assert.True(portfolio.FromCache);
        Assert.Null(portfolio.SyncedAtUtc);
        Assert.Single(portfolio.Projects);
    }

    /// <summary>
    /// The portfolio lists the projects by code with the queue, and reports the refresh time when the server answered.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task LoadPortfolio_ListsProjectsByCodeAndTheQueue()
    {
        _ = api.SetupGet(client => client.IsAuthenticated).Returns(value: false);
        _ = await service.SaveProjectAsync(ProjectTestData.Project("B-2"), Guid.NewGuid(), CancellationToken.None);
        _ = await service.SaveProjectAsync(ProjectTestData.Project("A-1"), Guid.NewGuid(), CancellationToken.None);

        var offline = await service.LoadPortfolioAsync(refresh: false, CancellationToken.None);

        Assert.Equal(["A-1", "B-2"], offline.Projects.Select(project => project.ProjectCode).ToArray());
        Assert.Equal(2, offline.Pending.Count);
        Assert.True(offline.FromCache);

        _ = api.SetupGet(client => client.IsAuthenticated).Returns(value: true);
        _ = api.Setup(client => client.GetProjectsAsync(Any)).ReturnsAsync("[]");
        var online = await service.LoadPortfolioAsync(refresh: true, CancellationToken.None);

        Assert.False(online.FromCache);
        Assert.NotNull(online.SyncedAtUtc);
        Assert.Empty(online.Pending);
        Assert.Equal(2, online.Projects.Count);
    }

    /// <summary>
    /// An unknown project has no plan.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task LoadPlan_UnknownProject_ReturnsNull()
    {
        Assert.Null(await service.LoadPlanAsync(Guid.NewGuid(), CancellationToken.None));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        db.Dispose();
        connection.Dispose();
    }

    private static SyncApiReply Reply(HttpStatusCode status, string body) => new(status, Encoding.UTF8.GetBytes(body));

    private static string Json(params Project[] projects) => "[" + string.Join(",", projects.Select(project => ProjectPayloadMapper.ToJson(project).ToJsonString())) + "]";

    private static WbsNode Node(Project project, string title, WbsNodeType type = WbsNodeType.WorkPackage, Guid? parent = null, int start = 1, int end = 2, decimal hours = 0m, decimal cost = 0m) => new()
    {
        Id = Guid.NewGuid(),
        ProjectId = project.Id,
        ParentNodeId = parent,
        Title = title,
        NodeType = type,
        StartDate = ProjectTestData.Day(start),
        EndDate = ProjectTestData.Day(end),
        PlannedHours = hours,
        PlannedCost = cost,
    };

    private static WbsNode Clone(WbsNode node) => new()
    {
        Id = node.Id,
        ProjectId = node.ProjectId,
        ParentNodeId = node.ParentNodeId,
        WbsCode = node.WbsCode,
        Title = node.Title,
        NodeType = node.NodeType,
        StartDate = node.StartDate,
        EndDate = node.EndDate,
        PlannedHours = node.PlannedHours,
        PlannedCost = node.PlannedCost,
        PlannedRevenue = node.PlannedRevenue,
        SortOrder = node.SortOrder,
    };

    private static TaskDependency Link(Project project, WbsNode predecessor, WbsNode successor, DependencyType type = DependencyType.FS, int lag = 0) => new()
    {
        Id = Guid.NewGuid(),
        ProjectId = project.Id,
        PredecessorNodeId = predecessor.Id,
        SuccessorNodeId = successor.Id,
        DependencyType = type,
        LagDays = lag,
    };

    private async Task<Project> SeedProjectAsync()
    {
        var project = ProjectTestData.Project();
        _ = await service.SaveProjectAsync(project, Guid.NewGuid(), CancellationToken.None);
        sent.Clear();

        return project;
    }
}
