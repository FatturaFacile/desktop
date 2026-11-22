// <copyright file="FieldServiceMapViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.FieldService;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Drives the dispatcher map: a pin per work order site, a polyline per technician in nearest-neighbour order with the distance
/// and the time estimate, and the selection of a pin that selects the work order on the board. It draws on a plain canvas from
/// the coordinates of the board, so it needs no map library, tiles or network.
/// </summary>
internal sealed partial class FieldServiceMapViewModel : ObservableObject
{
    /// <summary>
    /// The width of the map canvas, in device-independent pixels.
    /// </summary>
    public const double CanvasWidth = 640.0;

    /// <summary>
    /// The height of the map canvas, in device-independent pixels.
    /// </summary>
    public const double CanvasHeight = 380.0;

    /// <summary>
    /// The diameter of a pin, in device-independent pixels.
    /// </summary>
    public const double PinSize = 16.0;

    private const double Padding = 28.0;
    private const string UnassignedColor = "#D29922";
    private const string PointSeparator = " ";

    private static readonly string[] Palette = ["#58A6FF", "#3FB950", "#BC8CFF", "#F778BA", "#79C0FF", "#FFA657"];

    private readonly FieldServiceLabels _labels;

    [ObservableProperty]
    private bool _hasPins;

    [ObservableProperty]
    private bool _hasMissing;

    [ObservableProperty]
    private string _missingText = string.Empty;

    [ObservableProperty]
    private string _selectedText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldServiceMapViewModel"/> class.
    /// </summary>
    /// <param name="labels">The labels of the module.</param>
    public FieldServiceMapViewModel(FieldServiceLabels labels)
    {
        ArgumentNullException.ThrowIfNull(labels);

        _labels = labels;
    }

    /// <summary>
    /// Occurs when the dispatcher selects a pin; the argument is the identifier of the work order.
    /// </summary>
    public event EventHandler<FieldServiceWorkOrderSelectedEventArgs>? WorkOrderSelected;

    /// <summary>
    /// Gets the width of the map canvas.
    /// </summary>
    public double MapWidth { get; } = CanvasWidth;

    /// <summary>
    /// Gets the height of the map canvas.
    /// </summary>
    public double MapHeight { get; } = CanvasHeight;

    /// <summary>
    /// Gets the color of the pins of the unassigned work orders.
    /// </summary>
    public string UnassignedBrush { get; } = UnassignedColor;

    /// <summary>
    /// Gets the pins.
    /// </summary>
    public ObservableCollection<FieldServiceMapPinRow> Pins { get; } = [];

    /// <summary>
    /// Gets the routes of the technicians.
    /// </summary>
    public ObservableCollection<FieldServiceMapRouteRow> Routes { get; } = [];

    /// <summary>
    /// Selects a pin, which selects the work order on the board.
    /// </summary>
    /// <param name="pin">The pin.</param>
    [RelayCommand]
    public void SelectPin(FieldServiceMapPinRow? pin)
    {
        if (pin is null)
        {
            return;
        }

        Highlight(pin.WorkOrderId);
        WorkOrderSelected?.Invoke(this, new FieldServiceWorkOrderSelectedEventArgs(pin.WorkOrderId));
    }

    /// <summary>
    /// Marks the pin of a work order as selected, without raising the selection event.
    /// </summary>
    /// <param name="workOrderId">The work order identifier, or null to clear the selection.</param>
    public void Highlight(string? workOrderId)
    {
        FieldServiceMapPinRow? selected = null;

        foreach (var pin in Pins)
        {
            pin.IsSelected = workOrderId is not null && string.Equals(pin.WorkOrderId, workOrderId, StringComparison.Ordinal);
            if (pin.IsSelected)
            {
                selected = pin;
            }
        }

        SelectedText = selected is null ? string.Empty : _labels.Format("FieldService_Map_Selected", selected.Number, selected.Title);
    }

    /// <summary>
    /// Empties the map.
    /// </summary>
    public void Clear()
    {
        Pins.Clear();
        Routes.Clear();
        HasPins = false;
        HasMissing = false;
        MissingText = string.Empty;
        SelectedText = string.Empty;
    }

