// <copyright file="ProjectPortfolioViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Projects;
using FatturaFacile.Application.Projects;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Drives the project portfolio and WBS view (Mod 078): the portfolio overview, the hierarchical WBS with its editors, the Gantt
/// diagram with the critical path, the Kanban board, the dependencies, the baseline comparison and the creation of a project from a
/// template. Everything is read from and written to the local database, so it works offline; changes wait in the outbox until the
/// web-api is reachable.
/// </summary>
internal sealed partial class ProjectPortfolioViewModel : ObservableObject
{
    private const double DayWidth = 14.0;
    private const double MilestoneWidth = 14.0;
    private const double PercentScale = 100.0;
    private const string NoValue = "-";
    private const string NumberFormat = "N2";
    private const string SignedFormat = "+#,##0.##;-#,##0.##;0";
    private const double IndentStep = 18.0;

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly TimeProvider _time;

    private ProjectPlan? _plan;
    private bool _suppressSelection;

    [ObservableProperty]
    private ProjectPortfolioRow? _selectedProject;

    [ObservableProperty]
    private WbsRow? _selectedNode;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private bool _hasPlan;

    [ObservableProperty]
    private bool _hasConflicts;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _pendingText = string.Empty;

    [ObservableProperty]
    private string _projectCountText = NoValue;

    [ObservableProperty]
    private string _totalHoursText = NoValue;

    [ObservableProperty]
    private string _totalCostText = NoValue;

    [ObservableProperty]
    private string _totalRevenueText = NoValue;

    [ObservableProperty]
    private string _averageProgressText = NoValue;

    [ObservableProperty]
    private string _criticalPathText = string.Empty;

    [ObservableProperty]
    private string _projectFinishText = string.Empty;

    [ObservableProperty]
    private double _ganttWidth = 400.0;

    [ObservableProperty]
    private Guid? _editingProjectId;

    [ObservableProperty]
    private string _editCode = string.Empty;

    [ObservableProperty]
    private string _editName = string.Empty;

    [ObservableProperty]
    private string _editCup = string.Empty;

    [ObservableProperty]
    private string _editCig = string.Empty;

    [ObservableProperty]
    private DateTime? _editStart;

    [ObservableProperty]
    private DateTime? _editEnd;

    [ObservableProperty]
    private bool _editIsTemplate;

    [ObservableProperty]
    private ProjectChoice<ProjectType>? _selectedProjectType;

    [ObservableProperty]
    private ProjectChoice<ProjectBillingModel>? _selectedBillingModel;

    [ObservableProperty]
    private ProjectChoice<ProjectStatus>? _selectedStatus;

    [ObservableProperty]
    private string _nodeTitle = string.Empty;

    [ObservableProperty]
    private ProjectChoice<WbsNodeType>? _selectedNodeType;

    [ObservableProperty]
    private DateTime? _nodeStart;

    [ObservableProperty]
    private DateTime? _nodeEnd;

    [ObservableProperty]
    private decimal _nodeHours;

    [ObservableProperty]
    private decimal _nodeCost;

    [ObservableProperty]
    private decimal _nodeRevenue;

    [ObservableProperty]
    private decimal _nodePercent;

    [ObservableProperty]
    private WbsRow? _dependencyPredecessor;

    [ObservableProperty]
    private WbsRow? _dependencySuccessor;

    [ObservableProperty]
    private ProjectChoice<DependencyType>? _selectedDependencyType;

    [ObservableProperty]
    private int _dependencyLag;

    [ObservableProperty]
    private string _baselineName = string.Empty;

    [ObservableProperty]
    private ProjectChoice<Guid>? _selectedBaseline;

    [ObservableProperty]
    private string _varianceSummary = string.Empty;

    [ObservableProperty]
    private string _templateCode = string.Empty;

    [ObservableProperty]
    private string _templateName = string.Empty;

