// <copyright file="CopilotChartSeries.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Copilot;

/// <summary>
/// A named series of label/value points ready to bind to a chart control.
/// </summary>
/// <param name="Name">The series name.</param>
/// <param name="Points">The points.</param>
public sealed record CopilotChartSeries(string Name, IReadOnlyList<CopilotDataPoint> Points);
