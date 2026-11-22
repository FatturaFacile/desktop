// <copyright file="FieldServiceMapViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.FieldService;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the dispatcher map: the pins and the nearest-neighbour routes drawn from the board, the labels, the sites without
/// coordinates, and the selection of a pin that selects the work order on the board.
/// </summary>
public sealed class FieldServiceMapViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly DateTimeOffset Monday = new(2027, 3, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTime SyncedAt = new(2027, 3, 1, 6, 0, 0, DateTimeKind.Utc);

    private readonly ILocalizationService loc = LocalizationService.Instance;
    private readonly Mock<ISender> sender = new();

    /// <summary>
    /// A pin is drawn for every work order with a site, unassigned ones in their own color, inside the canvas.
    /// </summary>
    [Fact]
    public void Show_DrawsAPinPerSite()
    {
        var map = new FieldServiceMapViewModel(new FieldServiceLabels(loc));

        map.Show(Board([Block("q", 46.0, 9.0)], Lane("Anna", Block("a", 46.1, 9.1, 8), Block("b", 46.2, 9.2, 10))), selectedWorkOrderId: null);

        Assert.True(map.HasPins);
        Assert.False(map.HasMissing);
        Assert.Equal(3, map.Pins.Count);
        var queued = Assert.Single(map.Pins, pin => pin.IsUnassigned);
        Assert.Equal("WO-q", queued.Number);
        Assert.Equal(map.UnassignedBrush, queued.Color);
        Assert.All(map.Pins, pin =>
        {
            Assert.InRange(pin.Left, 0.0, FieldServiceMapViewModel.CanvasWidth - FieldServiceMapViewModel.PinSize);
            Assert.InRange(pin.Top, 0.0, FieldServiceMapViewModel.CanvasHeight - FieldServiceMapViewModel.PinSize);
        });
    }

    /// <summary>
    /// A technician route starts at the earliest visit and goes to the nearest stop next, with the estimate in the summary.
    /// </summary>
    [Fact]
    public void Show_DrawsTheNearestNeighbourRouteWithItsEstimate()
    {
        var map = new FieldServiceMapViewModel(new FieldServiceLabels(loc));
        var first = Block("1", 46.00, 9.0, startHour: 8);
        var far = Block("2", 46.50, 9.0, startHour: 9);
        var near = Block("3", 46.05, 9.0, startHour: 10);

        map.Show(Board([], Lane("Anna", far, near, first)), selectedWorkOrderId: null);

        var route = Assert.Single(map.Routes);
        Assert.Equal("Anna", route.TechnicianName);
        Assert.StartsWith("M ", route.PathData, StringComparison.Ordinal);
        Assert.Equal(2, route.PathData.Split(" L ").Length - 1);
        Assert.Equal(["WO-1", "WO-3", "WO-2"], map.Pins.Select(pin => pin.Number), StringComparer.Ordinal);
        Assert.All(map.Pins, pin => Assert.Equal(route.Color, pin.Color));

        var planned = FieldServiceRoutePlanner.Plan(
            [new FieldServiceRouteStop("1", "WO-1", new FieldServiceGeoPoint(46.00, 9.0)), new FieldServiceRouteStop("3", "WO-3", new FieldServiceGeoPoint(46.05, 9.0)), new FieldServiceRouteStop("2", "WO-2", new FieldServiceGeoPoint(46.50, 9.0))]);
        var culture = loc.CurrentCulture;
        var expected = string.Format(
            culture,
            loc["FieldService_Map_RouteLine"],
            "Anna",
            "3",
            ((decimal)Math.Round(planned.RoadKm, 1, MidpointRounding.AwayFromZero)).ToString("0.##", culture),
            Math.Round(planned.TravelMinutes, MidpointRounding.AwayFromZero).ToString("0", culture));
        Assert.Equal(expected, route.SummaryText);
    }

    /// <summary>
    /// A technician has a color of his own, and a lane with a single site has a pin and a summary but no line.
    /// </summary>
    [Fact]
    public void Show_GivesEachTechnicianAColorAndNoLineForASingleStop()
    {
        var map = new FieldServiceMapViewModel(new FieldServiceLabels(loc));

        map.Show(Board([], Lane("Anna", Block("a", 46.0, 9.0, 8), Block("b", 46.1, 9.0, 9)), Lane("Luca", Block("c", 46.3, 9.3, 8))), selectedWorkOrderId: null);

        Assert.Equal(2, map.Routes.Count);
        Assert.NotEqual(map.Routes[0].Color, map.Routes[1].Color, StringComparer.Ordinal);
        Assert.NotEmpty(map.Routes[0].PathData);
        Assert.Empty(map.Routes[1].PathData);
        Assert.Contains("0", map.Routes[1].SummaryText, StringComparison.Ordinal);
    }

    /// <summary>
    /// Work orders without coordinates are counted and left off the map.
    /// </summary>
    [Fact]
    public void Show_CountsTheSitesWithoutCoordinates()
    {
        var map = new FieldServiceMapViewModel(new FieldServiceLabels(loc));

        map.Show(Board([Block("q", latitude: null, longitude: null)], Lane("Anna", Block("a", 46.1, 9.1, 8), Block("b", 46.2, longitude: null, startHour: 9))), selectedWorkOrderId: null);

        _ = Assert.Single(map.Pins);
        Assert.True(map.HasMissing);
        Assert.Equal(string.Format(loc.CurrentCulture, loc["FieldService_Map_NoCoordinates"], "2"), map.MissingText);
    }

    /// <summary>
    /// Impossible coordinates are not drawn, and a board without any site shows the empty state.
    /// </summary>
    [Fact]
    public void Show_WithoutSites_HasNoPins()
    {
        var map = new FieldServiceMapViewModel(new FieldServiceLabels(loc));
        map.Show(Board([Block("q", 46.0, 9.0)]), selectedWorkOrderId: null);
        Assert.True(map.HasPins);

        map.Show(Board([Block("q", 123.0, 9.0), Block("r", 46.0, 200.0)]), selectedWorkOrderId: null);

        Assert.False(map.HasPins);
        Assert.Empty(map.Pins);
        Assert.Empty(map.Routes);
        Assert.True(map.HasMissing);
    }

    /// <summary>
    /// A single site is centered on the map.
    /// </summary>
    [Fact]
    public void Show_CentersASingleSite()
    {
        var map = new FieldServiceMapViewModel(new FieldServiceLabels(loc));

        map.Show(Board([Block("q", 46.0, 9.0)]), selectedWorkOrderId: null);

        var pin = Assert.Single(map.Pins);
        Assert.Equal((FieldServiceMapViewModel.CanvasWidth - FieldServiceMapViewModel.PinSize) / 2, pin.Left, precision: 6);
        Assert.Equal((FieldServiceMapViewModel.CanvasHeight - FieldServiceMapViewModel.PinSize) / 2, pin.Top, precision: 6);
    }

    /// <summary>
    /// Selecting a pin highlights it, tells which work order it is and raises the selection once.
    /// </summary>
    [Fact]
    public void SelectPin_HighlightsAndRaisesTheSelection()
    {
        var map = new FieldServiceMapViewModel(new FieldServiceLabels(loc));
        map.Show(Board([Block("q", 46.0, 9.0)], Lane("Anna", Block("a", 46.1, 9.1, 8))), selectedWorkOrderId: null);
        List<string> raised = [];
        map.WorkOrderSelected += (_, e) => raised.Add(e.WorkOrderId);

        map.SelectPinCommand.Execute(map.Pins.Single(pin => string.Equals(pin.Number, "WO-a", StringComparison.Ordinal)));

        Assert.Equal(["a"], raised, StringComparer.Ordinal);
        Assert.Equal(["WO-a"], map.Pins.Where(pin => pin.IsSelected).Select(pin => pin.Number), StringComparer.Ordinal);
        Assert.Equal(string.Format(loc.CurrentCulture, loc["FieldService_Map_Selected"], "WO-a", "Title a"), map.SelectedText);

        map.SelectPinCommand.Execute(parameter: null);
        _ = Assert.Single(raised);
    }

    /// <summary>
    /// Highlighting from the board marks the pin without raising the selection, and null clears it.
    /// </summary>
    [Fact]
    public void Highlight_MarksThePinWithoutRaising()
    {
        var map = new FieldServiceMapViewModel(new FieldServiceLabels(loc));
        map.Show(Board([Block("q", 46.0, 9.0)]), selectedWorkOrderId: "q");
        var raised = false;
        map.WorkOrderSelected += (_, _) => raised = true;

        Assert.True(map.Pins[0].IsSelected);

        map.Highlight(workOrderId: null);

        Assert.False(map.Pins[0].IsSelected);
        Assert.Empty(map.SelectedText);
        Assert.False(raised);
    }

    /// <summary>
    /// The board shows the map of the cached board, and selecting the pin of an unassigned work order selects it in the queue.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Board_PinOfAnUnassignedOrder_SelectsItInTheQueue()
    {
        var vm = await LoadedBoardAsync(Board([Block("q", 46.0, 9.0), Block("r", 46.1, 9.1)], Lane("Anna", Block("a", 46.2, 9.2, 8))));

        Assert.Equal(3, vm.Map.Pins.Count);

        vm.Map.SelectPinCommand.Execute(vm.Map.Pins.Single(pin => string.Equals(pin.Number, "WO-r", StringComparison.Ordinal)));

        Assert.Equal("WO-r", vm.SelectedUnassigned?.Number);
        Assert.Equal(["WO-r"], vm.Map.Pins.Where(pin => pin.IsSelected).Select(pin => pin.Number), StringComparer.Ordinal);
    }

    /// <summary>
    /// Selecting the pin of a scheduled work order selects the lane of its technician.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Board_PinOfAScheduledOrder_SelectsTheTechnicianLane()
    {
        var vm = await LoadedBoardAsync(Board([Block("q", 46.0, 9.0)], Lane("Anna", Block("a", 46.2, 9.2, 8)), Lane("Luca", Block("b", 46.3, 9.3, 8))));

        vm.Map.SelectPinCommand.Execute(vm.Map.Pins.Single(pin => string.Equals(pin.Number, "WO-b", StringComparison.Ordinal)));

        Assert.Equal("Luca", vm.SelectedLane?.Lane.Name);
    }

    /// <summary>
    /// Selecting a work order in the queue highlights its pin, and the map follows the board when it is reloaded.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Board_QueueSelection_HighlightsThePinAndSurvivesAReload()
    {
        var vm = await LoadedBoardAsync(Board([Block("q", 46.0, 9.0)], Lane("Anna", Block("a", 46.2, 9.2, 8))));

        vm.SelectedUnassigned = vm.Unassigned[0];
        Assert.Equal(["WO-q"], vm.Map.Pins.Where(pin => pin.IsSelected).Select(pin => pin.Number), StringComparer.Ordinal);

        await vm.SyncAsync();

        Assert.Equal(2, vm.Map.Pins.Count);
        Assert.Equal(["WO-q"], vm.Map.Pins.Where(pin => pin.IsSelected).Select(pin => pin.Number), StringComparer.Ordinal);
    }

    /// <summary>
    /// The labels of the map exist in every language, with the same placeholders.
    /// </summary>
    [Fact]
    public void Labels_ExistInEveryLanguage()
    {
        string[] keys =
        [
            "FieldService_Map_Title",
            "FieldService_Map_Hint",
            "FieldService_Map_Empty",
            "FieldService_Map_NoCoordinates",
            "FieldService_Map_RouteLine",
            "FieldService_Map_Unassigned",
            "FieldService_Map_Selected",
            "FieldService_Map_Disclaimer",
            "FieldService_Signature_Pad",
            "FieldService_Signature_Clear",
            "FieldService_Signature_Undo",
            "FieldService_Signature_Hash",
            "FieldService_Signature_Strokes",
            "FieldService_Signature_Empty",
            "FieldService_Err_SignerLength",
        ];
        foreach (var key in keys)
        {
            var text = loc[key];
            Assert.False(string.IsNullOrWhiteSpace(text), key);
            Assert.NotEqual(key, text, StringComparer.Ordinal);
        }

        Assert.Contains("{3}", loc["FieldService_Map_RouteLine"], StringComparison.Ordinal);
        Assert.Contains("{0}", loc["FieldService_Signature_Hash"], StringComparison.Ordinal);
    }

    private static FieldServiceBlock Block(string id, double? latitude, double? longitude, int? startHour = null) => new(
        id,
        "WO-" + id,
        "Title " + id,
        "corrective",
        "normal",
        "dispatched",
        startHour is { } hour ? Monday.AddHours(hour) : null,
        startHour is { } end ? Monday.AddHours(end + 1) : null,
        Address: null,
        latitude,
        longitude,
        [],
        0);

    private static FieldServiceBoard Board(FieldServiceBlock[] unassigned, params FieldServiceLane[] lanes) =>
        new(Monday, Monday.AddDays(7), unassigned, lanes);

    private static FieldServiceLane Lane(string name, params FieldServiceBlock[] blocks) =>
        new("tech-" + name, name, IsAvailable: true, [], VanReady: true, VanProducts: 3, blocks);

    private async Task<FieldServiceBoardViewModel> LoadedBoardAsync(FieldServiceBoard board)
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetDispatcherGanttAndMapBoardQuery>(), Any)).ReturnsAsync(new FieldServiceCached<FieldServiceBoard>(board, SyncedAt));
        _ = sender.Setup(s => s.Send(It.IsAny<SyncFieldServiceCommand>(), Any)).ReturnsAsync(new FieldServiceCallResult(FieldServiceCallStatus.Ok));
        FieldServiceBoardViewModel vm = new(sender.Object, loc);

        await vm.LoadAsync();

        return vm;
    }
}
