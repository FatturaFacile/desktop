// <copyright file="GridControlAITools.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Windows.Automation;
using DevExpress.AIIntegration;
using DevExpress.Data;
using DevExpress.Xpf.Grid;

namespace FatturaFacile.AiTools;

/// <summary>
/// Provides AI function tools for interacting with and inspecting DevExpress GridControl instances.
/// </summary>
public static class GridControlAITools
{
    /// <summary>
    /// Retrieves visible column metadata and accessibility help text from the specified grid control.
    /// </summary>
    /// <param name="gridControl">The target <see cref="GridControl"/> instance.</param>
    /// <returns>An array of anonymous objects containing the column name and help description.</returns>
    [AIIntegrationTool]
    [Description("Retrieves information about columns in the specified GridControl.")]
    public static object[] GetColumnInfo(
        [AIIntegrationToolTarget("The GridControl instance from which the method retrieves column information.")]
        GridControl gridControl)
    {
        return gridControl.Columns.Where(c => c.Visible).Select(c => new
        {
            Name = c.FieldName,
            Description = AutomationProperties.GetHelpText(c),
        }).ToArray();
    }

    /// <summary>
    /// Sorts the target grid by the specified column in ascending or descending direction.
    /// </summary>
    /// <param name="gridControl">The target <see cref="GridControl"/> instance.</param>
    /// <param name="columnName">The column name to be sorted by.</param>
    /// <param name="isAscending">True for ascending order; false for descending.</param>
    /// <returns>A confirmation message.</returns>
    /// <exception cref="ArgumentException">Thrown when the specified column is not found in the grid.</exception>
    [AIIntegrationTool]
    [Description(
        "Sorts the grid by the specified column. Example: 'Sort the grid by Country in ascending order.'. Returns a confirmation message or an actionable error.")]
    public static object SortByColumn(
        [AIIntegrationToolTarget("The GridControl instance to be sorted.")]
        GridControl gridControl,
        [Description("The column name to be sorted by.")]
        string columnName,
        [Description("True for ascending order. False for descending order.")]
        bool isAscending)
    {
        var column = FindColumn(gridControl, columnName);
        if (column == null)
        {
            throw new ArgumentException(
                $"Error: Column '{columnName}' not found. Please check the column name and try again.");
        }

        gridControl.SortBy(column, isAscending ? ColumnSortOrder.Ascending : ColumnSortOrder.Descending);
        return
            $"Sorting applied successfully to column '{columnName}' in {(isAscending ? "ascending" : "descending")} order.";
    }

    /// <summary>
    /// Groups grid data by the specified column.
    /// </summary>
    /// <param name="gridControl">The target <see cref="GridControl"/> instance.</param>
    /// <param name="columnName">The column name to be grouped by.</param>
    /// <param name="groupIndex">The grouping order index (default 0).</param>
    /// <returns>A confirmation message.</returns>
    /// <exception cref="ArgumentException">Thrown when the specified column is not found in the grid.</exception>
    [AIIntegrationTool]
    [Description(
        "Groups grid data by the specified column. Optionally, the method specifies the grouping order. Example: 'Group the grid by Country.' Returns a confirmation or actionable error.")]
    public static object GroupByColumn(
        [AIIntegrationToolTarget("The GridControl instance to be grouped.")]
        GridControl gridControl,
        [Description("The column name to be grouped by.")]
        string columnName,
        [Description(
            "The grouping order index (optional, default is 0). Lower index corresponds to higher grouping priority.")]
        int groupIndex = 0)
    {
        var column = FindColumn(gridControl, columnName);
        if (column == null)
        {
            throw new ArgumentException(
                $"Error: Column '{columnName}' not found. Please check the column name and try again.");
        }

        gridControl.GroupBy(column, groupIndex);
        return $"Grouping applied successfully on column '{columnName}' at index {groupIndex}.";
    }

    /// <summary>
    /// Removes all grouping levels from the grid view.
    /// </summary>
    /// <param name="gridControl">The target <see cref="GridControl"/> instance.</param>
    /// <returns>A confirmation message.</returns>
    [AIIntegrationTool]
    [Description("Removes all grouping from the grid view. Returns a confirmation message.")]
    public static string ClearGrouping(
        [AIIntegrationToolTarget("The GridControl instance in which the grouping from to be cleared.")]
        GridControl gridControl)
    {
        gridControl.ClearGrouping();
        return "All grouping cleared from the grid view.";
    }

    /// <summary>
    /// Finds rows that contain the query text string across grid columns.
    /// </summary>
    /// <param name="gridControl">The target <see cref="GridControl"/> instance.</param>
    /// <param name="query">Text string to search for.</param>
    /// <returns>A confirmation message.</returns>
    [AIIntegrationTool]
    [Description("Finds rows that contain query text strings in the specified or all columns.")]
    public static object SearchRows(
        [AIIntegrationToolTarget("The GridControl instance to which the search rules to be applied to.")]
        GridControl gridControl,
        [Description("Text strings to be searched for (case-insensitive).")]
        string query)
    {
        gridControl.View.SearchString = query;
        return "Search applied successfully.";
    }

    private static GridColumn FindColumn(
        GridControl gridControl,
        string columnName)
    {
        return gridControl.Columns.FirstOrDefault(c =>
            string.Equals(c.Name, columnName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.FieldName, columnName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals((string)c.HeaderCaption, columnName, StringComparison.OrdinalIgnoreCase));
    }
}