    [ObservableProperty]
    private DateTime? _templateStart;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectPortfolioViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="time">The clock.</param>
    public ProjectPortfolioViewModel(ISender sender, ILocalizationService localization, TimeProvider time)
    {
        _sender = sender;
        _localization = localization;
        _time = time;

        ProjectTypes = [.. Enum.GetValues<ProjectType>().Select(value => new ProjectChoice<ProjectType>(value, _localization["Project_Type_" + value]))];
        BillingModels = [.. Enum.GetValues<ProjectBillingModel>().Select(value => new ProjectChoice<ProjectBillingModel>(value, _localization["Project_Billing_" + value]))];
        Statuses = [.. Enum.GetValues<ProjectStatus>().Select(value => new ProjectChoice<ProjectStatus>(value, _localization["Project_Status_" + value]))];
        NodeTypes = [.. Enum.GetValues<WbsNodeType>().Select(value => new ProjectChoice<WbsNodeType>(value, _localization["Project_NodeType_" + value]))];
        DependencyTypes = [.. Enum.GetValues<DependencyType>().Select(value => new ProjectChoice<DependencyType>(value, _localization["Project_DepType_" + value]))];

        ResetProjectEditor();
        ResetNodeEditor();
        SelectedDependencyType = DependencyTypes[0];
        TemplateStart = _time.GetLocalNow().Date;
    }

    /// <summary>
    /// Gets the project types of the picker.
    /// </summary>
    public IReadOnlyList<ProjectChoice<ProjectType>> ProjectTypes { get; }

    /// <summary>
    /// Gets the billing models of the picker.
    /// </summary>
    public IReadOnlyList<ProjectChoice<ProjectBillingModel>> BillingModels { get; }

    /// <summary>
    /// Gets the statuses of the picker.
    /// </summary>
    public IReadOnlyList<ProjectChoice<ProjectStatus>> Statuses { get; }

    /// <summary>
    /// Gets the node types of the picker.
    /// </summary>
    public IReadOnlyList<ProjectChoice<WbsNodeType>> NodeTypes { get; }

    /// <summary>
    /// Gets the dependency types of the picker.
    /// </summary>
    public IReadOnlyList<ProjectChoice<DependencyType>> DependencyTypes { get; }

    /// <summary>
    /// Gets the projects of the portfolio overview.
    /// </summary>
    internal ObservableCollection<ProjectPortfolioRow> Projects { get; } = [];

    /// <summary>
    /// Gets the WBS tree rows of the selected project.
    /// </summary>
    internal ObservableCollection<WbsRow> WbsRows { get; } = [];

    /// <summary>
    /// Gets the Gantt lines of the selected project.
    /// </summary>
    internal ObservableCollection<GanttBarRow> GanttBars { get; } = [];

    /// <summary>
    /// Gets the Kanban columns of the selected project.
    /// </summary>
    internal ObservableCollection<KanbanColumnRow> KanbanColumns { get; } = [];

    /// <summary>
    /// Gets the dependencies of the selected project.
    /// </summary>
    internal ObservableCollection<ProjectDependencyRow> Dependencies { get; } = [];

    /// <summary>
    /// Gets the baselines of the selected project.
    /// </summary>
    internal ObservableCollection<ProjectChoice<Guid>> Baselines { get; } = [];

    /// <summary>
    /// Gets the baseline comparison rows.
    /// </summary>
    internal ObservableCollection<ProjectVarianceRow> VarianceRows { get; } = [];

    /// <summary>
    /// Loads the portfolio: replays the offline queue and refreshes from the web-api when it is reachable.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public Task LoadAsync() => ReloadAsync(refresh: true);

    /// <summary>
    /// Replays the offline queue and pulls the web-api again.
    /// </summary>
    /// <returns>A task representing the sync.</returns>
    [RelayCommand]
    public Task SyncAsync() => ReloadAsync(refresh: true);

    /// <summary>
    /// Clears the project editor to create a new project.
    /// </summary>
    [RelayCommand]
    public void NewProject() => ResetProjectEditor();

