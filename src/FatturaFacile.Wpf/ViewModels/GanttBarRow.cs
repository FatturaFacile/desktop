// <copyright file="GanttBarRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One line of the Gantt diagram: the bar of a node placed on the day scale.
/// </summary>
/// <param name="Label">The code and title of the node.</param>
/// <param name="Offset">The left margin of the bar, in device-independent pixels.</param>
/// <param name="Width">The width of the bar.</param>
/// <param name="ProgressWidth">The width of the completed part of the bar.</param>
/// <param name="IsCritical">Whether the node is on the critical path.</param>
/// <param name="IsMilestone">Whether the node is a milestone (drawn as a diamond).</param>
/// <param name="IsSummary">Whether the node is a summary bar.</param>
/// <param name="Tooltip">The dates and float of the node.</param>
internal sealed record GanttBarRow(string Label, Thickness Offset, double Width, double ProgressWidth, bool IsCritical, bool IsMilestone, bool IsSummary, string Tooltip);
