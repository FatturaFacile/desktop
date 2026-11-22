// <copyright file="CapacityHeatmapViewModel.cs" company="Fattura Facile">
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
/// Drives the capacity heatmap: one row per employee, one cell per week or day, coloured green below 80 percent, yellow up to 100
/// percent and red above (overload), with the hours behind each load in the tooltip.
/// </summary>
internal sealed partial class CapacityHeatmapViewModel : ObservableObject
{
    private const int WeeksShown = 8;
    private const int DaysShown = 14;
    private const int DaysPerWeek = 7;
    private const string PeriodHeaderFormat = "dd.MM";
    private const string NoValue = "-";

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly ResourcePlanningFormatter _format;

    [ObservableProperty]
    private DateTime _fromDate;

    [ObservableProperty]
    private PlanningChoice? _selectedGranularity;

    [ObservableProperty]
    private PlanningChoice? _selectedDepartment;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasData;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _summaryText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="CapacityHeatmapViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="time">The clock.</param>
    public CapacityHeatmapViewModel(ISender sender, ILocalizationService localization, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(time);

        _sender = sender;
        _localization = localization;
        _format = new ResourcePlanningFormatter(localization);
        var today = DateOnly.FromDateTime(time.GetLocalNow().Date);
        _fromDate = MondayOf(today).ToDateTime(TimeOnly.MinValue);
        GranularityChoices =
        [
            new PlanningChoice(nameof(HeatmapGranularity.Week), _localization["ResourcePlanning_Granularity_Week"]),
            new PlanningChoice(nameof(HeatmapGranularity.Day), _localization["ResourcePlanning_Granularity_Day"]),
        ];
        _selectedGranularity = GranularityChoices[0];
        DepartmentChoices = [new PlanningChoice(string.Empty, _localization["ResourcePlanning_AllDepartments"])];
        _selectedDepartment = DepartmentChoices[0];
    }

    /// <summary>
    /// Gets the cell sizes.
    /// </summary>
    public IReadOnlyList<PlanningChoice> GranularityChoices { get; }

    /// <summary>
    /// Gets the departments the heatmap can be limited to, the first entry being all of them.
    /// </summary>
    public ObservableCollection<PlanningChoice> DepartmentChoices { get; }

    /// <summary>
    /// Gets the first day of each column.
    /// </summary>
    public ObservableCollection<string> PeriodHeaders { get; } = [];

    /// <summary>
    /// Gets the employee rows.
    /// </summary>
    public ObservableCollection<HeatmapRowItem> Rows { get; } = [];

    /// <summary>
    /// Gets the Monday of the week of a day.
    /// </summary>
    /// <param name="day">The day.</param>
    /// <returns>The Monday on or before the day.</returns>
    public static DateOnly MondayOf(DateOnly day) => day.AddDays(-(((int)day.DayOfWeek + DaysPerWeek - 1) % DaysPerWeek));

    /// <summary>
    /// Loads the heatmap of the selected window, granularity and department.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            await LoadDepartmentsAsync();
            var granularity = SelectedGranularity?.Key == nameof(HeatmapGranularity.Day) ? HeatmapGranularity.Day : HeatmapGranularity.Week;
            var from = DateOnly.FromDateTime(FromDate);
            var to = from.AddDays(granularity == HeatmapGranularity.Day ? DaysShown - 1 : (WeeksShown * DaysPerWeek) - 1);
            var department = Guid.TryParse(SelectedDepartment?.Key, out var departmentId) ? departmentId : (Guid?)null;
            var heatmap = await _sender.Send(new GetCapacityHeatmapQuery(from, to, granularity, department));
            Apply(heatmap);
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

    private async Task LoadDepartmentsAsync()
    {
        if (DepartmentChoices.Count > 1)
        {
            return;
        }

        var resources = await _sender.Send(new GetPlanningResourcesQuery(null));
        foreach (var department in resources.Where(resource => resource.DepartmentId is not null).GroupBy(resource => resource.DepartmentId!.Value).Select(group => (Id: group.Key, Name: ResourcePlanningFormatter.OrDash(group.First().DepartmentName))).OrderBy(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase))
        {
            DepartmentChoices.Add(new PlanningChoice(department.Id.ToString("D", CultureInfo.InvariantCulture), department.Name));
        }
    }

    private void Apply(CapacityHeatmap heatmap)
    {
        PeriodHeaders.Clear();
        Rows.Clear();
        StatusMessage = string.Empty;

        var culture = _format.Culture;
        foreach (var period in heatmap.Periods)
        {
            PeriodHeaders.Add(period.ToString(PeriodHeaderFormat, culture));
        }

        var overloadedCells = 0;
        foreach (var row in heatmap.Rows)
        {
            var cells = new List<HeatmapCellItem>(row.Cells.Count);
            foreach (var cell in row.Cells)
            {
                var band = CapacityBands.Classify(cell, heatmap.Thresholds);
                var overloaded = band == CapacityBand.Red;
                overloadedCells += overloaded ? 1 : 0;
                cells.Add(new HeatmapCellItem(band == CapacityBand.None ? NoValue : _format.Percent(cell.LoadPercent), band.ToString(), overloaded, ToolTipOf(cell)));
            }

            Rows.Add(new HeatmapRowItem(row.EmployeeName, ResourcePlanningFormatter.OrDash(row.DepartmentName), cells, cells.Exists(cell => cell.IsOverloaded)));
        }

        HasData = Rows.Count > 0;
        SummaryText = HasData
            ? string.Format(culture, _localization["ResourcePlanning_Heatmap_Summary"], Rows.Count.ToString(culture), overloadedCells.ToString(culture), _format.Period(heatmap.From, heatmap.To))
            : _localization["ResourcePlanning_Heatmap_Empty"];
    }

    private string ToolTipOf(HeatmapCell cell) =>
        string.Join(
            Environment.NewLine,
            _format.Day(cell.PeriodStart),
            string.Format(_format.Culture, _localization["ResourcePlanning_Tip_Net"], _format.Hours(cell.NetAvailableHours), _format.Hours(cell.ContractualHours), _format.Hours(cell.LeaveAndHolidayHours)),
            string.Format(_format.Culture, _localization["ResourcePlanning_Tip_Booked"], _format.Hours(cell.HardBookedHours), _format.Hours(cell.SoftBookedHours)),
            string.Format(_format.Culture, _localization["ResourcePlanning_Tip_Billable"], _format.Hours(cell.BillableBookedHours), _format.Hours(cell.InternalBookedHours)));
}
