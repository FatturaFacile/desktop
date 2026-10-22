using FatturaFacile.ViewModels;
using FatturaFacile.Views;

namespace FatturaFacile.Infrastructure;

public static class AppModules
{
    public static ModuleInfo[] Modules => [
    new ("Data Grid", typeof(DataGridView), typeof(DataGridViewModel)),
    new ("Edit Form", typeof(EditFormView), typeof(EditFormViewModel)),
    new ("Alt Edit Form", typeof(AltEditFormView), typeof(AltEditFormViewModel)),
    new ("Side Detail", typeof(GridSideDetailView), typeof(GridSideDetailViewModel)),
    new ("Master Detail", typeof(MasterDetailGridView), typeof(MasterDetailGridViewModel)),
    new ("Tree List", typeof(TreeListBasicView), typeof(TreeListBasicViewModel)),
    new ("Property Grid", typeof(PropertyGridBasicView), typeof(PropertyGridBasicViewModel)),
    new ("Scheduler", typeof(SchedulerCalendarView), typeof(SchedulerCalendarViewModel)),
    new ("Rich Editor", typeof(RichEditView), typeof(RichEditViewModel)),
    new ("Spreadsheet", typeof(SpreadsheetView), typeof(SpreadsheetViewModel)),
    new ("PDF Viewer", typeof(PdfViewerView), typeof(PdfViewerViewModel)),
    new ("Blazor Hybrid", typeof(BlazorHybridView), typeof(BlazorHybridViewModel)),
    new ("Pie Chart", typeof(PieChartView), typeof(PieChartViewModel)),
    new ("Donut Chart", typeof(DonutChartView), typeof(DonutChartViewModel)),
    new ("Bar Chart", typeof(BarChartView), typeof(BarChartViewModel)),
    new ("Stacked Bar Chart", typeof(StackedBarChartView), typeof(StackedBarChartViewModel)),
    new ("Area Chart", typeof(AreaChartView), typeof(AreaChartViewModel)),
    new ("Area Stacked Chart", typeof(AreaStackedChartView), typeof(AreaStackedChartViewModel)),
    new ("Diagram Control", typeof(DiagramControlView), typeof(DiagramControlViewModel)),
    new ("Gantt View", typeof(GanttBasicView), typeof(GanttBasicViewModel)),
    new ("Kanban View", typeof(KanbanView), typeof(KanbanViewModel)),
    new ("Dashboard", typeof(DashboardView), typeof(DashboardViewModel)),
    new ("Report", typeof(ReportView), typeof(ReportViewModel)),
    new ("Map", typeof(MapView), typeof(MapViewModel)),
    new ("Pivot", typeof(PivotView), typeof(PivotViewModel)),
        ];
}
