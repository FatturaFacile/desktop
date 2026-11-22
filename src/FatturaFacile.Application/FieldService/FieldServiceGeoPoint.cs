// <copyright file="FieldServiceGeoPoint.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// A position on the earth.
/// </summary>
/// <param name="Latitude">The latitude in degrees.</param>
/// <param name="Longitude">The longitude in degrees.</param>
public sealed record FieldServiceGeoPoint(double Latitude, double Longitude);