    /// <summary>
    /// Draws the board: the unassigned work orders and the work orders of every lane that have coordinates.
    /// </summary>
    /// <param name="board">The board.</param>
    /// <param name="selectedWorkOrderId">The work order selected on the board, if any.</param>
    public void Show(FieldServiceBoard board, string? selectedWorkOrderId)
    {
        ArgumentNullException.ThrowIfNull(board);

        Clear();

        var unassigned = board.Unassigned.Where(HasSite).ToList();
        var lanes = board.Lanes
            .Select(lane => (Lane: lane, Blocks: lane.Blocks.Where(HasSite).OrderBy(block => block.Start ?? DateTimeOffset.MaxValue).ToList()))
            .ToList();

        var missing = board.Unassigned.Count - unassigned.Count + lanes.Sum(entry => entry.Lane.Blocks.Count - entry.Blocks.Count);
        HasMissing = missing > 0;
        MissingText = missing > 0 ? _labels.Format("FieldService_Map_NoCoordinates", missing.ToString(_labels.Culture)) : string.Empty;

        var located = unassigned.Concat(lanes.SelectMany(entry => entry.Blocks)).ToList();
        HasPins = located.Count > 0;

        if (located.Count == 0)
        {
            return;
        }

        var canvas = FieldServiceMapProjector.Project(located.ConvertAll(PointOf), CanvasWidth, CanvasHeight, Padding);
        var canvasOf = new Dictionary<string, FieldServiceCanvasPoint>(StringComparer.Ordinal);
        for (var index = 0; index < located.Count; index++)
        {
            canvasOf[located[index].WorkOrderId] = canvas[index];
        }

        foreach (var order in unassigned)
        {
            Pins.Add(PinOf(order, canvasOf[order.WorkOrderId], UnassignedColor, order.Number + " " + order.Title + Environment.NewLine + _labels.Key("FieldService_Map_Unassigned"), isUnassigned: true));
        }

        for (var laneIndex = 0; laneIndex < lanes.Count; laneIndex++)
        {
            AddLane(lanes[laneIndex].Lane, lanes[laneIndex].Blocks, Palette[laneIndex % Palette.Length], canvasOf);
        }

        Highlight(selectedWorkOrderId);
    }

    private static bool HasSite(FieldServiceBlock block) =>
        block.Latitude is >= -90 and <= 90 && block.Longitude is >= -180 and <= 180;

    private static FieldServiceGeoPoint PointOf(FieldServiceBlock block) => new(block.Latitude!.Value, block.Longitude!.Value);

    private static string Coordinate(double value) => value.ToString("0.#", CultureInfo.InvariantCulture);

    private static string PathOf(IEnumerable<FieldServiceCanvasPoint> points) =>
        "M " + string.Join(" L ", points.Select(point => Coordinate(point.X) + PointSeparator + Coordinate(point.Y)));

    private static FieldServiceMapPinRow PinOf(FieldServiceBlock order, FieldServiceCanvasPoint at, string color, string tooltip, bool isUnassigned) =>
        new(order.WorkOrderId, order.Number, order.Title, new FieldServiceMapPinPosition(at.X - (PinSize / 2), at.Y - (PinSize / 2)), color, tooltip, isUnassigned);

    private void AddLane(FieldServiceLane lane, List<FieldServiceBlock> blocks, string color, Dictionary<string, FieldServiceCanvasPoint> canvasOf)
    {
        if (blocks.Count == 0)
        {
            return;
        }

        var route = FieldServiceRoutePlanner.Plan(blocks.ConvertAll(block => new FieldServiceRouteStop(block.WorkOrderId, block.Number, PointOf(block))));
        var byId = blocks.ToDictionary(block => block.WorkOrderId, StringComparer.Ordinal);

        foreach (var stop in route.Stops)
        {
            var block = byId[stop.WorkOrderId];
            var tooltip = block.Number + " " + block.Title + Environment.NewLine + lane.Name + Environment.NewLine + _labels.Window(block.Start, block.End);
            Pins.Add(PinOf(block, canvasOf[stop.WorkOrderId], color, tooltip, isUnassigned: false));
        }

        var summary = _labels.Format(
            "FieldService_Map_RouteLine",
            lane.Name,
            route.Stops.Count.ToString(_labels.Culture),
            _labels.Number((decimal)Math.Round(route.RoadKm, 1, MidpointRounding.AwayFromZero)),
            Math.Round(route.TravelMinutes, MidpointRounding.AwayFromZero).ToString("0", _labels.Culture));
        var path = route.Stops.Count > 1 ? PathOf(route.Stops.Select(stop => canvasOf[stop.WorkOrderId])) : string.Empty;

        Routes.Add(new FieldServiceMapRouteRow(lane.Name, color, path, summary));
    }
}
