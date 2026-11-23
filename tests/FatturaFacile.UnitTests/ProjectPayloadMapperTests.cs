// <copyright file="ProjectPayloadMapperTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using FatturaFacile.Application.Projects;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the mapping between the portfolio entities and the JSON of the web-api (DTO and sync payload contract).
/// </summary>
public sealed class ProjectPayloadMapperTests
{
    /// <summary>
    /// A project keeps every field through a write and a read, including CUP, CIG and the rolled-up figures.
    /// </summary>
    [Fact]
    public void Project_RoundTrips()
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            LegalEntityId = Guid.NewGuid(),
            ProjectCode = "P-7",
            Name = "Bridge",
            CustomerId = Guid.NewGuid(),
            ProjectManagerId = Guid.NewGuid(),
            ProjectType = ProjectType.InternalRAndD,
            BillingModel = ProjectBillingModel.SalProgress,
            CostCenterId = Guid.NewGuid(),
            StartDate = new DateOnly(2027, 3, 1),
            TargetEndDate = new DateOnly(2027, 9, 30),
            Status = ProjectStatus.OnHold,
            CupCode = "B12C34D56E78F90",
            CigCode = "ABC1234567",
            PlannedHours = 120.5m,
            ActualHours = 10m,
            PlannedCost = 9000m,
            ActualCost = 800m,
            PlannedRevenue = 15000m,
            PercentComplete = 12.5m,
            IsTemplate = true,
            TemplateSourceId = Guid.NewGuid(),
        };

        var json = ProjectPayloadMapper.ToJson(project);
        var back = Assert.Single(ProjectPayloadMapper.ParseProjects("[" + json.ToJsonString() + "]"));

        Assert.Equal("internal_r_and_d", json["project_type"]?.GetValue<string>());
        Assert.Equal("sal_progress", json["billing_model"]?.GetValue<string>());
        Assert.Equal("on_hold", json["status"]?.GetValue<string>());
        Assert.Equal("2027-03-01", json["start_date"]?.GetValue<string>());
        Assert.Equal(project.Id, back.Id);
        Assert.Equal(project.LegalEntityId, back.LegalEntityId);
        Assert.Equal("P-7", back.ProjectCode);
        Assert.Equal(project.CustomerId, back.CustomerId);
        Assert.Equal(project.ProjectManagerId, back.ProjectManagerId);
        Assert.Equal(ProjectType.InternalRAndD, back.ProjectType);
        Assert.Equal(ProjectBillingModel.SalProgress, back.BillingModel);
        Assert.Equal(project.CostCenterId, back.CostCenterId);
        Assert.Equal(project.StartDate, back.StartDate);
        Assert.Equal(project.TargetEndDate, back.TargetEndDate);
        Assert.Equal(ProjectStatus.OnHold, back.Status);
        Assert.Equal("B12C34D56E78F90", back.CupCode);
        Assert.Equal("ABC1234567", back.CigCode);
        Assert.Equal(120.5m, back.PlannedHours);
        Assert.Equal(15000m, back.PlannedRevenue);
        Assert.Equal(12.5m, back.PercentComplete);
        Assert.True(back.IsTemplate);
        Assert.Equal(project.TemplateSourceId, back.TemplateSourceId);
    }

    /// <summary>
    /// A node keeps its hierarchy, dates and CPM results through a write and a read.
    /// </summary>
    [Fact]
    public void Node_RoundTrips()
    {
        var projectId = Guid.NewGuid();
        var node = ProjectTestData.Node("1.2", 3, 9, projectId, Guid.NewGuid(), WbsNodeType.Deliverable, hours: 16m, cost: 160m, revenue: 240m, percent: 33m);
        node.SortOrder = 4;
        node.EarlyStart = ProjectTestData.Day(3);
        node.EarlyFinish = ProjectTestData.Day(9);
        node.LateStart = ProjectTestData.Day(5);
        node.LateFinish = ProjectTestData.Day(11);
        node.TotalFloatDays = 2;
        node.FreeFloatDays = 1;
        node.IsCritical = true;

        var json = ProjectPayloadMapper.ToJson(node);
        var back = Assert.Single(ProjectPayloadMapper.ParseNodes("{\"data\":[" + json.ToJsonString() + "]}", projectId));

        Assert.Equal("deliverable", json["node_type"]?.GetValue<string>());
        Assert.Equal(node.Id, back.Id);
        Assert.Equal(node.ParentNodeId, back.ParentNodeId);
        Assert.Equal("1.2", back.WbsCode);
        Assert.Equal(WbsNodeType.Deliverable, back.NodeType);
        Assert.Equal(16m, back.PlannedHours);
        Assert.Equal(ProjectTestData.Day(9), back.EndDate);
        Assert.Equal(33m, back.PercentComplete);
        Assert.Equal(4, back.SortOrder);
        Assert.Equal(ProjectTestData.Day(5), back.LateStart);
        Assert.Equal(2, back.TotalFloatDays);
        Assert.Equal(1, back.FreeFloatDays);
        Assert.True(back.IsCritical);
    }

    /// <summary>
    /// A dependency keeps its type and lag, and the wire name of the type is lower case.
    /// </summary>
    [Fact]
    public void Dependency_RoundTrips()
    {
        var projectId = Guid.NewGuid();
        var first = ProjectTestData.Node("1", 1, 2, projectId);
        var second = ProjectTestData.Node("2", 1, 2, projectId);
        var link = ProjectTestData.Link(first, second, DependencyType.FF, -2);

        var json = ProjectPayloadMapper.ToJson(link);
        var back = Assert.Single(ProjectPayloadMapper.ParseDependencies("[" + json.ToJsonString() + "]", projectId));

        Assert.Equal("ff", json["dependency_type"]?.GetValue<string>());
        Assert.Equal(link.Id, back.Id);
        Assert.Equal(first.Id, back.PredecessorNodeId);
        Assert.Equal(second.Id, back.SuccessorNodeId);
        Assert.Equal(DependencyType.FF, back.DependencyType);
        Assert.Equal(-2, back.LagDays);
    }

    /// <summary>
    /// Reading is tolerant: numbers as text, enums as numbers or names, timestamps as dates, an `items` wrapper, and unusable items are skipped.
    /// </summary>
    [Fact]
    public void Parse_IsTolerantAndSkipsUnusableItems()
    {
        var id = Guid.NewGuid();
        var json = "{\"data\":{\"items\":[" +
            "{\"id\":\"" + id + "\",\"project_code\":\"X\",\"name\":\"N\",\"project_type\":1,\"status\":\"on-hold\",\"billing_model\":\"FixedPrice\"," +
            "\"start_date\":\"2027-03-01T00:00:00Z\",\"planned_hours\":\"12.50\",\"is_template\":1}," +
            "{\"project_code\":\"no id\"}," +
            "\"not an object\"]}}";

        var projects = ProjectPayloadMapper.ParseProjects(json);

        var project = Assert.Single(projects);
        Assert.Equal(id, project.Id);
        Assert.Equal(ProjectType.InternalRAndD, project.ProjectType);
        Assert.Equal(ProjectStatus.OnHold, project.Status);
        Assert.Equal(ProjectBillingModel.FixedPrice, project.BillingModel);
        Assert.Equal(new DateOnly(2027, 3, 1), project.StartDate);
        Assert.Equal(12.5m, project.PlannedHours);
        Assert.True(project.IsTemplate);
        Assert.Empty(ProjectPayloadMapper.ParseProjects(string.Empty));
        Assert.Empty(ProjectPayloadMapper.ParseProjects("{\"data\":null}"));
        _ = Assert.ThrowsAny<JsonException>(() => ProjectPayloadMapper.ParseProjects("{"));
    }

    /// <summary>
    /// A dependency without both nodes is skipped, and an unknown enum falls back to the default value.
    /// </summary>
    [Fact]
    public void ParseDependencies_SkipsLinksWithoutNodes()
    {
        var projectId = Guid.NewGuid();
        var good = "{\"id\":\"" + Guid.NewGuid() + "\",\"predecessor_node_id\":\"" + Guid.NewGuid() + "\",\"successor_node_id\":\"" + Guid.NewGuid() + "\",\"dependency_type\":\"zz\"}";
        var bad = "{\"id\":\"" + Guid.NewGuid() + "\",\"predecessor_node_id\":\"" + Guid.NewGuid() + "\"}";

        var links = ProjectPayloadMapper.ParseDependencies("[" + good + "," + bad + "]", projectId);

        var link = Assert.Single(links);
        Assert.Equal(DependencyType.FS, link.DependencyType);
        Assert.Equal(projectId, link.ProjectId);
    }

    /// <summary>
    /// A baseline travels with its snapshot as an object, and a snapshot sent as text is read back as the same text.
    /// </summary>
    [Fact]
    public void Baseline_SnapshotAsObjectOrText()
    {
        var projectId = Guid.NewGuid();
        var baseline = new ProjectBaseline
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            BaselineNumber = 2,
            Name = "Rebaseline",
            CapturedAt = new DateTime(2027, 4, 2, 10, 0, 0, DateTimeKind.Utc),
            SnapshotJson = "{\"planned_hours\":5}",
        };

        var json = ProjectPayloadMapper.ToJson(baseline);
        var fromObject = Assert.Single(ProjectPayloadMapper.ParseBaselines("[" + json.ToJsonString() + "]", projectId));
        var asText = "[{\"id\":\"" + baseline.Id + "\",\"snapshot_json\":\"{\\\"planned_hours\\\":5}\",\"captured_at\":\"2027-04-02T10:00:00Z\"}]";
        var fromText = Assert.Single(ProjectPayloadMapper.ParseBaselines(asText, projectId));

        Assert.Equal(JsonValueKind.Object, JsonElement.Parse(json.ToJsonString()).GetProperty("snapshot_json").ValueKind);
        Assert.Equal(2, fromObject.BaselineNumber);
        Assert.Equal("Rebaseline", fromObject.Name);
        Assert.Equal(baseline.CapturedAt, fromObject.CapturedAt);
        Assert.Equal("{\"planned_hours\":5}", fromObject.SnapshotJson);
        Assert.Equal("{\"planned_hours\":5}", fromText.SnapshotJson);
    }
}
