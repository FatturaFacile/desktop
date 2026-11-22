// <copyright file="FieldServiceMapRouteRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The route of a technician on the dispatcher map.
/// </summary>
/// <param name="TechnicianName">The technician name.</param>
/// <param name="Color">The color of the polyline and of the pins.</param>
/// <param name="PathData">The polyline as path geometry text (empty for a single stop).</param>
/// <param name="SummaryText">The localized stops, distance and time estimate.</param>
internal sealed record FieldServiceMapRouteRow(string TechnicianName, string Color, string PathData, string SummaryText);
