// <copyright file="DssSimulationRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One line of the what-if result: a figure before and after the scenario.
/// </summary>
/// <param name="Title">The localized name of the figure.</param>
/// <param name="Baseline">The formatted figure before the scenario.</param>
/// <param name="Simulated">The formatted figure with the scenario applied.</param>
/// <param name="Delta">The formatted difference.</param>
/// <param name="DeltaState">Good, Bad or Neutral, for the colour of the difference.</param>
internal sealed record DssSimulationRow(string Title, string Baseline, string Simulated, string Delta, string DeltaState);
