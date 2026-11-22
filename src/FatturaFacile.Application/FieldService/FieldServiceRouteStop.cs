// <copyright file="FieldServiceRouteStop.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// A visit of a route.
/// </summary>
/// <param name="WorkOrderId">The work order identifier.</param>
/// <param name="Number">The work order number.</param>
/// <param name="Point">The site position.</param>
public sealed record FieldServiceRouteStop(string WorkOrderId, string Number, FieldServiceGeoPoint Point);
