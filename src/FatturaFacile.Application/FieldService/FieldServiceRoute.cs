// <copyright file="FieldServiceRoute.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// The visiting order of a technician with the travel estimate.
/// </summary>
/// <param name="Stops">The stops in visiting order.</param>
/// <param name="StraightLineKm">The sum of the great-circle distances between the stops.</param>
/// <param name="RoadKm">The estimated road distance: the straight line distance times the road factor.</param>
/// <param name="TravelMinutes">The estimated driving time at the average speed.</param>
public sealed record FieldServiceRoute(IReadOnlyList<FieldServiceRouteStop> Stops, double StraightLineKm, double RoadKm, double TravelMinutes);
