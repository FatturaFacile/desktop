// <copyright file="CopilotChartBehavior.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using FatturaFacile.Application.Copilot;
using Syncfusion.UI.Xaml.Charts;

namespace FatturaFacile.Wpf.Behaviors;

/// <summary>
/// Draws a Copilot visualization (bar, line or pie) on a Syncfusion <see cref="SfChart"/> without code-behind:
/// bind the Visualization attached property to the payload.
/// </summary>
[SuppressMessage("Design", "MA0182:Avoid unused internal types", Justification = "Used from XAML.")]
internal static class CopilotChartBehavior
{
    /// <summary>
    /// Identifies the Visualization attached property.
    /// </summary>
    public static readonly DependencyProperty VisualizationProperty = DependencyProperty.RegisterAttached(
        "Visualization",
        typeof(CopilotVisualization),
        typeof(CopilotChartBehavior),
        new PropertyMetadata(defaultValue: null, OnVisualizationChanged));

    /// <summary>
    /// Gets the visualization.
    /// </summary>
    /// <param name="target">The chart.</param>
    /// <returns>The visualization.</returns>
    public static CopilotVisualization? GetVisualization(DependencyObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return (CopilotVisualization?)target.GetValue(VisualizationProperty);
    }

    /// <summary>
    /// Sets the visualization.
    /// </summary>
    /// <param name="target">The chart.</param>
    /// <param name="value">The visualization.</param>
    public static void SetVisualization(DependencyObject target, CopilotVisualization? value)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.SetValue(VisualizationProperty, value);
    }

    private static void OnVisualizationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not SfChart chart)
        {
            return;
        }

        chart.Series.Clear();

        if (e.NewValue is not CopilotVisualization { IsChart: true } visualization)
        {
            return;
        }

        var series = visualization.GetSeries();

        if (string.Equals(visualization.Type, "pie", StringComparison.Ordinal))
        {
            chart.Legend = new ChartLegend();
            chart.Series.Add(new PieSeries
            {
                ItemsSource = series.Count > 0 ? series[0].Points : null,
                XBindingPath = nameof(CopilotDataPoint.Label),
                YBindingPath = nameof(CopilotDataPoint.Value),
                ShowTooltip = true,
            });
            return;
        }

        chart.PrimaryAxis = new CategoryAxis { Header = visualization.X };
        chart.SecondaryAxis = new NumericalAxis();
        chart.Legend = series.Count > 1 ? new ChartLegend() : null;

        foreach (var item in series)
        {
            XyDataSeries xy = string.Equals(visualization.Type, "line", StringComparison.Ordinal) ? new LineSeries() : new ColumnSeries();
            xy.ItemsSource = item.Points;
            xy.XBindingPath = nameof(CopilotDataPoint.Label);
            xy.YBindingPath = nameof(CopilotDataPoint.Value);
            xy.Label = item.Name;
            xy.ShowTooltip = true;
            chart.Series.Add(xy);
        }
    }
}
