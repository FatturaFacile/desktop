// <copyright file="CopilotDataPoint.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Copilot;

/// <summary>
/// A label/value pair bound to a chart series.
/// </summary>
/// <param name="Label">The category label.</param>
/// <param name="Value">The numeric value.</param>
public sealed record CopilotDataPoint(string Label, double Value);
