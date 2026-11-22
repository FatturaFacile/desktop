// <copyright file="FieldServiceRoutePlanner.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Plans the route of a technician over the sites of the work orders without any map service: the first stop is the one the
/// caller passes first (the earliest scheduled), each following stop is the nearest unvisited one (nearest neighbour), and the
/// travel is estimated from the great-circle (haversine) distance, a road factor and an average speed.
/// </summary>
public static class FieldServiceRoutePlanner
{
    /// <summary>
    /// The average driving speed, in kilometres per hour.
    /// </summary>
    public const double AverageSpeedKmh = 50.0;

    /// <summary>
    /// The factor that turns a straight line distance into a road distance.
    /// </summary>
    public const double RoadFactor = 1.3;

    private const double EarthRadiusKm = 6371.0088;
    private const double DegreesToRadians = Math.PI / 180.0;
    private const double MinutesPerHour = 60.0;

    /// <summary>
    /// Computes the great-circle distance between two positions with the haversine formula.
    /// </summary>
    /// <param name="from">The first position.</param>
    /// <param name="to">The second position.</param>
    /// <returns>The distance in kilometres.</returns>
    public static double HaversineKm(FieldServiceGeoPoint from, FieldServiceGeoPoint to)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        var lat1 = from.Latitude * DegreesToRadians;
        var lat2 = to.Latitude * DegreesToRadians;
        var deltaLat = lat2 - lat1;
        var deltaLng = (to.Longitude - from.Longitude) * DegreesToRadians;
        var a = (Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2)) + (Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(deltaLng / 2) * Math.Sin(deltaLng / 2));

        return 2 * EarthRadiusKm * Math.Asin(Math.Min(1.0, Math.Sqrt(a)));
    }

    /// <summary>
    /// Plans the route over the stops.
    /// </summary>
    /// <param name="stops">The stops, the first one being where the route starts.</param>
    /// <returns>The route; with no or one stop the distance and the time are zero.</returns>
    public static FieldServiceRoute Plan(IReadOnlyList<FieldServiceRouteStop> stops)
    {
        ArgumentNullException.ThrowIfNull(stops);

        if (stops.Count == 0)
        {
            return new FieldServiceRoute([], 0, 0, 0);
        }

        List<FieldServiceRouteStop> remaining = [.. stops];
        List<FieldServiceRouteStop> ordered = [remaining[0]];
        remaining.RemoveAt(0);
        var straight = 0.0;

        while (remaining.Count > 0)
        {
            var current = ordered[^1];
            var nearest = 0;
            var nearestKm = double.MaxValue;

            for (var index = 0; index < remaining.Count; index++)
            {
                var km = HaversineKm(current.Point, remaining[index].Point);
                if (km < nearestKm)
                {
                    nearestKm = km;
                    nearest = index;
                }
            }

            straight += nearestKm;
            ordered.Add(remaining[nearest]);
            remaining.RemoveAt(nearest);
        }

        var road = straight * RoadFactor;

        return new FieldServiceRoute(ordered, straight, road, road / AverageSpeedKmh * MinutesPerHour);
    }
}
