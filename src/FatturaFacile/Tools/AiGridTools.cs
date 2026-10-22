using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using DevExpress.AIIntegration;
using DevExpress.AIIntegration.Extensions;
using DevExpress.Data;
using DevExpress.Data.Filtering;
using DevExpress.DataAccess.ExpressionEditor;
using DevExpress.Xpf.Grid;

namespace AIDemo.Tools {
    public static class AIGridTools {

        [AIIntegrationTool]
        [Description("Retrieve information about the columns in the specified Grid.")]
        public static object[] RetrieveColumnsInfo(
            [AIIntegrationToolTarget("Target Grid from which to retrieve column information.")]
            GridControl gridControl) {
            return gridControl.Columns.Where(c => c.Visible).Select(c => new {
                Caption = c.HeaderCaption,
                FieldName = c.FieldName,
                Description = AutomationProperties.GetHelpText(c),
            }).ToArray();
        }

        [AIIntegrationTool]
        [Description("Sort the Grid by a specified column in ascending or descending order. Returns a confirmation message or an actionable error.")]
        public static string SortByColumn(
            [AIIntegrationToolTarget("Target Grid to sort.")]
            GridControl gridControl,
            [Description("The name of the column to sort by. Use the column caption or field name.")]
            string columnName,
            [Description("Sort direction: true for ascending, false for descending.")]
            bool isAscending
        ) {
            var column = FindColumn(gridControl, columnName);
            if(column == null)
                return $"Error: Column '{columnName}' not found. Please check the column name and try again.";

            gridControl.SortBy(column, isAscending ? ColumnSortOrder.Ascending : ColumnSortOrder.Descending);
            return $"Sorting applied to '{column.FieldName}' in {(isAscending ? "ascending" : "descending")} order.";
        }

        [AIIntegrationTool]
        [Description("Remove sorting from the specified Grid column. Returns a confirmation message or an actionable error.")]
        public static string RemoveColumnSorting(
            [AIIntegrationToolTarget("Target Grid to remove sorting from.")]
            GridControl gridControl,
            [Description("The name of the column to remove sorting from. Use the column caption or field name.")]
            string columnName) {

            var column = FindColumn(gridControl, columnName);
            if(column == null)
                return $"Error: Column '{columnName}' not found. Please check the column name and try again.";

            column.SortOrder = ColumnSortOrder.None;
            return $"Sorting removed from column '{column.FieldName}'.";
        }

        [AIIntegrationTool]
        [Description("Groups the Grid data by the specified column. Optionally specify the grouping order. Example: 'Group the grid by Country.' Returns a confirmation or actionable error.")]
        public static string GroupByColumn(
            [AIIntegrationToolTarget("Target Grid to group.")]
            GridControl gridControl,
            [Description("The name of the column to group by. Use the column caption or field name.")]
            string columnName,
            [Description("The grouping order index (optional, default is 0). Lower index means higher grouping priority.")]
            int groupIndex = 0
        ) {
            var column = FindColumn(gridControl, columnName);
            if(column == null)
                return $"Error: Column '{columnName}' not found. Please check the column name and try again.";

            gridControl.GroupBy(column, groupIndex);
            return $"Grouping applied successfully on column '{columnName}' at index {groupIndex}.";
        }

        [AIIntegrationTool]
        [Description("Removes all grouping from the Grid. Returns a confirmation message.")]
        public static string ClearGrouping(
            [AIIntegrationToolTarget("Target Grid to clear grouping from.")]
            GridControl gridControl
        ) {
            gridControl.ClearGrouping();
            return "All grouping cleared from the grid view.";
        }

        [AIIntegrationTool]
        [Description("Create and apply a grid filter from a natural-language prompt. Converts the prompt to a filter expression and applies it. Returns a summary of the filter applied or actionable error.")]
        public static async Task<string> SetFilter(
            [AIIntegrationToolTarget("Target Grid to apply the filter to.")]
            GridControl gridControl,
            [Description("Natural-language description of the filter. Examples: \"Show orders from 2024\", \"price > 100 and status = 'Paid'\".")]
            string userPrompt
        ) {
            if(string.IsNullOrWhiteSpace(userPrompt))
                return "Error: filter prompt is empty.";

            var functions = ExpressionEditorContextHelper.GetFunctions().ToRequestFunctionInfo();
            var columns = gridControl.Columns
                .Select(c => new PromptToFilterRequest.ColumnInfo(c.FieldName, (string)c.HeaderCaption, AutomationProperties.GetHelpText(c) ?? (string)c.HeaderCaption, c.FieldType, new List<PromptToFilterRequest.ColumnInfo>()))
                .ToList();
            var request = new PromptToFilterRequest(userPrompt, string.Empty, columns, functions);
            var result = await AIExtensionsContainerDesktop.Default.PromptToExpressionAsync(request);

            if(string.IsNullOrEmpty(result.Response))
                return "Error: failed to generate a filter expression.";

            try {
                CriteriaOperator.Parse(result.Response);
            }
            catch(Exception ex) {
                return $"Error: generated filter expression '{result.Response}' is invalid. {ex.Message}";
            }

            gridControl.FilterString = result.Response;
            return $"Filter applied: {result.Response})";
        }

        [AIIntegrationTool]
        [Description("Clears all filters from the Grid. Returns a confirmation message.")]
        public static string ClearFilter(
            [AIIntegrationToolTarget("Target Grid to clear filters from.")]
            GridControl gridControl
        ) {
            gridControl.FilterString = string.Empty;
            return "All filters cleared from the grid view.";
        }

        static GridColumn FindColumn(GridControl gridControl, string columnName) {
            return gridControl.Columns.FirstOrDefault(c =>
                string.Equals(c.Name, columnName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.FieldName, columnName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals((string)c.HeaderCaption, columnName, StringComparison.OrdinalIgnoreCase));
        }

        static IList<PromptToFilterRequest.ColumnInfo> GetColumnInfo(GridColumnCollection columns) => columns
            .Select(c => new PromptToFilterRequest.ColumnInfo(
                c.FieldName,
                (string)c.HeaderCaption,
                (string)c.HeaderCaption,
                c.FieldType,
                new List<PromptToFilterRequest.ColumnInfo>()))
            .ToList();
    }
}
