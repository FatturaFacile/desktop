// <copyright file="TimeTrackingViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Records;
using FatturaFacile.Application.Common.Reporting;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Payroll.Commands;
using FatturaFacile.Application.Features.Payroll.Queries;
using FatturaFacile.Application.Features.TimeTracking.Commands;
using FatturaFacile.Application.Features.TimeTracking.Queries;
using FatturaFacile.Application.Features.Users.Queries;
using FatturaFacile.Application.TimeTracking;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Wpf.Services;
using FatturaFacile.Wpf.ViewModels.Payroll;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Working time recording (timbrature) according to the Swiss Labour Act: personal clocking, monthly evaluation with
/// the legal checks, audited corrections, simplified daily records, holiday calendar and printable time sheets.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TimeTrackingViewModel"/> class.
/// </remarks>
/// <param name="sender">The MediatR sender.</param>
/// <param name="localizationService">The localization service.</param>
/// <param name="printService">The print service.</param>
internal sealed partial class TimeTrackingViewModel(ISender sender, ILocalizationService localizationService, IReportPrintService printService)
    : PayrollSectionViewModel(sender, localizationService)
{
    [ObservableProperty]
    private ClockStatusDto? _clockStatus;

    [ObservableProperty]
    private HrEmployee? _selectedEmployee;

    [ObservableProperty]
    private int _year = DateTime.Today.Year;

    [ObservableProperty]
    private int _month = DateTime.Today.Month;

    [ObservableProperty]
    private WorkingTimeReport? _report;

    [ObservableProperty]
    private TimeClockEvent? _selectedEvent;

    [ObservableProperty]
    private DateTime _editDate = DateTime.Today;

    [ObservableProperty]
    private string _editTime = "08:00";

    [ObservableProperty]
    private string _editType = TimeEventType.In;

    [ObservableProperty]
    private string _editNote = string.Empty;

    [ObservableProperty]
    private string _reason = string.Empty;

    [ObservableProperty]
    private DateTime _dailyDate = DateTime.Today;

    [ObservableProperty]
    private decimal _dailyHours = 8m;

    [ObservableProperty]
    private string _dailyStart = string.Empty;

    [ObservableProperty]
    private string _dailyEnd = string.Empty;

    [ObservableProperty]
    private PublicHoliday _newHoliday = new() { Date = DateTime.Today };

    [ObservableProperty]
    private PublicHoliday? _selectedHoliday;

    [ObservableProperty]
    private UserDto? _selectedUser;

    /// <summary>
    /// Gets the employees.
    /// </summary>
    public ObservableCollection<HrEmployee> Employees { get; } = [];

    /// <summary>
    /// Gets the days of the evaluated period.
    /// </summary>
    public ObservableCollection<WorkingDayResult> Days { get; } = [];

    /// <summary>
    /// Gets the weeks of the evaluated period.
    /// </summary>
    public ObservableCollection<WorkingWeekResult> Weeks { get; } = [];

    /// <summary>
    /// Gets the clock events of the period (deleted ones included).
    /// </summary>
    public ObservableCollection<TimeClockEvent> Events { get; } = [];

    /// <summary>
    /// Gets the audit trail of the period.
    /// </summary>
    public ObservableCollection<TimeAuditEntry> Audit { get; } = [];

    /// <summary>
    /// Gets the public holidays of the year.
    /// </summary>
    public ObservableCollection<PublicHoliday> Holidays { get; } = [];

    /// <summary>
    /// Gets the user accounts that can be linked to the employee.
    /// </summary>
    public ObservableCollection<UserDto> Users { get; } = [];

    /// <summary>
    /// Gets the period summary.
    /// </summary>
    public string Summary => Report is null
        ? string.Empty
        : string.Format(
            CultureInfo.CurrentCulture,
            Localization["Time_Summary"],
            WorkingTimeReport.FormatMinutes(Report.WorkedMinutes),
            WorkingTimeReport.FormatMinutes(Report.TargetMinutes),
            WorkingTimeReport.FormatMinutes(Report.CreditedMinutes),
            WorkingTimeReport.FormatMinutes(Report.BalanceMinutes),
            WorkingTimeReport.FormatMinutes(Report.StatutoryOvertimeMinutes),
            WorkingTimeReport.FormatMinutes(Report.StatutoryOvertimeYearMinutes),
            WorkingTimeReport.FormatMinutes(Report.StatutoryOvertimeCapMinutes));

    /// <summary>
    /// Gets the period issues as text.
    /// </summary>
    public string PeriodIssues => Report is null ? string.Empty : string.Join(Environment.NewLine, Report.Issues);

    /// <summary>
    /// Gets the personal clock state as text.
    /// </summary>
    public string ClockText => ClockStatus is null || string.Equals(ClockStatus.State, "NotLinked", StringComparison.Ordinal)
        ? Localization["Time_NotLinked"]
        : string.Format(
            CultureInfo.CurrentCulture,
            Localization["Time_ClockState"],
            ClockStatus.EmployeeName,
            Localization["Time_State_" + ClockStatus.State],
            WorkingTimeReport.FormatMinutes(ClockStatus.WorkedTodayMinutes));

    private DateTime PeriodStart => new(Year, Month, 1, 0, 0, 0, DateTimeKind.Unspecified);

    private DateTime PeriodEnd => PeriodStart.AddMonths(1).AddDays(-1);

    /// <inheritdoc/>
    [RelayCommand]
    public override Task LoadAsync() => RunAsync(async () =>
    {
        ClockStatus = await Sender.Send(new GetClockStatusQuery());

        try
        {
            var selectedId = SelectedEmployee?.Id;
            var employees = await Sender.Send(new GetPayrollRecordsQuery<HrEmployee>());
            Replace(Employees, employees.OrderBy(static e => e.LastName, StringComparer.CurrentCulture));
            SelectedEmployee = Employees.FirstOrDefault(e => e.Id == selectedId)
                ?? Employees.FirstOrDefault(e => e.Id == ClockStatus.EmployeeId)
                ?? Employees.FirstOrDefault();
        }
        catch (UnauthorizedAccessException)
        {
            Employees.Clear();
        }

        await LoadUsersAsync();
        await LoadHolidaysAsync();
    });

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();

        foreach (var item in items)
        {
            target.Add(item);
        }
    }

    private static TimeSpan? ParseTime(string text) =>
        TimeSpan.TryParseExact(text.Trim(), ["h\\:mm", "hh\\:mm"], CultureInfo.InvariantCulture, out var time) ? time : null;

    partial void OnClockStatusChanged(ClockStatusDto? value) => OnPropertyChanged(nameof(ClockText));

    partial void OnReportChanged(WorkingTimeReport? value)
    {
        OnPropertyChanged(nameof(Summary));
        OnPropertyChanged(nameof(PeriodIssues));
    }

    partial void OnSelectedEmployeeChanged(HrEmployee? value) => _ = EvaluateAsync();

    partial void OnYearChanged(int value) => _ = EvaluateAsync();

    partial void OnMonthChanged(int value) => _ = EvaluateAsync();

    partial void OnSelectedEventChanged(TimeClockEvent? value)
    {
        if (value is null)
        {
            return;
        }

        EditDate = value.Timestamp.Date;
        EditTime = value.Timestamp.ToString("HH:mm", CultureInfo.InvariantCulture);
        EditType = value.EventType;
        EditNote = value.Note ?? string.Empty;
    }

    [RelayCommand]
    private Task ClockAsync(string? eventType) => RunAsync(async () =>
    {
        ClockStatus = await Sender.Send(new ClockCommand(eventType ?? TimeEventType.In));
        await EvaluateAsync();
    });

    [RelayCommand]
    private Task EvaluateAsync() => SelectedEmployee is not { } employee
        ? Task.CompletedTask
        : RunAsync(async () =>
        {
            Report = await Sender.Send(new GetWorkingTimeReportQuery(employee.Id, PeriodStart, PeriodEnd));
            Replace(Days, Report.Days);
            Replace(Weeks, Report.Weeks);
            Replace(Events, await Sender.Send(new GetTimeEventsQuery(employee.Id, PeriodStart, PeriodEnd, IncludeDeleted: true)));

            try
            {
                Replace(Audit, await Sender.Send(new GetTimeAuditQuery(employee.Id, PeriodStart, PeriodEnd.AddDays(31))));
            }
            catch (UnauthorizedAccessException)
            {
                Audit.Clear();
            }
        });

    [RelayCommand]
    private void NewEvent()
    {
        SelectedEvent = null;
        EditDate = DateTime.Today;
        EditTime = DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture);
        EditType = TimeEventType.In;
        EditNote = string.Empty;
    }

    [RelayCommand]
    private Task SaveEventAsync()
    {
        if (SelectedEmployee is not { } employee)
        {
            return Task.CompletedTask;
        }

        if (ParseTime(EditTime) is not { } time)
        {
            StatusMessage = Localization["Time_InvalidTime"];
            return Task.CompletedTask;
        }

        TimeClockEvent values = new()
        {
            Id = SelectedEvent?.Id ?? Guid.Empty,
            EmployeeId = employee.Id,
            Timestamp = EditDate.Date + time,
            EventType = EditType,
            Note = string.IsNullOrWhiteSpace(EditNote) ? null : EditNote,
        };

        return RunAsync(
            async () =>
            {
                _ = await Sender.Send(new SaveTimeEventCommand(values, Reason));
                Reason = string.Empty;
                await EvaluateAsync();
            },
            "Common_Saved");
    }

    [RelayCommand]
    private Task DeleteEventAsync() => SelectedEvent is null
        ? Task.CompletedTask
        : RunAsync(
            async () =>
            {
                _ = await Sender.Send(new DeleteTimeEventCommand(SelectedEvent.Id, Reason));
                Reason = string.Empty;
                await EvaluateAsync();
            },
            "Common_Deleted");

    [RelayCommand]
    private Task SaveDailyAsync()
    {
        if (SelectedEmployee is not { } employee)
        {
            return Task.CompletedTask;
        }

        var start = ParseTime(DailyStart);
        var end = ParseTime(DailyEnd);
        TimeDailyRecord record = new()
        {
            EmployeeId = employee.Id,
            Date = DailyDate.Date,
            WorkedMinutes = (int)Math.Round(DailyHours * 60m, MidpointRounding.AwayFromZero),
            StartTime = start is { } s ? DailyDate.Date + s : null,
            EndTime = end is { } e ? DailyDate.Date + e : null,
        };

        return RunAsync(
            async () =>
            {
                _ = await Sender.Send(new SaveTimeDailyRecordCommand(record, string.IsNullOrWhiteSpace(Reason) ? null : Reason));
                await EvaluateAsync();
            },
            "Common_Saved");
    }

    [RelayCommand]
    private Task PrintAsync() => SelectedEmployee is null
        ? Task.CompletedTask
        : RunAsync(async () => _ = await printService.PrintAsync(ReportDocumentType.TimeSheet, SelectedEmployee.Id, PeriodStart, PeriodEnd));

    [RelayCommand]
    private Task LinkUserAsync()
    {
        if (SelectedEmployee is not { } employee || SelectedUser is not { } user)
        {
            return Task.CompletedTask;
        }

        var copy = RecordCopier.Clone(employee);
        copy.UserId = user.Id;
        return RunAsync(
            async () =>
            {
                _ = await Sender.Send(new SavePayrollRecordCommand<HrEmployee>(copy));
                await LoadAsync();
            },
            "Common_Saved");
    }

    [RelayCommand]
    private Task CreateHolidaysAsync() => RunAsync(async () =>
    {
        var count = await Sender.Send(new EnsurePublicHolidaysCommand(Year));
        await LoadHolidaysAsync();
        StatusMessage = string.Format(CultureInfo.CurrentCulture, Localization["Time_HolidaysCreated"], count);
    });

    [RelayCommand]
    private Task AddHolidayAsync() => RunAsync(async () =>
    {
        _ = await Sender.Send(new SavePayrollRecordCommand<PublicHoliday>(NewHoliday));
        NewHoliday = new PublicHoliday { Date = NewHoliday.Date.AddDays(1) };
        await LoadHolidaysAsync();
    });

    [RelayCommand]
    private Task RemoveHolidayAsync() => SelectedHoliday is null
        ? Task.CompletedTask
        : RunAsync(async () =>
        {
            _ = await Sender.Send(new DeletePayrollRecordCommand<PublicHoliday>(SelectedHoliday.Id));
            await LoadHolidaysAsync();
        });

    private async Task LoadHolidaysAsync()
    {
        try
        {
            var from = new DateTime(Year, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
            var to = from.AddYears(1);
            var holidays = await Sender.Send(new GetPayrollRecordsQuery<PublicHoliday>(h => h.Date >= from && h.Date < to));
            Replace(Holidays, holidays.OrderBy(static h => h.Date));
        }
        catch (UnauthorizedAccessException)
        {
            Holidays.Clear();
        }
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            Replace(Users, await Sender.Send(new GetUsersQuery()));
        }
        catch (UnauthorizedAccessException)
        {
            Users.Clear();
        }
    }
}