    /// <summary>
    /// Saves the project of the editor.
    /// </summary>
    /// <returns>A task representing the save.</returns>
    [RelayCommand]
    public async Task SaveProjectAsync()
    {
        var existing = EditingProjectId is { } id && _plan?.Project.Id == id ? _plan.Project : null;
        var project = new Project
        {
            Id = EditingProjectId ?? Guid.NewGuid(),
            LegalEntityId = existing?.LegalEntityId,
            ProjectCode = EditCode,
            Name = EditName,
            CustomerId = existing?.CustomerId,
            ProjectManagerId = existing?.ProjectManagerId,
            CostCenterId = existing?.CostCenterId,
            ProjectType = SelectedProjectType?.Value ?? ProjectType.ClientBillable,
            BillingModel = SelectedBillingModel?.Value ?? ProjectBillingModel.TimeAndMaterials,
            Status = SelectedStatus?.Value ?? ProjectStatus.Planning,
            StartDate = ToDate(EditStart),
            TargetEndDate = ToDate(EditEnd),
            CupCode = EditCup,
            CigCode = EditCig,
            IsTemplate = EditIsTemplate,
            TemplateSourceId = existing?.TemplateSourceId,
            CreatedAt = existing?.CreatedAt,
        };

        var result = await RunAsync(() => _sender.Send(new SaveProjectCommand(project, Guid.NewGuid())));
        if (result is { Status: not ProjectActionStatus.Rejected })
        {
            await ReloadAsync(refresh: false, select: project.Id);
        }
    }

    /// <summary>
    /// Adds a top-level node from the node editor.
    /// </summary>
    /// <returns>A task representing the save.</returns>
    [RelayCommand]
    public Task AddRootNodeAsync() => AddNodeAsync(parentId: null);

    /// <summary>
    /// Adds a child of the selected node from the node editor.
    /// </summary>
    /// <returns>A task representing the save.</returns>
    [RelayCommand]
    public Task AddChildNodeAsync() => AddNodeAsync(SelectedNode?.Id);

    /// <summary>
    /// Writes the node editor onto the selected node.
    /// </summary>
    /// <returns>A task representing the save.</returns>
    [RelayCommand]
    public async Task UpdateNodeAsync()
    {
        var current = _plan?.Nodes.FirstOrDefault(node => node.Id == SelectedNode?.Id);
        if (current is null)
        {
            StatusMessage = _localization["Project_Err_Select"];

            return;
        }

        var node = Clone(current);
        Fill(node);
        await SaveNodeAsync(node);
    }

    /// <summary>
    /// Deletes the selected node with its descendants.
    /// </summary>
    /// <returns>A task representing the delete.</returns>
    [RelayCommand]
    public async Task DeleteNodeAsync()
    {
        if (_plan is null || SelectedNode is not { } node)
        {
            StatusMessage = _localization["Project_Err_Select"];

            return;
        }

        var projectId = _plan.Project.Id;
        var result = await RunAsync(() => _sender.Send(new DeleteWbsNodeCommand(projectId, node.Id, Guid.NewGuid())));
        if (result is { Status: not ProjectActionStatus.Rejected })
        {
            await ReloadAsync(refresh: false, select: projectId);
        }
    }

    /// <summary>
    /// Adds the dependency chosen in the dependency editor.
    /// </summary>
    /// <returns>A task representing the save.</returns>
    [RelayCommand]
    public async Task AddDependencyAsync()
    {
        if (_plan is null || DependencyPredecessor is null || DependencySuccessor is null)
        {
            StatusMessage = _localization["Project_Err_Select"];

            return;
        }

        var dependency = new TaskDependency
        {
            Id = Guid.NewGuid(),
            ProjectId = _plan.Project.Id,
            PredecessorNodeId = DependencyPredecessor.Id,
            SuccessorNodeId = DependencySuccessor.Id,
            DependencyType = SelectedDependencyType?.Value ?? DependencyType.FS,
            LagDays = DependencyLag,
        };
        var projectId = _plan.Project.Id;
        var result = await RunAsync(() => _sender.Send(new SaveTaskDependencyCommand(dependency, Guid.NewGuid())));
        if (result is { Status: not ProjectActionStatus.Rejected })
        {
            await ReloadAsync(refresh: false, select: projectId);
        }
    }

