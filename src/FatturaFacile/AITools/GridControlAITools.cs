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
using DevExpress.Xpf.Grid;
using AIIntegrationMethods = DevExpress.AIIntegration.AIIntegration;

namespace DevExpress.DevAV.AiTools {
    public static class GridControlAITools {
        [AIIntegrationTool]
        [Description("Retrieves information about columns in the specified GridControl.")]
        public static object[] GetColumnInfo(
            [AIIntegrationToolTarget("The GridControl instance from which the method retrieves column information.")]
            GridControl gridControl) {
            return gridControl.Columns.Where(c => c.Visible).Select(c => new {
                Name = c.FieldName,
                Description = AutomationProperties.GetHelpText(c),
            }).ToArray();
        }
        [AIIntegrationTool]
        [Description("Sorts the grid by the specified column. Example: 'Sort the grid by Country in ascending order.'. Returns a confirmation message or an actionable error.")]
        public static object SortByColumn(
            [AIIntegrationToolTarget("The GridControl instance to be sorted.")]
            GridControl gridControl,
            [Description("The column name to be sorted by.")]
            string columnName,
            [Description("True for ascending order. False for descending order.")]
            bool isAscending) {
            var column = FindColumn(gridControl, columnName);
            if(column == null)
                throw new ArgumentException($"Error: Column '{columnName}' not found. Please check the column name and try again.");

            gridControl.SortBy(column, isAscending ? ColumnSortOrder.Ascending : ColumnSortOrder.Descending);
            return $"Sorting applied successfully to column '{columnName}' in {(isAscending ? "ascending" : "descending")} order.";
        }

        [AIIntegrationTool]
        [Description("Groups grid data by the specified column. Optionally, the method specifies the grouping order. Example: 'Group the grid by Country.' Returns a confirmation or actionable error.")]
        public static object GroupByColumn(
            [AIIntegrationToolTarget("The GridControl instance to be grouped.")]
            GridControl gridControl,
            [Description("The column name to be grouped by.")]
            string columnName,
            [Description("The grouping order index (optional, default is 0). Lower index corresponds to higher grouping priority.")]
            int groupIndex = 0) {
            var column = FindColumn(gridControl, columnName);
            if(column == null)
                throw new ArgumentException($"Error: Column '{columnName}' not found. Please check the column name and try again.");

            gridControl.GroupBy(column, groupIndex);
            return $"Grouping applied successfully on column '{columnName}' at index {groupIndex}.";
        }
        [AIIntegrationTool]
        [Description("Removes all grouping from the grid view. Returns a confirmation message.")]
        public static string ClearGrouping(
            [AIIntegrationToolTarget("The GridControl instance in which the grouping from to be cleared.")]
            GridControl gridControl) {
            gridControl.ClearGrouping();
            return "All grouping cleared from the grid view.";
        }
        [AIIntegrationTool]
        [Description("Finds rows that contain query text strings in the specified or all columns.")]
        public static object SearchRows(
            [AIIntegrationToolTarget("The GridControl instance to which the search rules to be applied to.")]
            GridControl gridControl,
            [Description("Text strings to be searched for (case-insensitive).")]
            string query) {
            gridControl.View.SearchString = query;
            return "Search applied successfully.";
        }

        static GridColumn FindColumn(GridControl gridControl, string columnName) {
            return gridControl.Columns.FirstOrDefault(c =>
                string.Equals(c.Name, columnName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.FieldName, columnName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals((string)c.HeaderCaption, columnName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
