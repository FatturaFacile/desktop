using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.TimeTracking;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.ViewModels;

/// <summary>
/// Represents time tracking view model.
/// </summary>
/// <param name="mediator">The mediator.</param>
public sealed partial class TimeTrackingViewModel(ISender mediator) : ViewModelBase
{
    private readonly ISender _mediator = mediator;

/// <summary>
/// Gets or sets the time entries.
/// </summary>
    [ObservableProperty]
    public partial ObservableCollection<TimeEntryDto> TimeEntries { get; set; } = [];

/// <summary>
/// Gets or sets a value indicating whether currently clocked in.
/// </summary>
    [ObservableProperty]
    public partial bool IsCurrentlyClockedIn { get; set; }

/// <summary>
/// Gets or sets the active session duration.
/// </summary>
    [ObservableProperty]
    public partial string ActiveSessionDuration { get; set; } = "00:00:00";

/// <summary>
/// Gets or sets the compliance status text.
/// </summary>
    [ObservableProperty]
    public partial string ComplianceStatusText { get; set; } = "Conforme: D.Lgs 66/2003 (Riposo 11h) & ArG Svizzera (Limite 45h/settimana)";

/// <summary>
/// Initialize Async.
/// </summary>
/// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public override Task InitializeAsync() => LoadEntriesAsync();

/// <summary>
/// Load Entries Async.
/// </summary>
/// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    [RelayCommand]
    public async Task LoadEntriesAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var list = await _mediator.Send(new GetTimeEntriesQuery());
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
/// Clock In Async.
/// </summary>
/// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    [RelayCommand]
    public async Task ClockInAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            // Sample Employee Id
            var dummyEmployeeId = new Guid(0x11111111, 0x2222, 0x3333, 0x44, 0x44, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55) /* 11111111-2222-3333-4444-555555555555 */;
            var entry = await _mediator.Send(new ClockInCommand(dummyEmployeeId, RecordingType.Detailed, 45.4642, 9.1900, "BADGE-IT-01"));
            TimeEntries.Insert(0, entry);
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
/// Clock Out Async.
/// </summary>
/// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    [RelayCommand]
    public async Task ClockOutAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var dummyEmployeeId = new Guid(0x11111111, 0x2222, 0x3333, 0x44, 0x44, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55) /* 11111111-2222-3333-4444-555555555555 */;
            _ = await _mediator.Send(new ClockOutCommand(dummyEmployeeId, 30));
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
