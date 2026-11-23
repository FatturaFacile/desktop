// <copyright file="AllocationsViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.ResourcePlanning;
using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Drives the allocations list: allocations are created and deleted (after a confirmation), soft bookings are converted into hard
/// commitments and any allocation is rebalanced by shifting, reassigning, splitting or resizing it, or by dragging it to a week of
/// an employee on the board; the server answers with the overload it causes.
/// </summary>
internal sealed partial class AllocationsViewModel : ObservableObject
{
    private const int PastDays = 14;
    private const int FutureDays = 84;

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly ResourcePlanningFormatter _format;

    [ObservableProperty]
    private DateTime _fromDate;

    [ObservableProperty]
    private DateTime _toDate;

    [ObservableProperty]
    private PlanningChoice? _selectedBookingFilter;

    [ObservableProperty]
    private AllocationRow? _selectedRow;

    [ObservableProperty]
    private PlanningChoice? _selectedStrategy;

    [ObservableProperty]
    private PlanningChoice? _selectedEmployee;

    [ObservableProperty]
    private string _shiftDaysText = string.Empty;

    [ObservableProperty]
    private DateTime? _newStartDate;

    [ObservableProperty]
    private DateTime? _newEndDate;

    [ObservableProperty]
    private DateTime? _splitDate;

    [ObservableProperty]
    private string _hoursPerDayText = string.Empty;

    [ObservableProperty]
    private string _convertProjectIdText = string.Empty;

    [ObservableProperty]
    private PlanningChoice? _selectedConvertProject;

    [ObservableProperty]
    private bool _isDeletePending;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasData;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _overloadMessage = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllocationsViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="time">The clock.</param>
    /// <param name="form">The form that creates an allocation.</param>
    /// <param name="board">The board allocations are dragged onto.</param>
    public AllocationsViewModel(ISender sender, ILocalizationService localization, TimeProvider time, AllocationFormViewModel form, AllocationBoardViewModel board)
    {
        ArgumentNullException.ThrowIfNull(time);

        _sender = sender;
        _localization = localization;
        Form = form;
        Board = board;
        _format = new ResourcePlanningFormatter(localization);
        var today = time.GetLocalNow().Date;
        _fromDate = today.AddDays(-PastDays);
        _toDate = today.AddDays(FutureDays);
        BookingFilters =
        [
            new PlanningChoice(string.Empty, _localization["ResourcePlanning_Booking_All"]),
            new PlanningChoice(nameof(ResourceBookingType.SoftTentative), _localization["ResourcePlanning_Booking_SoftTentative"]),
            new PlanningChoice(nameof(ResourceBookingType.HardCommitted), _localization["ResourcePlanning_Booking_HardCommitted"]),
        ];
        StrategyChoices =
        [
            new PlanningChoice(nameof(RebalanceStrategy.Shift), _localization["ResourcePlanning_Strategy_Shift"]),
            new PlanningChoice(nameof(RebalanceStrategy.Reassign), _localization["ResourcePlanning_Strategy_Reassign"]),
            new PlanningChoice(nameof(RebalanceStrategy.Split), _localization["ResourcePlanning_Strategy_Split"]),
            new PlanningChoice(nameof(RebalanceStrategy.Resize), _localization["ResourcePlanning_Strategy_Resize"]),
        ];
        _selectedBookingFilter = BookingFilters[0];
        _selectedStrategy = StrategyChoices[0];
    }

    /// <summary>
    /// Gets the form that creates an allocation.
    /// </summary>
    public AllocationFormViewModel Form { get; }

    /// <summary>
    /// Gets the board allocations are dragged onto.
    /// </summary>
    public AllocationBoardViewModel Board { get; }

    /// <summary>
    /// Gets the booking type filters, the first entry being all.
    /// </summary>
    public IReadOnlyList<PlanningChoice> BookingFilters { get; }

    /// <summary>
    /// Gets the rebalance strategies.
    /// </summary>
    public IReadOnlyList<PlanningChoice> StrategyChoices { get; }

