// <copyright file="OeeLossLine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing;

/// <summary>
/// One of the six big losses of an OEE summary.
/// </summary>
/// <param name="Category">The loss category (equipment_failure, setup_adjustment, minor_stops, reduced_speed, process_defects, startup_rejects).</param>
/// <param name="Minutes">The lost minutes.</param>
/// <param name="Factor">The OEE factor the loss weighs on (availability, performance or quality).</param>
public sealed record OeeLossLine(string Category, double Minutes, string Factor);
