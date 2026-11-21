// <copyright file="CopilotVisualization.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Copilot;

/// <summary>
/// A structured result the assistant wants rendered as a chart, KPI card or table.
/// </summary>
/// <param name="Type">"bar", "line", "pie", "kpi_card" or "table".</param>
/// <param name="Title">The title.</param>
/// <param name="Columns">The column names of the summary table.</param>
/// <param name="Rows">The rows of the summary table; each row maps column name to the formatted value.</param>
/// <param name="X">The dimension plotted on the x axis.</param>
/// <param name="Labels">The category labels of a chart.</param>
/// <param name="Datasets">The series of a chart.</param>
/// <param name="Metric">The metric name of a KPI card.</param>
/// <param name="Value">The value of a KPI card.</param>
public sealed record CopilotVisualization(
    string Type,
    string Title,
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyDictionary<string, string>> Rows,
    string? X,
    IReadOnlyList<string> Labels,
    IReadOnlyList<CopilotDataset> Datasets,
    string? Metric,
    string? Value)
{
    /// <summary>
    /// Gets a value indicating whether the visualization is drawn with a chart control.
    /// </summary>
    public bool IsChart => string.Equals(Type, "bar", StringComparison.Ordinal) || string.Equals(Type, "line", StringComparison.Ordinal) || string.Equals(Type, "pie", StringComparison.Ordinal);

    /// <summary>
    /// Gets a value indicating whether the visualization is a KPI card.
    /// </summary>
    public bool IsKpi => string.Equals(Type, "kpi_card", StringComparison.Ordinal);

    /// <summary>
    /// Gets a value indicating whether the visualization is a table (also the fallback for unknown types).
    /// </summary>
    public bool IsTable => !IsChart && !IsKpi;

    /// <summary>
    /// Builds the series of the chart as label/value points, one per dataset.
    /// </summary>
    /// <returns>The series.</returns>
    public IReadOnlyList<CopilotChartSeries> GetSeries() =>
        [.. Datasets.Select(d => new CopilotChartSeries(d.Name, [.. d.Data.Select((v, i) => new CopilotDataPoint(i < Labels.Count ? Labels[i] : string.Empty, v))]))];
}
