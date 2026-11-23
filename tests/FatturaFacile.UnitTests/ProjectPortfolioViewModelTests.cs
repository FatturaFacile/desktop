// <copyright file="ProjectPortfolioViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Projects;
using FatturaFacile.Application.Projects;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the project portfolio view model: portfolio KPIs, WBS tree, Gantt, Kanban, baseline comparison and the commands.
/// </summary>
public sealed class ProjectPortfolioViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();

    private readonly Mock<ISender> sender = new();
    private readonly ProjectPortfolioViewModel viewModel;
    private readonly Project project = ProjectTestData.Project("P-1");
    private readonly WbsNode design;
    private readonly WbsNode build;
    private readonly WbsNode gate;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectPortfolioViewModelTests"/> class.
    /// </summary>
    public ProjectPortfolioViewModelTests()
    {
        Mock<ILocalizationService> localization = new();
        _ = localization.Setup(l => l[It.IsAny<string>()]).Returns((string key) => key switch
        {
            "Project_CriticalPath" => "{0}",
            "Project_Var_Summary" => "{0}|{1}|{2}|{3}|{4}",
            _ => "[" + key + "]",
        });
        _ = localization.SetupGet(l => l.CurrentCulture).Returns(System.Globalization.CultureInfo.InvariantCulture);

        design = ProjectTestData.Node("1", 1, 3, project.Id, hours: 8m, percent: 100m);
        build = ProjectTestData.Node("2", 4, 8, project.Id, hours: 16m, percent: 40m);
        gate = ProjectTestData.Node("3", 9, 9, project.Id, type: WbsNodeType.Milestone);
        var phase = ProjectTestData.Node("0", 1, 1, project.Id, type: WbsNodeType.Phase);
        phase.SortOrder = 0;
        design.ParentNodeId = phase.Id;
        build.ParentNodeId = phase.Id;
        design.SortOrder = 1;
        build.SortOrder = 2;
        gate.SortOrder = 3;
        project.PlannedHours = 24m;
        project.PercentComplete = 60m;
        WbsNode[] nodes = [phase, design, build, gate];
        TaskDependency[] links = [ProjectTestData.Link(design, build), ProjectTestData.Link(build, gate)];
        var plan = new ProjectPlan(project, nodes, links, [], CriticalPathEngine.Calculate(nodes, links, ProjectTestData.Day(1)));

        _ = sender
            .Setup(s => s.Send(It.IsAny<GetProjectPortfolioQuery>(), Any))
            .ReturnsAsync(new ProjectPortfolio([project], [], [], FromCache: false, DateTime.UtcNow));
        _ = sender.Setup(s => s.Send(It.IsAny<GetProjectPlanQuery>(), Any)).ReturnsAsync(plan);
        _ = sender
            .Setup(s => s.Send(It.IsAny<SaveProjectCommand>(), Any))
            .ReturnsAsync(new ProjectActionResult(ProjectActionStatus.Sent));
        _ = sender
            .Setup(s => s.Send(It.IsAny<SaveWbsNodeCommand>(), Any))
            .ReturnsAsync(new ProjectActionResult(ProjectActionStatus.Queued));

        viewModel = new ProjectPortfolioViewModel(sender.Object, localization.Object, TimeProvider.System);
    }

    /// <summary>
    /// Loading shows the portfolio KPIs and selects the first project.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ShowsTheOverviewAndSelectsTheFirstProject()
    {
        await viewModel.LoadCommand.ExecuteAsync(parameter: null);

        var row = Assert.Single(viewModel.Projects);
        Assert.Equal("P-1", row.Code);
        Assert.Equal(row, viewModel.SelectedProject);
        Assert.Equal("1", viewModel.ProjectCountText);
        Assert.Equal("24.00", viewModel.TotalHoursText);
        Assert.Equal("60.0%", viewModel.AverageProgressText);
        Assert.Equal("[Project_Status_Synced]", viewModel.StatusMessage);
        Assert.False(viewModel.IsOffline);
        Assert.True(viewModel.HasPlan);
    }

    /// <summary>
    /// The plan fills the WBS tree with depth and float, the Gantt with critical and milestone bars, the Kanban by progress and the dependency list.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Plan_FillsTreeGanttKanbanAndDependencies()
    {
        await viewModel.LoadCommand.ExecuteAsync(parameter: null);

        Assert.Equal(4, viewModel.WbsRows.Count);
        Assert.Equal<string>(["0", "1", "2", "3"], [.. viewModel.WbsRows.Select(row => row.Code)]);
        Assert.True(viewModel.WbsRows[0].IsSummary);
        Assert.Equal(0.0, viewModel.WbsRows[0].Indent.Left);
        Assert.True(viewModel.WbsRows[1].Indent.Left > 0.0);
        Assert.Equal("0", viewModel.WbsRows[2].FloatText);
        Assert.True(viewModel.WbsRows[2].IsCritical);

        Assert.Equal(4, viewModel.GanttBars.Count);
        var milestoneBar = viewModel.GanttBars.Single(bar => bar.IsMilestone);
        Assert.Contains("3", milestoneBar.Label, StringComparison.Ordinal);
        Assert.True(viewModel.GanttBars.Single(bar => bar.IsSummary).Width > 0.0);
        Assert.True(viewModel.GanttBars.Where(bar => !bar.IsSummary && !bar.IsMilestone).All(bar => bar.IsCritical));
        Assert.True(viewModel.GanttWidth > 0.0);

        Assert.Equal(3, viewModel.KanbanColumns.Count);
        Assert.Equal<string>(["2"], [.. viewModel.KanbanColumns[1].Cards.Select(card => card.Code)]);
        Assert.Equal<string>(["1"], [.. viewModel.KanbanColumns[2].Cards.Select(card => card.Code)]);
        Assert.Equal<string>(["3"], [.. viewModel.KanbanColumns[0].Cards.Select(card => card.Code)]);

        Assert.Equal(2, viewModel.Dependencies.Count);
        Assert.Equal("1 > 2 (FS)", viewModel.Dependencies[0].Text);
        Assert.Equal("1 > 2 > 3", viewModel.CriticalPathText);
    }

    /// <summary>
    /// Selecting a node copies it into the node editor.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SelectingANode_FillsTheNodeEditor()
    {
        await viewModel.LoadCommand.ExecuteAsync(parameter: null);

        viewModel.SelectedNode = viewModel.WbsRows.Single(row => string.Equals(row.Code, "2", StringComparison.Ordinal));

        Assert.Equal("Node 2", viewModel.NodeTitle);
        Assert.Equal(16m, viewModel.NodeHours);
        Assert.Equal(40m, viewModel.NodePercent);
        Assert.Equal(WbsNodeType.WorkPackage, viewModel.SelectedNodeType?.Value);
    }

    /// <summary>
    /// Saving a project sends the editor fields (trimmed server-side) and reports the localized outcome, then reloads.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveProject_SendsTheEditorFieldsAndReports()
    {
        await viewModel.LoadCommand.ExecuteAsync(parameter: null);
        viewModel.NewProjectCommand.Execute(parameter: null);
        viewModel.EditCode = "NEW-9";
        viewModel.EditName = "New bridge";
        viewModel.EditCup = "B12C34D56E78F90";
        viewModel.EditIsTemplate = true;

        await viewModel.SaveProjectCommand.ExecuteAsync(parameter: null);

        sender.Verify(
            s => s.Send(It.Is<SaveProjectCommand>(c => IsNewBridge(c) && c.Project.IsTemplate && c.CommandId != Guid.Empty), Any),
            Times.Once);
        Assert.Equal("[Project_Saved]", viewModel.StatusMessage);
    }

    /// <summary>
    /// A rejected change shows the localized validation message and does not reload.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveProject_Rejected_ShowsTheLocalizedError()
    {
        await viewModel.LoadCommand.ExecuteAsync(parameter: null);
        _ = sender
            .Setup(s => s.Send(It.IsAny<SaveProjectCommand>(), Any))
            .ReturnsAsync(new ProjectActionResult(ProjectActionStatus.Rejected, ProjectValidator.CupKey));

        await viewModel.SaveProjectCommand.ExecuteAsync(parameter: null);

        Assert.Equal("[Project_Err_Cup]", viewModel.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<GetProjectPortfolioQuery>(), Any), Times.Once);
    }

    /// <summary>
    /// A queued change says it is waiting for the network.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task AddRootNode_Queued_ReportsTheQueue()
    {
        await viewModel.LoadCommand.ExecuteAsync(parameter: null);
        viewModel.NodeTitle = "Commissioning";
        viewModel.NodeHours = 12m;

        await viewModel.AddRootNodeCommand.ExecuteAsync(parameter: null);

        sender.Verify(
            s => s.Send(It.Is<SaveWbsNodeCommand>(c => c.Node.Title == "Commissioning" && c.Node.PlannedHours == 12m && c.Node.ParentNodeId == null && c.Node.ProjectId == project.Id), Any),
            Times.Once);
        Assert.Equal("[Project_Queued]", viewModel.StatusMessage);
    }

    /// <summary>
    /// Adding a child uses the selected node as the parent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task AddChildNode_UsesTheSelectedNodeAsTheParent()
    {
        await viewModel.LoadCommand.ExecuteAsync(parameter: null);
        var phase = viewModel.WbsRows.Single(row => string.Equals(row.Code, "0", StringComparison.Ordinal));
        viewModel.SelectedNode = phase;
        viewModel.NodeTitle = "Child";

        await viewModel.AddChildNodeCommand.ExecuteAsync(parameter: null);

        sender.Verify(s => s.Send(It.Is<SaveWbsNodeCommand>(c => c.Node.ParentNodeId == phase.Id && c.Node.Title == "Child"), Any), Times.Once);
    }

    /// <summary>
    /// Deleting sends the project and the selected node.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task DeleteNode_SendsTheSelectedNode()
    {
        _ = sender
            .Setup(s => s.Send(It.IsAny<DeleteWbsNodeCommand>(), Any))
            .ReturnsAsync(new ProjectActionResult(ProjectActionStatus.Sent));
        await viewModel.LoadCommand.ExecuteAsync(parameter: null);
        var target = viewModel.WbsRows.Single(row => string.Equals(row.Code, "2", StringComparison.Ordinal));
        viewModel.SelectedNode = target;

        await viewModel.DeleteNodeCommand.ExecuteAsync(parameter: null);

        sender.Verify(s => s.Send(It.Is<DeleteWbsNodeCommand>(c => c.ProjectId == project.Id && c.NodeId == target.Id), Any), Times.Once);
        Assert.Equal("[Project_Saved]", viewModel.StatusMessage);
    }

    /// <summary>
    /// Without a selected node the node commands only show the hint.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task NodeCommands_WithoutSelection_ShowTheHint()
    {
        await viewModel.LoadCommand.ExecuteAsync(parameter: null);
        viewModel.SelectedNode = null;

        await viewModel.UpdateNodeCommand.ExecuteAsync(parameter: null);
        Assert.Equal("[Project_Err_Select]", viewModel.StatusMessage);

        viewModel.StatusMessage = string.Empty;
        await viewModel.DeleteNodeCommand.ExecuteAsync(parameter: null);
        Assert.Equal("[Project_Err_Select]", viewModel.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<DeleteWbsNodeCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// The dependency editor sends the chosen predecessor, successor, type and lag.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task AddDependency_SendsTheEditorChoices()
    {
        _ = sender
            .Setup(s => s.Send(It.IsAny<SaveTaskDependencyCommand>(), Any))
            .ReturnsAsync(new ProjectActionResult(ProjectActionStatus.Sent));
        await viewModel.LoadCommand.ExecuteAsync(parameter: null);
        viewModel.DependencyPredecessor = viewModel.WbsRows.Single(row => string.Equals(row.Code, "1", StringComparison.Ordinal));
        viewModel.DependencySuccessor = viewModel.WbsRows.Single(row => string.Equals(row.Code, "3", StringComparison.Ordinal));
        viewModel.SelectedDependencyType = viewModel.DependencyTypes.Single(choice => choice.Value == DependencyType.SS);
        viewModel.DependencyLag = 2;

        await viewModel.AddDependencyCommand.ExecuteAsync(parameter: null);

        sender.Verify(
            s => s.Send(It.Is<SaveTaskDependencyCommand>(c => c.Dependency.PredecessorNodeId == design.Id && c.Dependency.SuccessorNodeId == gate.Id && c.Dependency.DependencyType == DependencyType.SS && c.Dependency.LagDays == 2), Any),
            Times.Once);
    }

    /// <summary>
    /// Removing a dependency sends its identifier.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task RemoveDependency_SendsTheIdentifier()
    {
        _ = sender
            .Setup(s => s.Send(It.IsAny<DeleteTaskDependencyCommand>(), Any))
            .ReturnsAsync(new ProjectActionResult(ProjectActionStatus.Sent));
        await viewModel.LoadCommand.ExecuteAsync(parameter: null);
        var row = viewModel.Dependencies[0];

        await viewModel.RemoveDependencyCommand.ExecuteAsync(row);

        sender.Verify(s => s.Send(It.Is<DeleteTaskDependencyCommand>(c => c.DependencyId == row.Id && c.ProjectId == project.Id), Any), Times.Once);
    }

    /// <summary>
    /// Selecting a baseline shows the variance rows and the localized summary.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SelectingABaseline_ShowsTheVariance()
    {
        var baselineId = Guid.NewGuid();
        var baseline = new ProjectBaseline { Id = baselineId, ProjectId = project.Id, BaselineNumber = 1, Name = "Kick-off", CapturedAt = DateTime.UtcNow };
        WbsNode[] nodes = [design];
        TaskDependency[] links = [];
        var plan = new ProjectPlan(project, nodes, links, [baseline], CriticalPathEngine.Calculate(nodes, links, ProjectTestData.Day(1)));
        _ = sender.Setup(s => s.Send(It.IsAny<GetProjectPlanQuery>(), Any)).ReturnsAsync(plan);
        var variance = new BaselineVariance(
            baselineId,
            1,
            "Kick-off",
            DateTime.UtcNow,
            StartVarianceDays: 0,
            FinishVarianceDays: 3,
            HoursVariance: 4m,
            CostVariance: 0m,
            RevenueVariance: 0m,
            [new NodeVariance(design.Id, "1", "Node 1", VarianceStatus.Changed, 0, 3, 4m, 0m, 0m)]);
        _ = sender.Setup(s => s.Send(It.IsAny<GetBaselineVarianceQuery>(), Any)).ReturnsAsync(variance);
        await viewModel.LoadCommand.ExecuteAsync(parameter: null);

        viewModel.SelectedBaseline = Assert.Single(viewModel.Baselines);
        for (var attempt = 0; attempt < 100 && viewModel.VarianceRows.Count == 0; attempt++)
        {
            await Task.Delay(20, CancellationToken.None);
        }

        var row = Assert.Single(viewModel.VarianceRows);
        Assert.Equal("Changed", row.Status);
        Assert.Equal("[Project_Var_Changed]", row.StatusText);
        Assert.Equal("+3", row.FinishText);
        Assert.Equal("+4", row.HoursText);
        Assert.Equal("0|+3|+4|0|0", viewModel.VarianceSummary);
    }

    /// <summary>
    /// Creating from the selected project sends the template request with the chosen code, name and start.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task InstantiateTemplate_SendsTheRequest()
    {
        var created = Guid.NewGuid();
        _ = sender
            .Setup(s => s.Send(It.IsAny<InstantiateProjectTemplateCommand>(), Any))
            .ReturnsAsync(new ProjectActionResult(ProjectActionStatus.Sent, EntityId: created));
        await viewModel.LoadCommand.ExecuteAsync(parameter: null);
        viewModel.TemplateCode = "NEW-2";
        viewModel.TemplateName = "Clone";
        viewModel.TemplateStart = new DateTime(2027, 5, 3);

        await viewModel.InstantiateTemplateCommand.ExecuteAsync(parameter: null);

        sender.Verify(
            s => s.Send(It.Is<InstantiateProjectTemplateCommand>(c => c.Request.TemplateId == project.Id && c.Request.ProjectCode == "NEW-2" && c.Request.Name == "Clone" && c.Request.StartDate == new DateOnly(2027, 5, 3)), Any),
            Times.Once);
        Assert.Equal(string.Empty, viewModel.TemplateCode);
    }

    /// <summary>
    /// Queued and refused changes show in the pending text, and dismissing sends one command per refused change.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Queue_ShowsPendingTextAndDismissesConflicts()
    {
        var entry = new ProjectPendingChange(Guid.NewGuid(), ProjectOutboxAction.UpsertNode, project.Id, "P-1", DateTime.UtcNow, "refused");
        _ = sender
            .Setup(s => s.Send(It.IsAny<GetProjectPortfolioQuery>(), Any))
            .ReturnsAsync(new ProjectPortfolio([project], [entry], [entry], FromCache: true, SyncedAtUtc: null));

        await viewModel.LoadCommand.ExecuteAsync(parameter: null);

        Assert.True(viewModel.IsOffline);
        Assert.True(viewModel.HasConflicts);
        Assert.NotEmpty(viewModel.PendingText);
        Assert.Equal("[Project_OfflineUsingCache]", viewModel.StatusMessage);

        await viewModel.DismissConflictsCommand.ExecuteAsync(parameter: null);

        sender.Verify(s => s.Send(It.Is<DismissProjectConflictCommand>(c => c.EntryId == entry.EntryId), Any), Times.Once);
    }

    /// <summary>
    /// An empty portfolio shows the empty notice and no plan.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_EmptyPortfolio_ShowsTheNotice()
    {
        _ = sender
            .Setup(s => s.Send(It.IsAny<GetProjectPortfolioQuery>(), Any))
            .ReturnsAsync(new ProjectPortfolio([], [], [], FromCache: false, DateTime.UtcNow));

        await viewModel.LoadCommand.ExecuteAsync(parameter: null);

        Assert.Empty(viewModel.Projects);
        Assert.False(viewModel.HasPlan);
        Assert.Equal("[Project_NoProjects]", viewModel.StatusMessage);
    }

    private static bool IsNewBridge(SaveProjectCommand c) => string.Equals(c.Project.ProjectCode, "NEW-9", StringComparison.Ordinal) && string.Equals(c.Project.Name, "New bridge", StringComparison.Ordinal) && string.Equals(c.Project.CupCode, "B12C34D56E78F90", StringComparison.Ordinal);
}
