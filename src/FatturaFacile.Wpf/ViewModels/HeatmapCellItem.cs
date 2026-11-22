// <copyright file="HeatmapCellItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One cell of the capacity heatmap, ready to show.
/// </summary>
/// <param name="LoadText">The load, for example 87.5%.</param>
/// <param name="BandName">The band: None, Green, Yellow or Red.</param>
/// <param name="IsOverloaded">Whether the load is above the red threshold.</param>
/// <param name="ToolTip">The hours behind the load.</param>
internal sealed record HeatmapCellItem(string LoadText, string BandName, bool IsOverloaded, string ToolTip);
