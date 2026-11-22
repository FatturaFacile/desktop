// <copyright file="SlaMonitorViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Helpdesk;
using FatturaFacile.Application.Helpdesk;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Drives the SLA monitoring view: mean time to first response and to resolution, SLA compliance and satisfaction per queue,
/// priority or contract over a period. Reads the server and falls back to the last cached answer offline.
/// </summary>
internal sealed partial class SlaMonitorViewModel : ObservableObject
{
    private const double GoodCompliance = 95d;
    private const double WarnCompliance = 80d;
    private const int DefaultPeriodDays = 30;
    private const int MinutesPerHour = 60;
    private const string NoValue = "-";
    private const string DateTimeFormat = "g";
    private const string Good = "Good";
    private const string Warn = "Warn";
    private const string Bad = "Bad";

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;

    [ObservableProperty]
    private DateTime _fromDate;

    [ObservableProperty]
    private DateTime _toDate;

    [ObservableProperty]
    private HelpdeskChoice? _selectedGroup;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private bool _hasData;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _lastSyncedText = string.Empty;

    [ObservableProperty]
    private string _totalText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="SlaMonitorViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="time">The clock.</param>
    public SlaMonitorViewModel(ISender sender, ILocalizationService localization, TimeProvider time)
    {
        _sender = sender;
        _localization = localization;
        var today = time.GetLocalNow().Date;
        _toDate = today;
        _fromDate = today.AddDays(-DefaultPeriodDays);
        GroupChoices =
        [
            new HelpdeskChoice("queue", _localization["SlaMonitor_Group_queue"]),
            new HelpdeskChoice("priority", _localization["SlaMonitor_Group_priority"]),
            new HelpdeskChoice("contract", _localization["SlaMonitor_Group_contract"]),
        ];
        _selectedGroup = GroupChoices[0];
        LastSyncedText = _localization["Helpdesk_NeverSynced"];
    }

    /// <summary>
    /// Gets the groupings the metrics can be split by.
    /// </summary>
    public IReadOnlyList<HelpdeskChoice> GroupChoices { get; }

    /// <summary>
    /// Gets the groups of the table.
    /// </summary>
    internal ObservableCollection<SlaMetricRow> Rows { get; } = [];

    /// <summary>
    /// Loads the metrics of the selected period and grouping.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            var group = SelectedGroup?.Key ?? GroupChoices[0].Key;
            var load = await _sender.Send(new GetSlaMetricsQuery(DateOnly.FromDateTime(FromDate), DateOnly.FromDateTime(ToDate), group));
            Apply(load);
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

    private static string Compliance(double? percent, out string state)
    {
        state = percent switch
        {
            null => string.Empty,
            >= GoodCompliance => Good,
            >= WarnCompliance => Warn,
            _ => Bad,
        };

        return percent is { } value ? value.ToString("0.#", System.Globalization.CultureInfo.CurrentCulture) + "%" : NoValue;
    }

    partial void OnSelectedGroupChanged(HelpdeskChoice? value)
    {
        if (value is not null)
        {
            _ = LoadAsync();
        }
    }

    private void Apply(HelpdeskLoad<HelpdeskMetrics>? load)
    {
        Rows.Clear();
        HasData = load is not null;
        IsOffline = load?.FromCache ?? !HasData;

        if (load is null)
        {
            TotalText = string.Empty;
            LastSyncedText = _localization["Helpdesk_NeverSynced"];
            StatusMessage = _localization["Helpdesk_OfflineCache"];

            return;
        }

        var culture = _localization.CurrentCulture;
        LastSyncedText = string.Format(culture, _localization["Helpdesk_LastSynced"], load.SyncedAtUtc.ToLocalTime().ToString(DateTimeFormat, culture));
        StatusMessage = load.FromCache ? _localization["Helpdesk_OfflineCache"] : string.Empty;
        TotalText = load.Data.Groups.Count == 0
            ? _localization["SlaMonitor_NoData"]
            : string.Format(culture, _localization["SlaMonitor_Total"], load.Data.TotalTickets.ToString(culture));

        foreach (var group in load.Data.Groups)
        {
            var firstResponse = Compliance(group.FirstResponseCompliancePercent, out var firstResponseState);
            var resolution = Compliance(group.ResolutionCompliancePercent, out var resolutionState);
            Rows.Add(new SlaMetricRow(
                group.Key.Length == 0 ? NoValue : group.Key,
                group.Tickets.ToString(culture),
                Duration(group.MttfrMinutes),
                Duration(group.MttrMinutes),
                firstResponse,
                firstResponseState,
                resolution,
                resolutionState,
                CsatText(group.Csat)));
        }
    }

    private string Duration(double? minutes)
    {
        if (minutes is not { } value)
        {
            return NoValue;
        }

        var culture = _localization.CurrentCulture;
        var total = (int)Math.Round(value, MidpointRounding.AwayFromZero);

        return total < MinutesPerHour
            ? string.Format(culture, _localization["SlaMonitor_Duration_Minutes"], total.ToString(culture))
            : string.Format(culture, _localization["SlaMonitor_Duration_Hours"], (total / MinutesPerHour).ToString(culture), (total % MinutesPerHour).ToString(culture));
    }

    private string CsatText(HelpdeskCsat csat) =>
        csat.Average is { } average && csat.Responses > 0
            ? string.Format(_localization.CurrentCulture, _localization["SlaMonitor_Csat"], average.ToString("0.0", _localization.CurrentCulture), csat.Responses.ToString(_localization.CurrentCulture))
            : NoValue;
}
