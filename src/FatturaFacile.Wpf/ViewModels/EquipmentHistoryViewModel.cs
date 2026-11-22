// <copyright file="EquipmentHistoryViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.FieldService;
using FatturaFacile.Application.FieldService;
using FluentValidation;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Drives the equipment service history: the customer assets with their meters, and for the selected one the signed service
/// reports, the parts replaced (covered ones marked) and the meter progression. It reads the cache first, so it works offline.
/// </summary>
internal sealed partial class EquipmentHistoryViewModel : ObservableObject
{
    private const string NoValue = "-";
    private const int ShortIdLength = 8;

    private readonly ISender _sender;
    private readonly FieldServiceLabels _labels;

    private bool _suppressSelection;

    [ObservableProperty]
    private FieldServiceEquipmentRow? _selectedEquipment;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private bool _hasHistory;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _lastSyncedText = string.Empty;

    [ObservableProperty]
    private string _summaryText = NoValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="EquipmentHistoryViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    public EquipmentHistoryViewModel(ISender sender, ILocalizationService localization)
    {
        _sender = sender;
        _labels = new FieldServiceLabels(localization);
        LastSyncedText = _labels.Key("FieldService_NeverSynced");
    }

    /// <summary>
    /// Gets the customer assets of the picker.
    /// </summary>
    public ObservableCollection<FieldServiceEquipmentRow> Equipment { get; } = [];

    /// <summary>
    /// Gets the service reports of the selected asset, oldest first.
    /// </summary>
    public ObservableCollection<EquipmentHistoryRow> Entries { get; } = [];

    /// <summary>
    /// Loads the equipment and the history of the selected asset from the cache, pulling from the server when the cache is empty.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        await GuardAsync(async () =>
        {
            if (!await ShowEquipmentAsync())
            {
                await PullAsync(FieldServiceSyncScope.Equipment, key: null);
                _ = await ShowEquipmentAsync();
            }

            await ShowHistoryAsync(syncIfMissing: true);
        });
    }

    /// <summary>
    /// Pulls the equipment and the history of the selected asset from the server; offline, the cache stays on screen.
    /// </summary>
    /// <returns>A task representing the sync.</returns>
    [RelayCommand]
    public async Task SyncAsync()
    {
        await GuardAsync(async () =>
        {
            await PullAsync(FieldServiceSyncScope.Equipment, key: null);
            _ = await ShowEquipmentAsync();

            if (SelectedEquipment is { } selected)
            {
                await PullAsync(FieldServiceSyncScope.History, selected.Id);
            }

            await ShowHistoryAsync(syncIfMissing: false);
        });
    }

    partial void OnSelectedEquipmentChanged(FieldServiceEquipmentRow? value)
    {
        if (_suppressSelection || value is null)
        {
            return;
        }

        _ = GuardAsync(() => ShowHistoryAsync(syncIfMissing: true));
    }

    private async Task GuardAsync(Func<Task> action)
    {
        IsBusy = true;

        try
        {
            await action();
        }
        catch (ValidationException ex)
        {
            StatusMessage = ex.Errors.Select(error => _labels.Key(error.ErrorMessage)).FirstOrDefault() ?? ex.Message;
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

    private async Task PullAsync(FieldServiceSyncScope scope, string? key)
    {
        var result = await _sender.Send(new SyncFieldServiceCommand(scope, key, From: null, To: null));
        IsOffline = result.Status != FieldServiceCallStatus.Ok;
        StatusMessage = _labels.Call(result.Status, result.Detail);
    }

    private async Task<bool> ShowEquipmentAsync()
    {
        var cached = await _sender.Send(new GetFieldServiceEquipmentQuery());
        var keep = SelectedEquipment?.Id;

        _suppressSelection = true;
        try
        {
            Equipment.Clear();
            foreach (var asset in cached?.Value ?? [])
            {
                Equipment.Add(new FieldServiceEquipmentRow(
                    asset.Id,
                    asset.SerialNumber,
                    asset.SiteAddress ?? NoValue,
                    _labels.Format("FieldService_Meters", _labels.Number(asset.OperatingHours), asset.CycleCount.ToString(_labels.Culture)),
                    _labels.Day(asset.NextServiceDate)));
            }

            SelectedEquipment = Equipment.FirstOrDefault(row => string.Equals(row.Id, keep, StringComparison.Ordinal)) ?? Equipment.FirstOrDefault();
        }
        finally
        {
            _suppressSelection = false;
        }

        return Equipment.Count > 0;
    }

    private async Task ShowHistoryAsync(bool syncIfMissing)
    {
        Entries.Clear();
        if (SelectedEquipment is not { } selected)
        {
            HasHistory = false;
            SummaryText = NoValue;

            return;
        }

        var cached = await _sender.Send(new GetCustomerEquipmentServiceHistoryQuery(selected.Id));
        if (cached is null && syncIfMissing)
        {
            await PullAsync(FieldServiceSyncScope.History, selected.Id);
            cached = await _sender.Send(new GetCustomerEquipmentServiceHistoryQuery(selected.Id));
        }

        HasHistory = cached is not null;
        if (cached is null)
        {
            SummaryText = NoValue;
            LastSyncedText = _labels.Key("FieldService_NeverSynced");

            return;
        }

        var history = cached.Value;
        LastSyncedText = _labels.Format("FieldService_LastSynced", cached.SyncedAtUtc.ToLocalTime().ToString("g", _labels.Culture));
        SummaryText = _labels.Format(
            "FieldService_HistorySummary",
            history.SerialNumber,
            _labels.Number(history.OperatingHours),
            history.CycleCount.ToString(_labels.Culture),
            _labels.Day(history.LastServiceDate),
            _labels.Day(history.NextServiceDate));

        foreach (var entry in history.Entries)
        {
            Entries.Add(EntryRow(entry));
        }
    }

    private EquipmentHistoryRow EntryRow(EquipmentHistoryEntry entry) => new(
        entry.ReportId,
        entry.ReportNumber,
        _labels.Moment(entry.SignedAt),
        _labels.WorkType(entry.WorkType),
        entry.Summary,
        _labels.Number(entry.LaborHours),
        _labels.Format("FieldService_Report_Meter", entry.MeterHours is { } hours ? _labels.Number(hours) : NoValue, entry.MeterCycles?.ToString(_labels.Culture) ?? NoValue),
        entry.Parts.Count == 0 ? NoValue : string.Join("; ", entry.Parts.Select(PartText)));

    private string PartText(EquipmentHistoryPart part) =>
        part.ProductId[..Math.Min(ShortIdLength, part.ProductId.Length)] + " x" + _labels.Number(part.Quantity) + (part.Covered ? " (" + _labels.Key("FieldService_Part_Covered") + ")" : string.Empty);
}
