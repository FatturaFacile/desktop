// <copyright file="FieldServiceBoardViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.FieldService;
using FatturaFacile.Application.FieldService;
using FluentValidation;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Drives the dispatcher board: the queue of unassigned work orders, a Gantt-like timeline per technician with the skill and van
/// stock warnings a dispatch would raise, the dispatch and reschedule action and the trigger of the preventive maintenance engine.
/// It reads the local cache first, so the last synced board shows offline; dispatching and the preventive run need the server.
/// </summary>
internal sealed partial class FieldServiceBoardViewModel : ObservableObject
{
    private const double TimelineWidthPixels = 900.0;
    private const double MinBarWidth = 6.0;
    private const double FullScale = 1.0;
    private const int WindowDays = 7;
    private const string WarningSeparator = " | ";

    private readonly ISender _sender;
    private readonly FieldServiceLabels _labels;

    private FieldServiceBoard? _board;
    private DateTimeOffset? _windowStart;

    [ObservableProperty]
    private FieldServiceUnassignedRow? _selectedUnassigned;

    [ObservableProperty]
    private FieldServiceLaneRow? _selectedLane;

    [ObservableProperty]
    private DateTime _dispatchDate = DateTime.Today;

    [ObservableProperty]
    private string _startTimeText = "08:00";

    [ObservableProperty]
    private string _durationText = "2";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private bool _hasBoard;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _lastSyncedText = string.Empty;

    [ObservableProperty]
    private string _windowText = string.Empty;

    [ObservableProperty]
    private string _preventiveText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldServiceBoardViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    public FieldServiceBoardViewModel(ISender sender, ILocalizationService localization)
    {
        _sender = sender;
        _labels = new FieldServiceLabels(localization);
        LastSyncedText = _labels.Key("FieldService_NeverSynced");
        Map = new FieldServiceMapViewModel(_labels);
        Map.WorkOrderSelected += OnMapWorkOrderSelected;
    }

    /// <summary>
    /// Occurs when the server data changed (a dispatch or a preventive run), so the other tabs refresh.
    /// </summary>
    public event EventHandler? DataChanged;

    /// <summary>
    /// Gets the width of the timeline, in device-independent pixels.
    /// </summary>
    public double TimelineWidth => TimelineWidthPixels;

    /// <summary>
    /// Gets the map of the work order sites and the routes of the technicians.
    /// </summary>
    public FieldServiceMapViewModel Map { get; }

    /// <summary>
    /// Gets the work orders waiting for a technician.
    /// </summary>
    public ObservableCollection<FieldServiceUnassignedRow> Unassigned { get; } = [];

    /// <summary>
    /// Gets the technician lanes.
    /// </summary>
    public ObservableCollection<FieldServiceLaneRow> Lanes { get; } = [];

    /// <summary>
    /// Gets the blockers the server reported for the last dispatch.
    /// </summary>
    public ObservableCollection<string> DispatchWarnings { get; } = [];

