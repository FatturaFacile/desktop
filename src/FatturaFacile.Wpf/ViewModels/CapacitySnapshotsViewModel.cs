// <copyright file="CapacitySnapshotsViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.ResourcePlanning;
using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Recalculates the weekly capacity snapshots of a window (for all employees or for one) and shows the snapshots that are stored,
/// which feed the billable utilization report.
/// </summary>
internal sealed partial class CapacitySnapshotsViewModel : ObservableObject
{
    private const int PastDays = 28;
    private const int FutureDays = 28;

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly ResourcePlanningFormatter _format;
    private readonly Dictionary<Guid, string> _employeeNames = [];

    [ObservableProperty]
    private DateTime? _fromDate;

    [ObservableProperty]
    private DateTime? _toDate;

    [ObservableProperty]
    private PlanningChoice? _selectedEmployee;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasData;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="CapacitySnapshotsViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="time">The clock.</param>
    public CapacitySnapshotsViewModel(ISender sender, ILocalizationService localization, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(time);

        _sender = sender;
        _localization = localization;
        _format = new ResourcePlanningFormatter(localization);
        var today = time.GetLocalNow().Date;
        _fromDate = today.AddDays(-PastDays);
        _toDate = today.AddDays(FutureDays);
        EmployeeChoices = [new PlanningChoice(string.Empty, _localization["ResourcePlanning_Snapshots_AllEmployees"])];
        _selectedEmployee = EmployeeChoices[0];
    }

    /// <summary>
    /// Gets the employees a recalculation can be limited to, the first entry being all of them.
    /// </summary>
    public ObservableCollection<PlanningChoice> EmployeeChoices { get; }

    /// <summary>
    /// Gets the stored snapshots of the window.
    /// </summary>
    public ObservableCollection<SnapshotRow> Rows { get; } = [];

    /// <summary>
    /// Loads the employees and the stored snapshots of the window.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        var failure = await PlanningCalls.ReadAsync(ReloadAsync, _localization);
        StatusMessage = failure ?? StatusMessage;
        IsBusy = false;
    }

    /// <summary>
    /// Recalculates the snapshots of the window and shows the result.
    /// </summary>
    /// <returns>A task representing the recalculation.</returns>
    [RelayCommand]
    public async Task RecalculateAsync()
    {
        var built = PlanningDraftBuilder.SnapshotWindow(Day(FromDate), Day(ToDate), EmployeeOf());
        if (built.Request is not { } request)
        {
            StatusMessage = _format.Describe("Setup", built.ErrorCode ?? string.Empty);

            return;
        }

        IsBusy = true;
        var result = await PlanningCalls.WriteAsync(() => _sender.Send(new RecalculateCapacitySnapshotsCommand(request)), _format, _localization);
        if (result.IsSuccess && result.Value is { } summary)
        {
            var message = string.Format(_format.Culture, _localization["ResourcePlanning_Snapshots_Recalculated"], summary.Weeks.ToString(_format.Culture), summary.Snapshots.ToString(_format.Culture));
            var failure = await PlanningCalls.ReadAsync(ReloadAsync, _localization);
            StatusMessage = failure ?? message;
        }
        else
        {
            StatusMessage = result.IsSuccess ? _localization["ResourcePlanning_SaveFailed"] : result.Message;
        }

        IsBusy = false;
    }

    private static DateOnly? Day(DateTime? value) => value is { } date ? DateOnly.FromDateTime(date) : null;

    private Guid? EmployeeOf() => Guid.TryParse(SelectedEmployee?.Key, out var id) ? id : null;

    private async Task ReloadAsync()
    {
        await LoadEmployeesAsync();
        if (Day(FromDate) is not { } from || Day(ToDate) is not { } to || to < from)
        {
            StatusMessage = _format.Describe("Setup", PlanningDraftBuilder.SnapshotInvalidPeriod);

            return;
        }

        var snapshots = await _sender.Send(new GetCapacitySnapshotsQuery(from, to, EmployeeOf()));

        Rows.Clear();
        foreach (var snapshot in snapshots.OrderBy(item => item.WeekStartDate).ThenBy(item => NameOf(item.EmployeeId), StringComparer.CurrentCultureIgnoreCase))
        {
            Rows.Add(new SnapshotRow(
                snapshot,
                NameOf(snapshot.EmployeeId),
                _format.Day(snapshot.WeekStartDate),
                _format.Hours(snapshot.NetAvailableHours),
                _format.Hours(snapshot.BillableBookedHours),
                _format.Hours(snapshot.InternalBookedHours),
                _format.Percent(snapshot.UtilizationRatePercent)));
        }

        HasData = Rows.Count > 0;
        StatusMessage = HasData ? string.Empty : _localization["ResourcePlanning_Snapshots_Empty"];
    }

    private string NameOf(Guid employeeId) => _employeeNames.TryGetValue(employeeId, out var name) ? name : employeeId.ToString("D", CultureInfo.InvariantCulture);

    private async Task LoadEmployeesAsync()
    {
        if (EmployeeChoices.Count > 1)
        {
            return;
        }

        var resources = await _sender.Send(new GetPlanningResourcesQuery(DepartmentId: null));
        foreach (var resource in resources.OrderBy(item => item.FullName, StringComparer.CurrentCultureIgnoreCase))
        {
            _employeeNames[resource.EmployeeId] = resource.FullName;
            EmployeeChoices.Add(new PlanningChoice(resource.EmployeeId.ToString("D", CultureInfo.InvariantCulture), resource.FullName));
        }
    }
}