    /// <summary>
    /// Removes a dependency.
    /// </summary>
    /// <param name="row">The dependency to remove.</param>
    /// <returns>A task representing the delete.</returns>
    [RelayCommand]
    public async Task RemoveDependencyAsync(ProjectDependencyRow? row)
    {
        if (_plan is null || row is null)
        {
            return;
        }

        var projectId = _plan.Project.Id;
        var result = await RunAsync(() => _sender.Send(new DeleteTaskDependencyCommand(projectId, row.Id, Guid.NewGuid())));
        if (result is { Status: not ProjectActionStatus.Rejected })
        {
            await ReloadAsync(refresh: false, select: projectId);
        }
    }

    /// <summary>
    /// Freezes the current plan as a new baseline.
    /// </summary>
    /// <returns>A task representing the save.</returns>
    [RelayCommand]
    public async Task CaptureBaselineAsync()
    {
        if (_plan is null)
        {
            StatusMessage = _localization["Project_Err_Select"];

            return;
        }

        var projectId = _plan.Project.Id;
        var name = BaselineName;
        var result = await RunAsync(() => _sender.Send(new CaptureProjectBaselineCommand(projectId, name, Guid.NewGuid())));
        if (result is { Status: not ProjectActionStatus.Rejected })
        {
            BaselineName = string.Empty;
            await ReloadAsync(refresh: false, select: projectId);
            SelectedBaseline = Baselines.FirstOrDefault(choice => choice.Value == result.EntityId);
        }
    }

    /// <summary>
    /// Creates a new project from the selected project (a template or any project) with the data of the template editor.
    /// </summary>
    /// <returns>A task representing the save.</returns>
    [RelayCommand]
    public async Task InstantiateTemplateAsync()
    {
        if (SelectedProject is not { } source || TemplateStart is not { } start)
        {
            StatusMessage = _localization["Project_Err_Select"];

            return;
        }

        var request = new TemplateInstantiationRequest(source.Id, TemplateCode, TemplateName, DateOnly.FromDateTime(start));
        var result = await RunAsync(() => _sender.Send(new InstantiateProjectTemplateCommand(request, Guid.NewGuid())));
        if (result is { Status: not ProjectActionStatus.Rejected, EntityId: { } created })
        {
            TemplateCode = string.Empty;
            TemplateName = string.Empty;
            await ReloadAsync(refresh: false, select: created);
        }
    }

    /// <summary>
    /// Discards the changes the web-api refused.
    /// </summary>
    /// <returns>A task representing the save.</returns>
    [RelayCommand]
    public async Task DismissConflictsAsync()
    {
        var portfolio = await _sender.Send(new GetProjectPortfolioQuery(Refresh: false));
        foreach (var conflict in portfolio.Conflicts)
        {
            await _sender.Send(new DismissProjectConflictCommand(conflict.EntryId));
        }

        await ReloadAsync(refresh: false, select: SelectedProject?.Id);
    }

    partial void OnSelectedProjectChanged(ProjectPortfolioRow? value)
    {
        if (_suppressSelection || value is null)
        {
            return;
        }

        _ = ShowPlanAsync(value.Id);
    }

    partial void OnSelectedNodeChanged(WbsRow? value)
    {
        var node = value is null ? null : _plan?.Nodes.FirstOrDefault(candidate => candidate.Id == value.Id);
        if (node is null)
        {
            ResetNodeEditor();

            return;
        }

        NodeTitle = node.Title;
        SelectedNodeType = NodeTypes.FirstOrDefault(choice => choice.Value == node.NodeType);
        NodeStart = node.StartDate?.ToDateTime(TimeOnly.MinValue);
        NodeEnd = node.EndDate?.ToDateTime(TimeOnly.MinValue);
        NodeHours = node.PlannedHours;
        NodeCost = node.PlannedCost;
        NodeRevenue = node.PlannedRevenue;
        NodePercent = node.PercentComplete;
    }