    /// <summary>
    /// Loads the board from the cache, pulling it from the server when the cache is empty.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        await GuardAsync(async () =>
        {
            if (!await ShowCachedAsync())
            {
                await PullAsync();
                _ = await ShowCachedAsync();
            }
        });
    }

    /// <summary>
    /// Pulls the board of the current window from the server; offline, the cached board stays on screen.
    /// </summary>
    /// <returns>A task representing the sync.</returns>
    [RelayCommand]
    public async Task SyncAsync()
    {
        await GuardAsync(async () =>
        {
            await PullAsync();
            _ = await ShowCachedAsync();
        });
    }

    /// <summary>
    /// Moves the window one week back and pulls it.
    /// </summary>
    /// <returns>A task representing the move.</returns>
    [RelayCommand]
    public Task PreviousWeekAsync() => ShiftAsync(-WindowDays);

    /// <summary>
    /// Moves the window one week forward and pulls it.
    /// </summary>
    /// <returns>A task representing the move.</returns>
    [RelayCommand]
    public Task NextWeekAsync() => ShiftAsync(WindowDays);

    /// <summary>
    /// Dispatches (or reschedules) the selected work order to the selected technician in the chosen window. The server checks
    /// schedule, skills and van stock and returns every blocker together.
    /// </summary>
    /// <returns>A task representing the dispatch.</returns>
    [RelayCommand]
    public async Task DispatchAsync()
    {
        if (SelectedUnassigned is not { } order || SelectedLane is not { } lane)
        {
            StatusMessage = _labels.Key("FieldService_SelectToDispatch");

            return;
        }

        if (!TryWindow(out var start, out var end))
        {
            StatusMessage = _labels.Key("FieldService_Err_Window");

            return;
        }

        await GuardAsync(async () =>
        {
            var result = await _sender.Send(new DispatchTechnicianWorkOrderCommand(order.WorkOrderId, lane.Lane.TechnicianId, start, end));
            ShowDispatchResult(result, order, lane);

            if (result.Status == FieldServiceCallStatus.Ok)
            {
                await PullAsync();
                _ = await ShowCachedAsync();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        });
    }

    /// <summary>
    /// Runs the preventive maintenance engine of the server: it creates the work orders of the cycles that are due, once per cycle.
    /// </summary>
    /// <returns>A task representing the run.</returns>
    [RelayCommand]
    public async Task RunPreventiveAsync()
    {
        await GuardAsync(async () =>
        {
            var result = await _sender.Send(new GeneratePreventiveMaintenanceWorkOrdersCommand());
            PreventiveText = result.Status == FieldServiceCallStatus.Ok
                ? _labels.Format("FieldService_PreventiveDone", result.Evaluated.ToString(_labels.Culture), result.Created.ToString(_labels.Culture), result.Skipped.ToString(_labels.Culture))
                : _labels.Call(result.Status, result.Detail);
            StatusMessage = PreventiveText;

            if (result.Status == FieldServiceCallStatus.Ok)
            {
                await PullAsync();
                _ = await ShowCachedAsync();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        });
    }

    private static string ValidationText(ValidationException ex, FieldServiceLabels labels) =>
        ex.Errors.Select(error => labels.Key(error.ErrorMessage)).FirstOrDefault() ?? ex.Message;

    private static string? Coordinates(FieldServiceBlock order) =>
        order.Latitude is { } lat && order.Longitude is { } lng
            ? lat.ToString("0.#####", CultureInfo.InvariantCulture) + ", " + lng.ToString("0.#####", CultureInfo.InvariantCulture)
            : null;

    partial void OnSelectedUnassignedChanged(FieldServiceUnassignedRow? value)
    {
        UpdateWarnings();
        Map.Highlight(value?.WorkOrderId);
    }

    partial void OnDispatchDateChanged(DateTime value) => UpdateWarnings();

    partial void OnStartTimeTextChanged(string value) => UpdateWarnings();

    partial void OnDurationTextChanged(string value) => UpdateWarnings();

    private async Task GuardAsync(Func<Task> action)
    {
        IsBusy = true;

        try
        {
            await action();
        }
        catch (ValidationException ex)
        {
            StatusMessage = ValidationText(ex, _labels);
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

    private Task ShiftAsync(int days)
    {
        _windowStart = (_windowStart ?? _board?.From ?? DateTimeOffset.UtcNow).AddDays(days);

        return SyncAsync();
    }

    private async Task PullAsync()
    {
        var from = _windowStart;
        var to = from?.AddDays(WindowDays);
        var result = await _sender.Send(new SyncFieldServiceCommand(FieldServiceSyncScope.Board, Key: null, from, to));
        IsOffline = result.Status != FieldServiceCallStatus.Ok;
        StatusMessage = _labels.Call(result.Status, result.Detail);
    }

    private async Task<bool> ShowCachedAsync()
    {
        var cached = await _sender.Send(new GetDispatcherGanttAndMapBoardQuery());
        Apply(cached);

        return cached is not null;
    }

    private void Apply(FieldServiceCached<FieldServiceBoard>? cached)
    {
        var keepOrder = SelectedUnassigned?.WorkOrderId;
        var keepLane = SelectedLane?.Lane.TechnicianId;

        Unassigned.Clear();
        Lanes.Clear();
        _board = cached?.Value;
        HasBoard = cached is not null;

        if (cached is null)
        {
            WindowText = string.Empty;
            LastSyncedText = _labels.Key("FieldService_NeverSynced");
            SelectedUnassigned = null;
            SelectedLane = null;
            Map.Clear();

            return;
        }

        var board = cached.Value;
        _windowStart = board.From;
        WindowText = _labels.Format("FieldService_Window", _labels.Day(DateOnly.FromDateTime(board.From.LocalDateTime)), _labels.Day(DateOnly.FromDateTime(board.To.LocalDateTime)));
        LastSyncedText = _labels.Format("FieldService_LastSynced", cached.SyncedAtUtc.ToLocalTime().ToString("g", _labels.Culture));

        foreach (var order in board.Unassigned)
        {
            Unassigned.Add(UnassignedRowOf(order));
        }

        foreach (var lane in board.Lanes)
        {
            Lanes.Add(LaneRowOf(lane, board));
        }

        SelectedUnassigned = Unassigned.FirstOrDefault(row => string.Equals(row.WorkOrderId, keepOrder, StringComparison.Ordinal));
        SelectedLane = Lanes.FirstOrDefault(row => string.Equals(row.Lane.TechnicianId, keepLane, StringComparison.Ordinal));
        UpdateWarnings();
        Map.Show(board, SelectedUnassigned?.WorkOrderId);
    }

    private void OnMapWorkOrderSelected(object? sender, string workOrderId)
    {
        var queued = Unassigned.FirstOrDefault(row => string.Equals(row.WorkOrderId, workOrderId, StringComparison.Ordinal));
        if (queued is not null)
        {
            SelectedUnassigned = queued;

            return;
        }

        var lane = Lanes.FirstOrDefault(row => row.Lane.Blocks.Any(block => string.Equals(block.WorkOrderId, workOrderId, StringComparison.Ordinal)));
        if (lane is not null)
        {
            SelectedLane = lane;
        }
    }

    private FieldServiceUnassignedRow UnassignedRowOf(FieldServiceBlock order)
    {
        var site = string.Join(", ", new[] { order.Address, Coordinates(order) }.Where(part => !string.IsNullOrEmpty(part)));

        return new FieldServiceUnassignedRow(
            order.WorkOrderId,
            order.Number,
            order.Title,
            _labels.WorkType(order.WorkType),
            _labels.Priority(order.Priority),
            order.Priority,
            string.Join(", ", order.RequiredSkills),
            site,
            order.RequiredParts.ToString(_labels.Culture));
    }

    private FieldServiceLaneRow LaneRowOf(FieldServiceLane lane, FieldServiceBoard board)
    {
        var name = lane.IsAvailable ? lane.Name : _labels.Format("FieldService_Unavailable", lane.Name);
        var skills = _labels.Format("FieldService_SkillsLine", lane.Skills.Count == 0 ? "-" : string.Join(", ", lane.Skills));
        var van = lane.VanReady
            ? _labels.Format("FieldService_VanReady", lane.VanProducts.ToString(_labels.Culture))
            : _labels.Key("FieldService_NoVan");

        return new FieldServiceLaneRow(lane, name, skills, van, lane.Blocks.Where(block => block.Start is not null && block.End is not null).Select(block => BarOf(block, board)));
    }

    private FieldServiceBarRow BarOf(FieldServiceBlock block, FieldServiceBoard board)
    {
        var total = Math.Max(1.0, (board.To - board.From).TotalSeconds);
        var left = Math.Clamp((block.Start!.Value - board.From).TotalSeconds / total, 0.0, FullScale) * TimelineWidthPixels;
        var right = Math.Clamp((block.End!.Value - board.From).TotalSeconds / total, 0.0, FullScale) * TimelineWidthPixels;
        var tooltip = block.Number + " " + block.Title + Environment.NewLine + _labels.Window(block.Start, block.End) + Environment.NewLine + _labels.Status(block.Status);

        return new FieldServiceBarRow(block.Number, block.Title, left, Math.Max(MinBarWidth, right - left), block.Status, tooltip);
    }

    private bool TryWindow(out DateTimeOffset start, out DateTimeOffset end)
    {
        start = default;
        end = default;

        if (!TimeSpan.TryParse(StartTimeText, _labels.Culture, out var time)
            || time < TimeSpan.Zero
            || time >= TimeSpan.FromDays(1)
            || !double.TryParse(DurationText, NumberStyles.Float, _labels.Culture, out var hours)
            || hours <= 0)
        {
            return false;
        }

        start = new DateTimeOffset(DateTime.SpecifyKind(DispatchDate.Date + time, DateTimeKind.Unspecified));
        end = start.AddHours(hours);

        return true;
    }

    private void UpdateWarnings()
    {
        var order = _board?.Unassigned.FirstOrDefault(block => string.Equals(block.WorkOrderId, SelectedUnassigned?.WorkOrderId, StringComparison.Ordinal));
        var hasWindow = TryWindow(out var start, out var end);

        foreach (var lane in Lanes)
        {
            lane.ShowWarnings(order is null || !hasWindow ? string.Empty : string.Join(WarningSeparator, FieldServiceDispatchPreflight.Evaluate(lane.Lane, order, start, end).Select(_labels.Warning)));
        }
    }

    private void ShowDispatchResult(FieldServiceDispatchResult result, FieldServiceUnassignedRow order, FieldServiceLaneRow lane)
    {
        DispatchWarnings.Clear();
        foreach (var warning in result.Warnings)
        {
            DispatchWarnings.Add(_labels.Warning(warning));
        }

        StatusMessage = result.Status switch
        {
            FieldServiceCallStatus.Ok => _labels.Format("FieldService_Dispatched", order.Number, lane.Lane.Name),
            FieldServiceCallStatus.Rejected => _labels.Key("FieldService_DispatchBlocked"),
            _ => _labels.Call(result.Status, result.Detail),
        };
        IsOffline = result.Status is FieldServiceCallStatus.Offline;
    }
}
