// <copyright file="FieldServiceRoutePlannerTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.FieldService;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the haversine distance, the nearest-neighbour route with its travel estimate and the canvas projection of the map.
/// </summary>
public sealed class FieldServiceRoutePlannerTests
{
    private static FieldServiceRouteStop Stop(string id, double latitude, double longitude) => new(id, "WO-" + id, new FieldServiceGeoPoint(latitude, longitude));

    /// <summary>
    /// One degree of latitude is the earth radius times pi over 180.
    /// </summary>
    [Fact]
    public void Haversine_OneDegreeOfLatitude_IsAbout111Km()
    {
        Assert.Equal(111.195, FieldServiceRoutePlanner.HaversineKm(new FieldServiceGeoPoint(0, 0), new FieldServiceGeoPoint(1, 0)), precision: 1);
    }

    /// <summary>
    /// The distance between two real sites is plausible, symmetric and zero between equal points.
    /// </summary>
    [Fact]
    public void Haversine_LuganoToZurich_IsPlausibleAndSymmetric()
    {
        var lugano = new FieldServiceGeoPoint(46.0037, 8.9511);
        var zurich = new FieldServiceGeoPoint(47.3769, 8.5417);

        var km = FieldServiceRoutePlanner.HaversineKm(lugano, zurich);

        Assert.InRange(km, 155.0, 157.0);
        Assert.Equal(km, FieldServiceRoutePlanner.HaversineKm(zurich, lugano), precision: 9);
        Assert.Equal(0.0, FieldServiceRoutePlanner.HaversineKm(lugano, lugano), precision: 9);
    }

    /// <summary>
    /// Antipodal points do not overflow the arcsine.
    /// </summary>
    [Fact]
    public void Haversine_AntipodalPoints_IsHalfTheCircumference()
    {
        var km = FieldServiceRoutePlanner.HaversineKm(new FieldServiceGeoPoint(0, 0), new FieldServiceGeoPoint(0, 180));

        Assert.Equal(Math.PI * 6371.0088, km, precision: 3);
    }

    /// <summary>
    /// The route starts at the first stop and goes to the nearest unvisited stop each time.
    /// </summary>
    [Fact]
    public void Plan_VisitsTheNearestStopNext()
    {
        var route = FieldServiceRoutePlanner.Plan([Stop("a", 0, 0), Stop("b", 0, 3), Stop("c", 0, 1), Stop("d", 0, 2)]);

        Assert.Equal(["a", "c", "d", "b"], route.Stops.Select(stop => stop.WorkOrderId));
        Assert.Equal(3 * 111.195, route.StraightLineKm, precision: 1);
    }

    /// <summary>
    /// The road distance is the straight line distance times 1.3 and the time is that distance at 50 km/h.
    /// </summary>
    [Fact]
    public void Plan_EstimatesTheRoadDistanceAndTheTime()
    {
        var route = FieldServiceRoutePlanner.Plan([Stop("a", 0, 0), Stop("b", 0, 1)]);

        Assert.Equal(1.3, FieldServiceRoutePlanner.RoadFactor);
        Assert.Equal(50.0, FieldServiceRoutePlanner.AverageSpeedKmh);
        Assert.Equal(route.StraightLineKm * 1.3, route.RoadKm, precision: 9);
        Assert.Equal(route.RoadKm / 50.0 * 60.0, route.TravelMinutes, precision: 9);
        Assert.Equal(173.46, route.TravelMinutes, precision: 1);
    }

    /// <summary>
    /// With no stop, or a single one, there is nothing to drive.
    /// </summary>
    [Fact]
    public void Plan_WithZeroOrOneStop_HasNoDistance()
    {
        var none = FieldServiceRoutePlanner.Plan([]);
        var one = FieldServiceRoutePlanner.Plan([Stop("a", 46, 9)]);

        Assert.Empty(none.Stops);
        Assert.Equal(0.0, none.TravelMinutes);
        Assert.Single(one.Stops);
        Assert.Equal(0.0, one.RoadKm);
        Assert.Equal(0.0, one.TravelMinutes);
    }

    /// <summary>
    /// The planner does not change the list it is given and keeps every stop once, even at the same position.
    /// </summary>
    [Fact]
    public void Plan_KeepsEveryStopOnce()
    {
        List<FieldServiceRouteStop> stops = [Stop("a", 46, 9), Stop("b", 46, 9), Stop("c", 46.1, 9)];

        var route = FieldServiceRoutePlanner.Plan(stops);

        Assert.Equal(["a", "b", "c"], stops.Select(stop => stop.WorkOrderId));
        Assert.Equal(["a", "b", "c"], route.Stops.Select(stop => stop.WorkOrderId));
    }

    /// <summary>
    /// The projection puts north at the top, east at the right, keeps the margin and centers the drawing.
    /// </summary>
    [Fact]
    public void Project_FitsNorthUpInsideThePadding()
    {
        var points = FieldServiceMapProjector.Project([new FieldServiceGeoPoint(46, 8), new FieldServiceGeoPoint(47, 9)], 640, 380, 20);

        Assert.Equal(2, points.Count);
        Assert.Equal(360.0, points[0].Y, precision: 6);
        Assert.Equal(20.0, points[1].Y, precision: 6);
        Assert.True(points[0].X < points[1].X);
        Assert.Equal(640 - points[1].X, points[0].X, precision: 6);
        Assert.All(points, point =>
        {
            Assert.InRange(point.X, 20.0, 620.0);
            Assert.InRange(point.Y, 20.0, 360.0);
        });
    }

    /// <summary>
    /// A single position, or equal ones, is centered; a line of sites on one parallel is centered vertically.
    /// </summary>
    [Fact]
    public void Project_CentersDegenerateSets()
    {
        var single = FieldServiceMapProjector.Project([new FieldServiceGeoPoint(46, 9)], 640, 380, 20);
        var equal = FieldServiceMapProjector.Project([new FieldServiceGeoPoint(46, 9), new FieldServiceGeoPoint(46, 9)], 640, 380, 20);
        var line = FieldServiceMapProjector.Project([new FieldServiceGeoPoint(46, 8), new FieldServiceGeoPoint(46, 9)], 640, 380, 20);

        Assert.Equal(320.0, single[0].X, precision: 6);
        Assert.Equal(190.0, single[0].Y, precision: 6);
        Assert.Equal(single[0], equal[0]);
        Assert.Equal(single[0], equal[1]);
        Assert.Equal(190.0, line[0].Y, precision: 6);
        Assert.Equal(20.0, line[0].X, precision: 6);
        Assert.Equal(620.0, line[1].X, precision: 6);
    }

    /// <summary>
    /// No positions project to nothing.
    /// </summary>
    [Fact]
    public void Project_WithNoPoints_IsEmpty()
    {
        Assert.Empty(FieldServiceMapProjector.Project([], 640, 380, 20));
    }
}
