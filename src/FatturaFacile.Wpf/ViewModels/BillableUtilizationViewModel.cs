// <copyright file="BillableUtilizationViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
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
/// Drives the billable utilization report: the committed (hard) share and the forecast (hard and soft) share of the net availability
/// of each resource, department or cost center against its target.
/// </summary>
internal sealed partial class BillableUtilizationViewModel : ObservableObject
{
    private const double BarMax = 100d;

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly ResourcePlanningFormatter _format;

    [ObservableProperty]
    private DateTime _fromDate;

    [ObservableProperty]
    private DateTime _toDate;

    [ObservableProperty]
    private PlanningChoice? _selectedGroup;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasData;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _totalsText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="BillableUtilizationViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="time">The clock.</param>
    public BillableUtilizationViewModel(ISender sender, ILocalizationService localization, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(time);

        _sender = sender;
        _localization = localization;
        _format = new ResourcePlanningFormatter(localization);
        var today = time.GetLocalNow().Date;
        _fromDate = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        _toDate = _fromDate.AddMonths(1).AddDays(-1);
        GroupChoices =
        [
            new PlanningChoice(nameof(UtilizationGroupBy.Resource), _localization["ResourcePlanning_GroupBy_Resource"]),
            new PlanningChoice(nameof(UtilizationGroupBy.Department), _localization["ResourcePlanning_GroupBy_Department"]),
            new PlanningChoice(nameof(UtilizationGroupBy.CostCenter), _localization["ResourcePlanning_GroupBy_CostCenter"]),
        ];
        _selectedGroup = GroupChoices[0];
    }

    /// <summary>
    /// Gets the groupings of the report.
    /// </summary>
    public IReadOnlyList<PlanningChoice> GroupChoices { get; }

    /// <summary>
    /// Gets the rows of the report.
    /// </summary>
    public ObservableCollection<UtilizationRowItem> Rows { get; } = [];

    /// <summary>
    /// Loads the report of the selected period and grouping.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            var groupBy = Enum.TryParse<UtilizationGroupBy>(SelectedGroup?.Key, out var parsed) ? parsed : UtilizationGroupBy.Resource;
            var report = await _sender.Send(new GetBillableUtilizationQuery(DateOnly.FromDateTime(FromDate), DateOnly.FromDateTime(ToDate), groupBy));
            Apply(report);
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

    private static double Bar(double percent) => Math.Clamp(percent, 0d, BarMax);

    private void Apply(UtilizationReport report)
    {
        Rows.Clear();
        StatusMessage = string.Empty;

        foreach (var row in report.Rows)
        {
            Rows.Add(ToRow(row));
        }

        HasData = Rows.Count > 0;
        TotalsText = HasData
            ? string.Format(
                _format.Culture,
                _localization["ResourcePlanning_Utilization_Totals"],
                _format.Percent(report.Totals.ActualBillablePercent),
                _format.Percent(report.Totals.ForecastBillablePercent),
                _format.Percent(report.Totals.TargetPercent),
                _format.Percent(report.DefaultTargetPercent))
            : _localization["ResourcePlanning_Utilization_Empty"];
    }

    private UtilizationRowItem ToRow(UtilizationRow row)
    {
        var gap = (row.GapPercent > 0d ? "+" : string.Empty) + _format.Percent(row.GapPercent);

        return new UtilizationRowItem(
            ResourcePlanningFormatter.OrDash(row.KeyName),
            _format.Hours(row.NetAvailableHours),
            _format.Percent(row.ActualBillablePercent),
            _format.Percent(row.ForecastBillablePercent),
            _format.Percent(row.TargetPercent),
            gap,
            row.Status.ToString(),
            _localization["ResourcePlanning_Status_" + row.Status],
            Bar(row.ActualBillablePercent),
            Bar(row.TargetPercent));
    }
}
