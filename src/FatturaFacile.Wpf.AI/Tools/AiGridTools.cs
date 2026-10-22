// <copyright file="AiGridTools.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Windows.Automation;
using DevExpress.AIIntegration;
using DevExpress.AIIntegration.Extensions;
using DevExpress.Data;
using DevExpress.Data.Filtering;
using DevExpress.DataAccess.ExpressionEditor;
using DevExpress.Xpf.Grid;

namespace FatturaFacile.Tools;

/// <summary>
/// AI tools providing automated column inspection, filtering, and grouping on DevExpress GridControls.
/// </summary>
public static class AIGridTools
{
    /// <summary>
    /// Retrieves visible column metadata and help descriptions from the specified GridControl.
    /// </summary>
    /// <param name="gridControl">The target <see cref="GridControl"/> instance.</param>
    /// <returns>An array of objects representing column properties.</returns>
    [AIIntegrationTool]
    [Description("Retrieve information about the columns in the specified Grid.")]
    public static object[] RetrieveColumnsInfo(
        [AIIntegrationToolTarget("Target Grid from which to retrieve column information.")]
        GridControl gridControl)
    {
        return gridControl.Columns.Where(c => c.Visible).Select(c => new
        {
            Caption = c.HeaderCaption,
            FieldName = c.FieldName,
            Description = AutomationProperties.GetHelpText(c),
        }).ToArray();
    }

    /// <summary>
    /// Sorts the grid data rows by a designated column in ascending or descending direction.
    /// </summary>
    /// <param name="gridControl">The target <see cref="GridControl"/> instance.</param>
    /// <param name="columnName">The column name or caption to sort by.</param>
    /// <param name="isAscending">True for ascending sorting; false for descending.</param>
    /// <returns>A status message confirming the sorting operation.</returns>
    [AIIntegrationTool]
    [Description(
        "Sort the Grid by a specified column in ascending or descending order. Returns a confirmation message or an actionable error.")]
    public static string SortByColumn(
        [AIIntegrationToolTarget("Target Grid to sort.")]
        GridControl gridControl,
        [Description("The name of the column to sort by. Use the column caption or field name.")]
        string columnName,
        [Description("Sort direction: true for ascending, false for descending.")]
        bool isAscending
    )
    {
        var column = FindColumn(gridControl, columnName);
        if (column == null)
        {
            return $"Error: Column '{columnName}' not found. Please check the column name and try again.";
        }

        gridControl.SortBy(column, isAscending ? ColumnSortOrder.Ascending : ColumnSortOrder.Descending);
        return $"Sorting applied to '{column.FieldName}' in {(isAscending ? "ascending" : "descending")} order.";
    }

    /// <summary>
    /// Removes sorting applied to the designated column in the grid.
    /// </summary>
    /// <param name="gridControl">The target <see cref="GridControl"/> instance.</param>
    /// <param name="columnName">The name or caption of the column whose sort order to clear.</param>
    /// <returns>A status message confirming removal of column sorting.</returns>
    [AIIntegrationTool]
    [Description(
        "Remove sorting from the specified Grid column. Returns a confirmation message or an actionable error.")]
    public static string RemoveColumnSorting(
        [AIIntegrationToolTarget("Target Grid to remove sorting from.")]
        GridControl gridControl,
        [Description("The name of the column to remove sorting from. Use the column caption or field name.")]
        string columnName)
    {
        var column = FindColumn(gridControl, columnName);
        if (column == null)
        {
            return $"Error: Column '{columnName}' not found. Please check the column name and try again.";
        }

        column.SortOrder = ColumnSortOrder.None;
        return $"Sorting removed from column '{column.FieldName}'.";
    }

    /// <summary>
    /// Groups the grid rows by the specified column name.
    /// </summary>
    /// <param name="gridControl">The target <see cref="GridControl"/> instance.</param>
    /// <param name="columnName">The name or caption of the column to group by.</param>
    /// <param name="groupIndex">The grouping hierarchy index (default is 0).</param>
    /// <returns>A status message indicating grouping success.</returns>
    [AIIntegrationTool]
    [Description(
        "Groups the Grid data by the specified column. Optionally specify the grouping order. Example: 'Group the grid by Country.' Returns a confirmation or actionable error.")]
    public static string GroupByColumn(
        [AIIntegrationToolTarget("Target Grid to group.")]
        GridControl gridControl,
        [Description("The name of the column to group by. Use the column caption or field name.")]
        string columnName,
        [Description("The grouping order index (optional, default is 0). Lower index means higher grouping priority.")]
        int groupIndex = 0
    )
    {
        var column = FindColumn(gridControl, columnName);
        if (column == null)
        {
            return $"Error: Column '{columnName}' not found. Please check the column name and try again.";
        }

        gridControl.GroupBy(column, groupIndex);
        return $"Grouping applied successfully on column '{columnName}' at index {groupIndex}.";
    }

