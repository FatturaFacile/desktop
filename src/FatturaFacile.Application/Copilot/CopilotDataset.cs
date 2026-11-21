// <copyright file="CopilotDataset.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Copilot;

/// <summary>
/// A numeric series of a chart visualization.
/// </summary>
/// <param name="Name">The series name (metric).</param>
/// <param name="Data">The values, aligned with the chart labels.</param>
public sealed record CopilotDataset(string Name, IReadOnlyList<double> Data);
