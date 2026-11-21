// <copyright file="GanttTaskRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A row of the Gantt chart: a production order (summary) or one of its scheduled operations.
/// </summary>
internal sealed class GanttTaskRow
{
    /// <summary>
    /// Gets or sets the task name shown in the grid and on the bar.
    /// </summary>
    public string TaskName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start instant.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the finish instant.
    /// </summary>
    public DateTime FinishDate { get; set; }

    /// <summary>
    /// Gets or sets the progress in percent (0 to 100).
    /// </summary>
    public double Progress { get; set; }

    /// <summary>
    /// Gets the operations of an order row.
    /// </summary>
    public ObservableCollection<GanttTaskRow> Children { get; } = [];
}