    /// <summary>
    /// Removes all active grouping criteria from the grid view.
    /// </summary>
    /// <param name="gridControl">The target <see cref="GridControl"/> instance.</param>
    /// <returns>A confirmation message indicating that grouping has been cleared.</returns>
    [AIIntegrationTool]
    [Description("Removes all grouping from the Grid. Returns a confirmation message.")]
    public static string ClearGrouping(
        [AIIntegrationToolTarget("Target Grid to clear grouping from.")]
        GridControl gridControl
    )
    {
        gridControl.ClearGrouping();
        return "All grouping cleared from the grid view.";
    }

    /// <summary>
    /// Converts a natural language filter request into a DevExpress criteria expression and applies it to the grid.
    /// </summary>
    /// <param name="gridControl">The target <see cref="GridControl"/> instance.</param>
    /// <param name="userPrompt">The natural-language description of the desired filter.</param>
    /// <returns>A confirmation message with the applied filter expression or error details.</returns>
    [AIIntegrationTool]
    [Description(
        "Create and apply a grid filter from a natural-language prompt. Converts the prompt to a filter expression and applies it. Returns a summary of the filter applied or actionable error.")]
    public static async Task<string> SetFilter(
        [AIIntegrationToolTarget("Target Grid to apply the filter to.")]
        GridControl gridControl,
        [Description(
            "Natural-language description of the filter. Examples: \"Show orders from 2024\", \"price > 100 and status = 'Paid'\".")]
        string userPrompt
    )
    {
        if (string.IsNullOrWhiteSpace(userPrompt))
        {
            return "Error: filter prompt is empty.";
        }

        var functions = ExpressionEditorContextHelper.GetFunctions().ToRequestFunctionInfo();
        var columns = gridControl.Columns
            .Select(c => new PromptToFilterRequest.ColumnInfo(
                c.FieldName, (string)c.HeaderCaption, AutomationProperties.GetHelpText(c) ?? (string)c.HeaderCaption,
                c.FieldType, new List<PromptToFilterRequest.ColumnInfo>()))
            .ToList();
        var request = new PromptToFilterRequest(userPrompt, string.Empty, columns, functions);
        var result = await AIExtensionsContainerDesktop.Default.PromptToExpressionAsync(request);

        if (string.IsNullOrEmpty(result.Response))
        {
            return "Error: failed to generate a filter expression.";
        }

        try
        {
            _ = CriteriaOperator.Parse(result.Response);
        }
        catch (Exception ex)
        {
            return $"Error: generated filter expression '{result.Response}' is invalid. {ex.Message}";
        }

        gridControl.FilterString = result.Response;
        return $"Filter applied: {result.Response})";
    }

    /// <summary>
    /// Clears all active filters from the grid view.
    /// </summary>
    /// <param name="gridControl">The target <see cref="GridControl"/> instance.</param>
    /// <returns>A confirmation message indicating that filters were cleared.</returns>
    [AIIntegrationTool]
    [Description("Clears all filters from the Grid. Returns a confirmation message.")]
    public static string ClearFilter(
        [AIIntegrationToolTarget("Target Grid to clear filters from.")]
        GridControl gridControl
    )
    {
        gridControl.FilterString = string.Empty;
        return "All filters cleared from the grid view.";
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

    private static IList<PromptToFilterRequest.ColumnInfo> GetColumnInfo(
        GridColumnCollection columns) => columns
        .Select(c => new PromptToFilterRequest.ColumnInfo(
            c.FieldName,
            (string)c.HeaderCaption,
            (string)c.HeaderCaption,
            c.FieldType,
            new List<PromptToFilterRequest.ColumnInfo>()))
        .ToList();
}