    partial void OnSelectedBaselineChanged(ProjectChoice<Guid>? value) => _ = ShowVarianceAsync(value);

    private static DateOnly? ToDate(DateTime? value) => value is { } date ? DateOnly.FromDateTime(date) : null;

    private static WbsNode Clone(WbsNode node) => new()
    {
        Id = node.Id,
        TenantId = node.TenantId,
        ProjectId = node.ProjectId,
        ParentNodeId = node.ParentNodeId,
        WbsCode = node.WbsCode,
        Title = node.Title,
        NodeType = node.NodeType,
        PlannedHours = node.PlannedHours,
        ActualHours = node.ActualHours,
        PlannedCost = node.PlannedCost,
        ActualCost = node.ActualCost,
        PlannedRevenue = node.PlannedRevenue,
        StartDate = node.StartDate,
        EndDate = node.EndDate,
        PercentComplete = node.PercentComplete,
        SortOrder = node.SortOrder,
        CreatedAt = node.CreatedAt,
    };

    private static string Variant(VarianceStatus status) => status.ToString();

    private async Task ReloadAsync(bool refresh, Guid? select = null)
    {
        IsBusy = true;

        try
        {
            var portfolio = await _sender.Send(new GetProjectPortfolioQuery(refresh));
            var keep = select ?? SelectedProject?.Id;

            _suppressSelection = true;
            try
            {
                Projects.Clear();
                foreach (var project in portfolio.Projects)
                {
                    Projects.Add(RowOf(project));
                }

                SelectedProject = Projects.FirstOrDefault(row => row.Id == keep) ?? Projects.FirstOrDefault();
            }
            finally
            {
                _suppressSelection = false;
            }

            IsOffline = portfolio.FromCache;
            ShowTotals(portfolio.Projects);
            ShowQueue(portfolio);
            if (refresh)
            {
                StatusMessage = _localization[portfolio.FromCache ? "Project_OfflineUsingCache" : "Project_Status_Synced"];
            }

            if (SelectedProject is { } selected)
            {
                await ShowPlanAsync(selected.Id);
            }
            else
            {
                Apply(plan: null);
                StatusMessage = _localization["Project_NoProjects"];
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ShowPlanAsync(Guid projectId)
    {
        var plan = await _sender.Send(new GetProjectPlanQuery(projectId));
        Apply(plan);
    }

    private async Task ShowVarianceAsync(ProjectChoice<Guid>? baseline)
    {
        VarianceRows.Clear();
        VarianceSummary = string.Empty;
        if (baseline is null || _plan is null)
        {
            return;
        }

        var variance = await _sender.Send(new GetBaselineVarianceQuery(_plan.Project.Id, baseline.Value));
        if (variance is null)
        {
            return;
        }

        var culture = _localization.CurrentCulture;
        foreach (var node in variance.Nodes)
        {
            VarianceRows.Add(new ProjectVarianceRow(
                node.WbsCode,
                node.Title,
                _localization["Project_Var_" + node.Status],
                Variant(node.Status),
                Days(node.StartVarianceDays),
                Days(node.FinishVarianceDays),
                Signed(node.HoursVariance, culture),
                Signed(node.CostVariance, culture),
                Signed(node.RevenueVariance, culture)));
        }

        VarianceSummary = Format(
            "Project_Var_Summary",
            Days(variance.StartVarianceDays),
            Days(variance.FinishVarianceDays),
            Signed(variance.HoursVariance, culture),
            Signed(variance.CostVariance, culture),
            Signed(variance.RevenueVariance, culture));
    }

    private async Task AddNodeAsync(Guid? parentId)
    {
        if (_plan is null)
        {
            StatusMessage = _localization["Project_Err_Select"];

            return;
        }

        var node = new WbsNode { Id = Guid.NewGuid(), ProjectId = _plan.Project.Id, ParentNodeId = parentId };
        Fill(node);
        await SaveNodeAsync(node);
    }

    private async Task SaveNodeAsync(WbsNode node)
    {
        var projectId = node.ProjectId;
        var result = await RunAsync(() => _sender.Send(new SaveWbsNodeCommand(node, Guid.NewGuid())));
        if (result is { Status: not ProjectActionStatus.Rejected })
        {
            await ReloadAsync(refresh: false, select: projectId);
        }
    }

    private void Fill(WbsNode node)
    {
        node.Title = NodeTitle;
        node.NodeType = SelectedNodeType?.Value ?? WbsNodeType.WorkPackage;
        node.StartDate = ToDate(NodeStart);
        node.EndDate = ToDate(NodeEnd);
        node.PlannedHours = NodeHours;
        node.PlannedCost = NodeCost;
        node.PlannedRevenue = NodeRevenue;
        node.PercentComplete = NodePercent;
    }

    private async Task<ProjectActionResult?> RunAsync(Func<Task<ProjectActionResult>> action)
    {
        IsBusy = true;

        try
        {
            var result = await action();
            StatusMessage = result.Status switch
            {
                ProjectActionStatus.Sent => _localization["Project_Saved"],
                ProjectActionStatus.Queued => _localization["Project_Queued"],
                ProjectActionStatus.Conflict => Format("Project_Conflict", result.Detail ?? string.Empty),
                _ => _localization[result.Detail ?? "Project_Err_Generic"],
            };

            return result;
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = ex.Message;

            return null;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ResetProjectEditor()
    {
        EditingProjectId = null;
        EditCode = string.Empty;
        EditName = string.Empty;
        EditCup = string.Empty;
        EditCig = string.Empty;
        EditStart = _time.GetLocalNow().Date;
        EditEnd = null;
        EditIsTemplate = false;
        SelectedProjectType = ProjectTypes[0];
        SelectedBillingModel = BillingModels[0];
        SelectedStatus = Statuses[0];
    }

    private void ResetNodeEditor()
    {
        NodeTitle = string.Empty;
        SelectedNodeType = NodeTypes.FirstOrDefault(choice => choice.Value == WbsNodeType.WorkPackage);
        NodeStart = _time.GetLocalNow().Date;
        NodeEnd = null;
        NodeHours = 0m;
        NodeCost = 0m;
        NodeRevenue = 0m;
        NodePercent = 0m;
    }

    private void ShowTotals(IReadOnlyList<Project> projects)
    {
        var culture = _localization.CurrentCulture;
        var live = projects.Where(project => !project.IsTemplate).ToList();
        ProjectCountText = live.Count.ToString(culture);
        TotalHoursText = live.Sum(project => project.PlannedHours).ToString(NumberFormat, culture);
        TotalCostText = live.Sum(project => project.PlannedCost).ToString(NumberFormat, culture);
        TotalRevenueText = live.Sum(project => project.PlannedRevenue).ToString(NumberFormat, culture);
        AverageProgressText = (live.Count == 0 ? 0m : live.Average(project => project.PercentComplete)).ToString("N1", culture) + "%";
    }

    private void ShowQueue(ProjectPortfolio portfolio)
    {
        HasConflicts = portfolio.Conflicts.Count > 0;
        PendingText = portfolio.Pending.Count + portfolio.Conflicts.Count == 0
            ? string.Empty
            : Format("Project_PendingCount", portfolio.Pending.Count.ToString(_localization.CurrentCulture), portfolio.Conflicts.Count.ToString(_localization.CurrentCulture));
    }

    private ProjectPortfolioRow RowOf(Project project)
    {
        var culture = _localization.CurrentCulture;

        return new ProjectPortfolioRow(
            project.Id,
            project.ProjectCode,
            project.Name,
            _localization["Project_Status_" + project.Status],
            _localization["Project_Type_" + project.ProjectType],
            DateText(project.StartDate),
            DateText(project.TargetEndDate),
            (double)Math.Clamp(project.PercentComplete, 0m, 100m),
            project.PercentComplete.ToString("N1", culture) + "%",
            project.PlannedHours.ToString(NumberFormat, culture),
            project.PlannedCost.ToString(NumberFormat, culture),
            project.PlannedRevenue.ToString(NumberFormat, culture),
            project.IsTemplate);
    }

    private void Apply(ProjectPlan? plan)
    {
        _plan = plan;
        HasPlan = plan is not null;
        WbsRows.Clear();
        GanttBars.Clear();
        KanbanColumns.Clear();
        Dependencies.Clear();
        Baselines.Clear();
        VarianceRows.Clear();
        VarianceSummary = string.Empty;
        CriticalPathText = string.Empty;
        ProjectFinishText = string.Empty;
        SelectedBaseline = null;
        SelectedNode = null;
        DependencyPredecessor = null;
        DependencySuccessor = null;
        if (plan is null)
        {
            return;
        }

        FillEditor(plan.Project);

        var culture = _localization.CurrentCulture;
        var schedule = plan.Schedule.Nodes.ToDictionary(node => node.NodeId);
        var tree = WbsTreeOrder.Flatten(plan.Nodes);
        foreach (var item in tree)
        {
            var node = item.Node;
            var scheduled = schedule.GetValueOrDefault(node.Id);
            WbsRows.Add(new WbsRow(
                node.Id,
                node.WbsCode,
                node.Title,
                new System.Windows.Thickness(item.Depth * IndentStep, 0, 0, 0),
                _localization["Project_NodeType_" + node.NodeType],
                DateText(node.StartDate),
                DateText(node.EndDate),
                node.PlannedHours.ToString(NumberFormat, culture),
                node.PlannedCost.ToString(NumberFormat, culture),
                node.PercentComplete.ToString("N0", culture) + "%",
                scheduled is null ? NoValue : scheduled.TotalFloatDays.ToString(culture),
                scheduled?.IsCritical ?? false,
                item.HasChildren));
        }

        ShowGantt(tree, schedule, culture);
        ShowKanban(tree, schedule);

        foreach (var link in plan.Dependencies)
        {
            Dependencies.Add(new ProjectDependencyRow(link.Id, DependencyText(link, plan.Nodes)));
        }

        foreach (var baseline in plan.Baselines)
        {
            Baselines.Add(new ProjectChoice<Guid>(baseline.Id, "#" + baseline.BaselineNumber.ToString(culture) + " " + baseline.Name + " (" + baseline.CapturedAt.ToLocalTime().ToString("g", culture) + ")"));
        }

        if (plan.Schedule.HasCycle)
        {
            CriticalPathText = _localization["Project_Err_Cycle"];
        }
        else
        {
            var byId = plan.Nodes.ToDictionary(node => node.Id);
            CriticalPathText = plan.Schedule.CriticalPath.Count == 0
                ? string.Empty
                : Format("Project_CriticalPath", string.Join(" > ", plan.Schedule.CriticalPath.Select(id => byId[id].WbsCode)));
            ProjectFinishText = plan.Schedule.ProjectFinish is { } finish ? Format("Project_Finish", DateText(finish)) : string.Empty;
        }
    }

    private void FillEditor(Project project)
    {
        EditingProjectId = project.Id;
        EditCode = project.ProjectCode;
        EditName = project.Name;
        EditCup = project.CupCode ?? string.Empty;
        EditCig = project.CigCode ?? string.Empty;
        EditStart = project.StartDate?.ToDateTime(TimeOnly.MinValue);
        EditEnd = project.TargetEndDate?.ToDateTime(TimeOnly.MinValue);
        EditIsTemplate = project.IsTemplate;
        SelectedProjectType = ProjectTypes.FirstOrDefault(choice => choice.Value == project.ProjectType);
        SelectedBillingModel = BillingModels.FirstOrDefault(choice => choice.Value == project.BillingModel);
        SelectedStatus = Statuses.FirstOrDefault(choice => choice.Value == project.Status);
    }

    private void ShowGantt(IReadOnlyList<WbsTreeItem> tree, Dictionary<Guid, NodeSchedule> schedule, CultureInfo culture)
    {
        var bars = new List<(WbsTreeItem Item, DateOnly Start, DateOnly End, NodeSchedule? Schedule)>();
        foreach (var item in tree)
        {
            var scheduled = schedule.GetValueOrDefault(item.Node.Id);
            var start = item.Node.StartDate ?? scheduled?.EarlyStart;
            var end = item.Node.EndDate ?? scheduled?.EarlyFinish ?? start;
            if (start is { } first && end is { } last)
            {
                bars.Add((item, first, last < first ? first : last, scheduled));
            }
        }

        if (bars.Count == 0)
        {
            GanttWidth = DayWidth;

            return;
        }

        var origin = bars.Min(bar => bar.Start);
        var finish = bars.Max(bar => bar.End);
        GanttWidth = ((finish.DayNumber - origin.DayNumber) + 2) * DayWidth;

        foreach (var (item, start, end, scheduled) in bars)
        {
            var node = item.Node;
            var milestone = node.NodeType == WbsNodeType.Milestone;
            var width = milestone ? MilestoneWidth : ((end.DayNumber - start.DayNumber) + 1) * DayWidth;
            var tooltip = Format(
                "Project_Gantt_Tooltip",
                node.WbsCode + " " + node.Title,
                DateText(start),
                DateText(end),
                scheduled is null ? NoValue : scheduled.TotalFloatDays.ToString(culture));

            GanttBars.Add(new GanttBarRow(
                node.WbsCode + " " + node.Title,
                new System.Windows.Thickness((start.DayNumber - origin.DayNumber) * DayWidth, 0, 0, 0),
                width,
                width * (double)Math.Clamp(node.PercentComplete, 0m, 100m) / PercentScale,
                scheduled?.IsCritical ?? false,
                milestone,
                item.HasChildren,
                tooltip));
        }
    }

    private void ShowKanban(IReadOnlyList<WbsTreeItem> tree, Dictionary<Guid, NodeSchedule> schedule)
    {
        var culture = _localization.CurrentCulture;
        var cards = tree
            .Where(item => !item.HasChildren)
            .Select(item => item.Node)
            .ToList();

        KanbanColumnRow Column(string key, Func<WbsNode, bool> belongs)
        {
            var members = cards
                .Where(belongs)
                .Select(node => new KanbanCardRow(
                    node.WbsCode,
                    node.Title,
                    node.PercentComplete.ToString("N0", culture) + "%",
                    DateText(node.StartDate) + " - " + DateText(node.EndDate),
                    schedule.GetValueOrDefault(node.Id)?.IsCritical ?? false))
                .ToList();

            return new KanbanColumnRow(_localization[key] + " (" + members.Count.ToString(culture) + ")", members);
        }

        KanbanColumns.Add(Column("Project_Kanban_Todo", node => node.PercentComplete <= 0m));
        KanbanColumns.Add(Column("Project_Kanban_Doing", node => node.PercentComplete is > 0m and < 100m));
        KanbanColumns.Add(Column("Project_Kanban_Done", node => node.PercentComplete >= 100m));
    }

    private string DependencyText(TaskDependency link, IReadOnlyList<WbsNode> nodes)
    {
        var source = nodes.FirstOrDefault(node => node.Id == link.PredecessorNodeId)?.WbsCode ?? NoValue;
        var target = nodes.FirstOrDefault(node => node.Id == link.SuccessorNodeId)?.WbsCode ?? NoValue;
        var lag = link.LagDays == 0 ? string.Empty : " " + link.LagDays.ToString("+0;-0", _localization.CurrentCulture);

        return source + " > " + target + " (" + link.DependencyType + lag + ")";
    }

    private string DateText(DateOnly? date) => date is { } value ? value.ToString("d", _localization.CurrentCulture) : NoValue;

    private string Days(int? days) => days is { } value ? value.ToString("+0;-0;0", _localization.CurrentCulture) : NoValue;

    private string Signed(decimal value, CultureInfo culture) => value.ToString(SignedFormat, culture);

    private string Format(string key, params string[] arguments) => string.Format(_localization.CurrentCulture, _localization[key], arguments);
}
