// <copyright file="TimeTrackingViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.TimeTracking;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.ViewModels;

/// <summary>
/// Manages labor time tracking, employee clock-in/out records, work session durations, and regulatory compliance monitoring (e.g. Italian D.Lgs 66/2003 and Swiss ArG).
/// </summary>
/// <param name="mediator">The MediatR sender used to execute time-tracking queries and clocking commands.</param>
public sealed partial class TimeTrackingViewModel(ISender mediator) : ViewModelBase
{
    private readonly ISender mediator = mediator;

    /// <summary>
    /// Gets or sets the collection of recorded employee work shifts and time entries.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<TimeEntryDto> TimeEntries { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the current employee has an active open clock-in session.
    /// </summary>
    [ObservableProperty]
    public partial bool IsCurrentlyClockedIn { get; set; }

    /// <summary>
    /// Gets or sets the elapsed time display string (formatted as HH:mm:ss) for the current active work session.
    /// </summary>
    [ObservableProperty]
    public partial string ActiveSessionDuration { get; set; } = "00:00:00";

    /// <summary>
    /// Gets or sets the regulatory compliance assessment text summarizing rest period and maximum weekly hour limits.
    /// </summary>
    [ObservableProperty]
    public partial string ComplianceStatusText { get; set; } =
        "Conforme: D.Lgs 66/2003 (Riposo 11h) & ArG Svizzera (Limite 45h/settimana)";

    /// <summary>
    /// Asynchronously initializes the ViewModel by querying existing time entry records.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous initialization operation.</returns>
    public override Task InitializeAsync() => LoadEntriesAsync();

    /// <summary>
    /// Dispatches a <see cref="GetTimeEntriesQuery"/> via MediatR to fetch recorded time entries and calculates current clock-in state.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous query operation.</returns>
    [RelayCommand]
    public async Task LoadEntriesAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var list = await mediator.Send(new GetTimeEntriesQuery());
            TimeEntries = [with(list)];
            IsCurrentlyClockedIn = TimeEntries.Any(t => !t.ClockOutUtc.HasValue);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Dispatches a <see cref="ClockInCommand"/> to record the beginning of a work shift with geofencing and badge terminal details.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous clock-in operation.</returns>
    [RelayCommand]
    public async Task ClockInAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            // Sample Employee Id
            var dummyEmployeeId =
                new Guid(
                    0x11111111,
                    0x2222,
                    0x3333,
                    0x44,
                    0x44,
                    0x55,
                    0x55,
                    0x55,
                    0x55,
                    0x55,
                    0x55) /* 11111111-2222-3333-4444-555555555555 */;
            var entry = await mediator.Send(
                new ClockInCommand(
                    dummyEmployeeId,
                    RecordingType.Detailed,
                    45.4642,
                    9.1900,
                    "BADGE-IT-01"));
            TimeEntries.Insert(
                0,
                entry);
            IsCurrentlyClockedIn = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Dispatches a <see cref="ClockOutCommand"/> to terminate the active work shift and logs unpaid break minutes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous clock-out operation.</returns>
    [RelayCommand]
    public async Task ClockOutAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var dummyEmployeeId =
                new Guid(
                    0x11111111,
                    0x2222,
                    0x3333,
                    0x44,
                    0x44,
                    0x55,
                    0x55,
                    0x55,
                    0x55,
                    0x55,
                    0x55) /* 11111111-2222-3333-4444-555555555555 */;
            _ = await mediator.Send(
                new ClockOutCommand(
                    dummyEmployeeId,
                    30));
            IsCurrentlyClockedIn = false;
            await LoadEntriesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
