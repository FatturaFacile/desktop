// <copyright file="AllocationFormViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.ProjectEvm;
using FatturaFacile.Application.Features.ResourcePlanning;
using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The form that creates an allocation: employee, booking type, project (picked from the projects of the profitability cache, or typed in
/// as an identifier when there is no list), WBS node, opportunity, period, hours per day, billable flag, notes, whether an overload is
/// accepted and the skills the booking needs. It checks the form before anything is sent.
/// </summary>
internal sealed partial class AllocationFormViewModel : ObservableObject
{
    /// <summary>
    /// The problem code when the typed project identifier is not an identifier.
    /// </summary>
    public const string InvalidProjectId = "invalid_project_id";

    /// <summary>
    /// The problem code when the typed WBS node identifier is not an identifier.
    /// </summary>
    public const string InvalidWbsId = "invalid_wbs_id";

    /// <summary>
    /// The problem code when the typed opportunity identifier is not an identifier.
    /// </summary>
    public const string InvalidOpportunityId = "invalid_opportunity_id";

    private const int PeriodDays = 14;
    private const string DefaultHoursPerDay = "8";

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly ResourcePlanningFormatter _format;
    private bool _choicesLoaded;

    [ObservableProperty]
    private PlanningChoice? _selectedEmployee;

    [ObservableProperty]
    private PlanningChoice? _selectedBookingType;

    [ObservableProperty]
    private PlanningChoice? _selectedProject;

    [ObservableProperty]
    private string _projectIdText = string.Empty;

    [ObservableProperty]
    private string _wbsNodeIdText = string.Empty;

    [ObservableProperty]
    private string _opportunityIdText = string.Empty;

    [ObservableProperty]
    private DateTime? _startDate;

    [ObservableProperty]
    private DateTime? _endDate;

    [ObservableProperty]
    private string _hoursPerDayText = DefaultHoursPerDay;

    [ObservableProperty]
    private bool _isBillable = true;

    [ObservableProperty]
    private bool _allowOverload = true;

    [ObservableProperty]
    private string _notes = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllocationFormViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="time">The clock.</param>
    public AllocationFormViewModel(ISender sender, ILocalizationService localization, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(time);

        _sender = sender;
        _localization = localization;
        _format = new ResourcePlanningFormatter(localization);
        var today = time.GetLocalNow().Date;
        _startDate = today;
        _endDate = today.AddDays(PeriodDays - 1);
        BookingChoices =
        [
            new PlanningChoice(nameof(ResourceBookingType.SoftTentative), _localization["ResourcePlanning_Booking_SoftTentative"]),
            new PlanningChoice(nameof(ResourceBookingType.HardCommitted), _localization["ResourcePlanning_Booking_HardCommitted"]),
        ];
        _selectedBookingType = BookingChoices[0];
        ProjectChoices = [new PlanningChoice(string.Empty, _localization["ResourcePlanning_Project_None"])];
        _selectedProject = ProjectChoices[0];
    }

    /// <summary>
    /// Gets the booking types.
    /// </summary>
    public IReadOnlyList<PlanningChoice> BookingChoices { get; }

    /// <summary>
    /// Gets the employees an allocation can be created for.
    /// </summary>
    public ObservableCollection<PlanningChoice> EmployeeChoices { get; } = [];

    /// <summary>
    /// Gets the projects to pick from, the first entry being none.
    /// </summary>
    public ObservableCollection<PlanningChoice> ProjectChoices { get; }

    /// <summary>
    /// Gets the skills of the taxonomy the booking can require.
    /// </summary>
    public ObservableCollection<SelectableChoice> SkillChoices { get; } = [];

    /// <summary>
    /// Gets a value indicating whether the projects can be picked from a list; without one the identifier is typed in.
    /// </summary>
    public bool HasProjectPicker => ProjectChoices.Count > 1;

    /// <summary>
    /// Gets a value indicating whether the booking is a soft booking, which can be tied to an opportunity.
    /// </summary>
    public bool IsSoft => !string.Equals(SelectedBookingType?.Key, nameof(ResourceBookingType.HardCommitted), StringComparison.Ordinal);

    /// <summary>
    /// Gets a value indicating whether the taxonomy has skills to require.
    /// </summary>
    public bool HasSkills => SkillChoices.Count > 0;

    /// <summary>
    /// Replaces the employees of the list.
    /// </summary>
    /// <param name="employees">The employees.</param>
    public void SetEmployees(IEnumerable<PlanningChoice> employees)
    {
        ArgumentNullException.ThrowIfNull(employees);

        EmployeeChoices.Clear();
        foreach (var employee in employees)
        {
            EmployeeChoices.Add(employee);
        }
    }

    /// <summary>
    /// Loads the projects and the skills once. The projects come from the cache of the profitability view; when the user may not read it,
    /// or it is empty, the project is typed in as an identifier.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    public async Task LoadChoicesAsync()
    {
        if (_choicesLoaded)
        {
            return;
        }

        var skills = await _sender.Send(new GetPlanningSkillsQuery(Category: null));
        SkillChoices.Clear();
        foreach (var skill in skills.OrderBy(item => item.Category, StringComparer.CurrentCultureIgnoreCase).ThenBy(item => item.SkillName, StringComparer.CurrentCultureIgnoreCase))
        {
            SkillChoices.Add(new SelectableChoice(skill.Id.ToString("D", CultureInfo.InvariantCulture), skill.Category + " / " + skill.SkillName));
        }

        OnPropertyChanged(nameof(HasSkills));
        await LoadProjectsAsync();
        _choicesLoaded = true;
    }

    /// <summary>
    /// Checks the form and builds the request that creates the allocation.
    /// </summary>
    /// <returns>The request or the code of the first problem.</returns>
    public PlanningDraftResult<AllocationRequest> Build()
    {
        if (!PlanningIds.TryResolve(SelectedProject?.Key, ProjectIdText, out var project))
        {
            return PlanningDraftResults.Fail<AllocationRequest>(InvalidProjectId);
        }

        if (!PlanningIds.TryResolve(pickedKey: null, WbsNodeIdText, out var wbs))
        {
            return PlanningDraftResults.Fail<AllocationRequest>(InvalidWbsId);
        }

        if (!PlanningIds.TryResolve(pickedKey: null, OpportunityIdText, out var opportunity))
        {
            return PlanningDraftResults.Fail<AllocationRequest>(InvalidOpportunityId);
        }

        var employee = Guid.TryParse(SelectedEmployee?.Key, out var employeeId) ? employeeId : (Guid?)null;
        var type = Enum.TryParse<ResourceBookingType>(SelectedBookingType?.Key, out var parsed) ? parsed : ResourceBookingType.SoftTentative;
        var skillIds = SkillChoices.Where(skill => skill.IsSelected).Select(skill => Guid.Parse(skill.Key)).ToList();

        return AllocationRequestBuilder.Build(new AllocationDraft(
            employee,
            type,
            project,
            wbs,
            opportunity,
            Day(StartDate),
            Day(EndDate),
            _format.ParseDecimal(HoursPerDayText),
            IsBillable,
            Notes,
            AllowOverload,
            skillIds));
    }

    /// <summary>
    /// Clears what describes the booking after it was created, keeping the employee and the period for the next one.
    /// </summary>
    public void Reset()
    {
        SelectedProject = ProjectChoices[0];
        ProjectIdText = string.Empty;
        WbsNodeIdText = string.Empty;
        OpportunityIdText = string.Empty;
        Notes = string.Empty;
        foreach (var skill in SkillChoices)
        {
            skill.IsSelected = false;
        }
    }

    private static DateOnly? Day(DateTime? value) => value is { } date ? DateOnly.FromDateTime(date) : null;

    partial void OnSelectedBookingTypeChanged(PlanningChoice? value) => OnPropertyChanged(nameof(IsSoft));

    private async Task LoadProjectsAsync()
    {
        try
        {
            var projects = await _sender.Send(new GetProjectEvmProjectsQuery());
            foreach (var project in projects.Where(item => Guid.TryParse(item.Id, out _)).OrderBy(item => item.Label, StringComparer.CurrentCultureIgnoreCase))
            {
                ProjectChoices.Add(new PlanningChoice(project.Id, project.Label));
            }
        }
        catch (UnauthorizedAccessException)
        {
            // The profitability cache is not readable by this user: the project is typed in as an identifier instead.
            ProjectChoices.Clear();
            ProjectChoices.Add(new PlanningChoice(string.Empty, _localization["ResourcePlanning_Project_None"]));
        }

        SelectedProject = ProjectChoices[0];
        OnPropertyChanged(nameof(HasProjectPicker));
    }
}
