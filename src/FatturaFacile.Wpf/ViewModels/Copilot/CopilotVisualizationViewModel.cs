// <copyright file="CopilotVisualizationViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using FatturaFacile.Application.Copilot;

namespace FatturaFacile.Wpf.ViewModels.Copilot;

/// <summary>
/// Presentation model of a "visualization" payload: a Syncfusion chart (bar, line, pie), a KPI card or a data grid.
/// </summary>
internal sealed class CopilotVisualizationViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CopilotVisualizationViewModel"/> class.
    /// </summary>
    /// <param name="visualization">The payload.</param>
    public CopilotVisualizationViewModel(CopilotVisualization visualization)
    {
        ArgumentNullException.ThrowIfNull(visualization);

        Model = visualization;
        Table = BuildTable(visualization);
    }

    /// <summary>
    /// Gets the payload.
    /// </summary>
    public CopilotVisualization Model { get; }

    /// <summary>
    /// Gets the summary table (also shown below a chart as the source of the numbers).
    /// </summary>
    public DataView Table { get; }

    /// <summary>
    /// Gets the title.
    /// </summary>
    public string Title => Model.Title;

    /// <summary>
    /// Gets a value indicating whether the payload is drawn as a chart.
    /// </summary>
    public bool IsChart => Model.IsChart;

    /// <summary>
    /// Gets a value indicating whether the payload is a KPI card.
    /// </summary>
    public bool IsKpi => Model.IsKpi;

    /// <summary>
    /// Gets a value indicating whether the payload is a table.
    /// </summary>
    public bool IsTable => Model.IsTable;

    /// <summary>
    /// Gets the metric name of a KPI card.
    /// </summary>
    public string Metric => Model.Metric ?? string.Empty;

    /// <summary>
    /// Gets the value of a KPI card.
    /// </summary>
    public string Value => Model.Value ?? string.Empty;

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP004:Don't ignore created IDisposable", Justification = "The table is owned by the returned view for the lifetime of the message.")]
    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created", Justification = "The table is owned by the returned view for the lifetime of the message.")]
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The table is owned by the returned view for the lifetime of the message.")]
    private static DataView BuildTable(CopilotVisualization visualization)
    {
        var columns = visualization.Columns.Count > 0
            ? visualization.Columns
            : [.. visualization.Rows.SelectMany(static r => r.Keys).Distinct(StringComparer.Ordinal)];

        DataTable table = new() { Locale = CultureInfo.InvariantCulture };

        foreach (var column in columns)
        {
            _ = table.Columns.Add(column, typeof(string));
        }

        foreach (var row in visualization.Rows)
        {
            object?[] values = [.. columns.Select(c => row.TryGetValue(c, out var value) ? value : string.Empty)];
            _ = table.LoadDataRow(values, fAcceptChanges: true);
        }

        return table.DefaultView;
    }
}
