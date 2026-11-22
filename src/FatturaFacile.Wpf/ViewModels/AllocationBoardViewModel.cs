// <copyright file="AllocationBoardViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.ResourcePlanning;
using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The allocation board: a weekly heatmap of the employees whose cells are the places an allocation of the list is dragged to,
/// to move it to another week, to give it to a colleague, or both. The load of each week shows what the move would land on.
/// </summary>
internal sealed partial class AllocationBoardViewModel : ObservableObject
{
    private const int WeeksShown = 8;
    private const int DaysPerWeek = 7;
    private const string PeriodHeaderFormat = "dd.MM";
    private const string NoValue = "-";

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly ResourcePlanningFormatter _format;

    [ObservableProperty]
    private bool _hasData;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllocationBoardViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    public AllocationBoardViewModel(ISender sender, ILocalizationService localization)
    {
        _sender = sender;
        _localization = localization;
        _format = new ResourcePlanningFormatter(localization);
    }

    /// <summary>
    /// Gets the first day of each week shown.
    /// </summary>
    public ObservableCollection<string> PeriodHeaders { get; } = [];

    /// <summary>
    /// Gets the employee rows.
    /// </summary>
    public ObservableCollection<BoardRow> Rows { get; } = [];

    /// <summary>
    /// Loads the weekly load of the employees from the week of a day.
    /// </summary>
    /// <param name="from">A day of the first week.</param>
    /// <returns>A task representing the load.</returns>
    public async Task LoadAsync(DateOnly from)
    {
        try
        {
            var start = CapacityHeatmapViewModel.MondayOf(from);
            var end = start.AddDays((WeeksShown * DaysPerWeek) - 1);
            var heatmap = await _sender.Send(new GetCapacityHeatmapQuery(start, end, HeatmapGranularity.Week, null));
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
    }

    private void Apply(CapacityHeatmap heatmap)
    {
        PeriodHeaders.Clear();
        Rows.Clear();
        StatusMessage = string.Empty;

        foreach (var period in heatmap.Periods)
        {
            PeriodHeaders.Add(period.ToString(PeriodHeaderFormat, _format.Culture));
        }

        foreach (var row in heatmap.Rows)
        {
            var cells = new List<BoardCell>(row.Cells.Count);
            foreach (var cell in row.Cells)
            {
                var band = CapacityBands.Classify(cell, heatmap.Thresholds);
                var load = band == CapacityBand.None ? NoValue : _format.Percent(cell.LoadPercent);
                cells.Add(new BoardCell(row.EmployeeId, cell.PeriodStart, load, band.ToString(), band == CapacityBand.Red, row.EmployeeName + " - " + _format.Day(cell.PeriodStart) + ": " + load));
            }

            Rows.Add(new BoardRow(row.EmployeeName, cells));
        }

        HasData = Rows.Count > 0;
    }
}