    /// <summary>
    /// Gets a value indicating whether the selected allocation is a soft booking that can be converted into a hard commitment.
    /// </summary>
    public bool CanConvert => SelectedRow?.IsSoft == true;

    /// <summary>
    /// Gets a value indicating whether an allocation is selected.
    /// </summary>
    public bool HasSelection => SelectedRow is not null;

    /// <summary>
    /// Gets a value indicating whether the selected allocation needs a project typed in before it can be converted (it only has an opportunity).
    /// </summary>
    public bool NeedsProjectForConversion => CanConvert && SelectedRow?.Allocation.ProjectId is null;

    /// <summary>
    /// Gets the allocations of the window.
    /// </summary>
    public ObservableCollection<AllocationRow> Rows { get; } = [];

    /// <summary>
    /// Gets the employees an allocation can be given to.
    /// </summary>
    public ObservableCollection<PlanningChoice> EmployeeChoices { get; } = [];

    /// <summary>
    /// Loads the allocations of the window and the employees for the reassignment.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            await LoadEmployeesAsync();
            await Form.LoadChoicesAsync();
            await ReloadAsync();
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = ex.Message;
        }
        catch (HttpRequestException)
        {
            StatusMessage = _localization["ResourcePlanning_LoadFailed"];
        }
        catch (JsonException)
        {
            StatusMessage = _localization["ResourcePlanning_LoadFailed"];
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Converts the selected soft booking into a hard commitment.
    /// </summary>
    /// <returns>A task representing the conversion.</returns>
    [RelayCommand]
    public async Task ConvertToHardAsync()
    {
        if (SelectedRow is not { IsSoft: true } row)
        {
            StatusMessage = _localization["ResourcePlanning_Convert_NeedsSoft"];

            return;
        }

        Guid? project = null;
        if (row.Allocation.ProjectId is null)
        {
            if (!PlanningIds.TryResolve(SelectedConvertProject?.Key, ConvertProjectIdText, out var resolved))
            {
                StatusMessage = _localization["ResourcePlanning_Convert_InvalidProject"];

                return;
            }

            if (resolved is null)
            {
                StatusMessage = _localization["ResourcePlanning_Convert_NeedsProject"];

                return;
            }

            project = resolved;
        }

        _ = await RunAsync(() => _sender.Send(new ConvertAllocationToHardCommand(row.Allocation.Id, new ConvertToHardRequest(project, WbsNodeId: null))), "ResourcePlanning_Converted");
    }

    /// <summary>
    /// Rebalances the selected allocation with the strategy and the parameters of the form.
    /// </summary>
    /// <returns>A task representing the rebalance.</returns>
    [RelayCommand]
    public async Task RebalanceAsync()
    {
        if (SelectedRow is null)
        {
            StatusMessage = _localization["ResourcePlanning_Rebalance_NeedsSelection"];

            return;
        }

        var allocation = SelectedRow.Allocation;
        var built = RebalanceRequestBuilder.Build(allocation, Draft());
        if (built.Request is not { } request)
        {
            StatusMessage = _format.Describe("Rebalance", built.ErrorCode ?? string.Empty);

            return;
        }

        _ = await RunAsync(() => _sender.Send(new RebalanceAllocationCommand(allocation.Id, request)), "ResourcePlanning_Rebalanced");
    }

    /// <summary>
    /// Creates an allocation from the form; the form is checked first and the overload the booking causes is shown.
    /// </summary>
    /// <returns>A task representing the creation.</returns>
    [RelayCommand]
    public async Task CreateAsync()
    {
        var built = Form.Build();
        if (built.Request is not { } request)
        {
            OverloadMessage = string.Empty;
            StatusMessage = _format.Describe("Create", built.ErrorCode ?? string.Empty);

            return;
        }

        var created = await RunAsync(() => _sender.Send(new CreateResourceAllocationCommand(request)), "ResourcePlanning_Created");
        if (created)
        {
            Form.Reset();
        }
    }

    /// <summary>
    /// Asks the user to confirm the deletion of the selected allocation; nothing is deleted yet.
    /// </summary>
    [RelayCommand]
    public void RequestDelete()
    {
        if (SelectedRow is null)
        {
            StatusMessage = _localization["ResourcePlanning_Delete_NeedsSelection"];

            return;
        }

        IsDeletePending = true;
        StatusMessage = string.Format(_format.Culture, _localization["ResourcePlanning_Delete_Confirm"], SelectedRow.Allocation.EmployeeName, SelectedRow.PeriodText);
    }

    /// <summary>
    /// Withdraws the request to delete the selected allocation.
    /// </summary>
    [RelayCommand]
    public void CancelDelete()
    {
        IsDeletePending = false;
        StatusMessage = string.Empty;
    }

    /// <summary>
    /// Deletes the selected allocation once the user confirmed it.
    /// </summary>
    /// <returns>A task representing the deletion.</returns>
    [RelayCommand]
    public async Task ConfirmDeleteAsync()
    {
        if (!IsDeletePending || SelectedRow is not { } row)
        {
            return;
        }

        IsDeletePending = false;
        _ = await RunAsync(() => _sender.Send(new DeleteResourceAllocationCommand(row.Allocation.Id)), "ResourcePlanning_Deleted");
    }

    /// <summary>
    /// Moves an allocation that was dropped on a week of an employee of the board: a different week shifts it by whole weeks (the
    /// weekday stays), a different employee gives it to that colleague, and both can happen at once.
    /// </summary>
    /// <param name="request">The dragged allocation and the cell it was dropped on.</param>
    /// <returns>A task representing the move.</returns>
    [RelayCommand]
    public async Task DropAsync(DropRequest? request)
    {
        if (request is not { Payload: AllocationRow row, Target: BoardCell cell })
        {
            return;
        }

        var allocation = row.Allocation;
        var days = cell.WeekStart.DayNumber - CapacityHeatmapViewModel.MondayOf(allocation.StartDate).DayNumber;
        var employee = cell.EmployeeId == allocation.EmployeeId ? (Guid?)null : cell.EmployeeId;
        if (days == 0 && employee is null)
        {
            StatusMessage = _localization["ResourcePlanning_Drop_NoChange"];

            return;
        }

        var draft = days == 0
            ? new RebalanceDraft(RebalanceStrategy.Reassign, employee, StartDate: null, EndDate: null, SplitDate: null, ShiftDays: null, AllocatedHoursPerDay: null)
            : new RebalanceDraft(RebalanceStrategy.Shift, employee, StartDate: null, EndDate: null, SplitDate: null, days, AllocatedHoursPerDay: null);
        var built = RebalanceRequestBuilder.Build(allocation, draft);
        if (built.Request is not { } rebalance)
        {
            StatusMessage = _format.Describe("Rebalance", built.ErrorCode ?? string.Empty);

            return;
        }

        _ = await RunAsync(() => _sender.Send(new RebalanceAllocationCommand(allocation.Id, rebalance)), "ResourcePlanning_Moved");
    }

    private static DateOnly? Day(DateTime? value) => value is { } date ? DateOnly.FromDateTime(date) : null;

    private static string ReferenceOf(ResourceAllocation allocation) =>
        allocation.ProjectName is { Length: > 0 } project
            ? WithNode(project, allocation.WbsNodeTitle)
            : ResourcePlanningFormatter.OrDash(allocation.CrmOpportunityName);

    private static string WithNode(string project, string? node) => node is { Length: > 0 } ? project + " / " + node : project;

    partial void OnSelectedRowChanged(AllocationRow? value)
    {
        OnPropertyChanged(nameof(CanConvert));
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(NeedsProjectForConversion));
        ShiftDaysText = string.Empty;
        SplitDate = null;
        SelectedEmployee = null;
        ConvertProjectIdText = string.Empty;
        SelectedConvertProject = null;
        IsDeletePending = false;
        NewStartDate = value?.Allocation.StartDate.ToDateTime(TimeOnly.MinValue);
        NewEndDate = value?.Allocation.EndDate.ToDateTime(TimeOnly.MinValue);
        HoursPerDayText = value?.Allocation.AllocatedHoursPerDay.ToString("0.##", _format.Culture) ?? string.Empty;
    }

    private RebalanceDraft Draft()
    {
        var strategy = Enum.TryParse<RebalanceStrategy>(SelectedStrategy?.Key, out var parsed) ? parsed : RebalanceStrategy.Shift;
        var employee = Guid.TryParse(SelectedEmployee?.Key, out var employeeId) ? employeeId : (Guid?)null;

        return new RebalanceDraft(strategy, employee, Day(NewStartDate), Day(NewEndDate), Day(SplitDate), _format.ParseInt(ShiftDaysText), _format.ParseDecimal(HoursPerDayText));
    }

    private async Task<bool> RunAsync<T>(Func<Task<PlanningCommandOutcome<T>>> action, string successKey)
    {
        IsBusy = true;

        try
        {
            var outcome = await action();
            if (outcome.IsSuccess)
            {
                StatusMessage = _localization[successKey];
                OverloadMessage = outcome.Value is AllocationMutationResult result ? _format.Warnings(result) : string.Empty;
                await ReloadAsync();

                return true;
            }

            OverloadMessage = string.Empty;
            StatusMessage = _format.Refusal(outcome.Status, outcome.Issues);
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = ex.Message;
        }
        catch (HttpRequestException)
        {
            StatusMessage = _localization["ResourcePlanning_SaveFailed"];
        }
        catch (JsonException)
        {
            StatusMessage = _localization["ResourcePlanning_SaveFailed"];
        }
        finally
        {
            IsBusy = false;
        }

        return false;
    }

    private async Task LoadEmployeesAsync()
    {
        if (EmployeeChoices.Count > 0)
        {
            return;
        }

        var resources = await _sender.Send(new GetPlanningResourcesQuery(DepartmentId: null));
        foreach (var resource in resources.OrderBy(resource => resource.FullName, StringComparer.CurrentCultureIgnoreCase))
        {
            EmployeeChoices.Add(new PlanningChoice(resource.EmployeeId.ToString("D", CultureInfo.InvariantCulture), resource.FullName));
        }

        Form.SetEmployees(EmployeeChoices);
    }

    private async Task ReloadAsync()
    {
        ResourceBookingType? filter = Enum.TryParse<ResourceBookingType>(SelectedBookingFilter?.Key, out var parsed) ? parsed : null;
        var allocations = await _sender.Send(new GetResourceAllocationsQuery(DateOnly.FromDateTime(FromDate), DateOnly.FromDateTime(ToDate), EmployeeId: null, ProjectId: null, filter));
        var selectedId = SelectedRow?.Allocation.Id;

        Rows.Clear();
        foreach (var allocation in allocations.OrderBy(item => item.StartDate).ThenBy(item => item.EmployeeName, StringComparer.CurrentCultureIgnoreCase))
        {
            Rows.Add(ToRow(allocation));
        }

        HasData = Rows.Count > 0;
        SelectedRow = selectedId is { } id ? Rows.FirstOrDefault(row => row.Allocation.Id == id) : null;
        await Board.LoadAsync(DateOnly.FromDateTime(FromDate));
        if (!HasData)
        {
            StatusMessage = _localization["ResourcePlanning_Allocations_Empty"];
        }
    }

    private AllocationRow ToRow(ResourceAllocation allocation) =>
        new(
            allocation,
            ReferenceOf(allocation),
            _format.Period(allocation.StartDate, allocation.EndDate),
            string.Format(_format.Culture, _localization["ResourcePlanning_Allocation_HoursFormat"], _format.Hours(allocation.AllocatedHoursPerDay), _format.Hours(allocation.TotalAllocatedHours)),
            _format.Booking(allocation.BookingType),
            allocation.BookingType == ResourceBookingType.SoftTentative,
            allocation.IsOverloaded,
            _format.Percent(allocation.PeakLoadPercent));
}
