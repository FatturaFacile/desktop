namespace FatturaFacile;

partial class SampleDashboard
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        DevExpress.DashboardCommon.Dimension dimension1 = new DevExpress.DashboardCommon.Dimension();
        DevExpress.DashboardCommon.Measure measure1 = new DevExpress.DashboardCommon.Measure();
        DevExpress.DashboardCommon.Measure measure2 = new DevExpress.DashboardCommon.Measure();
        DevExpress.DashboardCommon.Measure measure3 = new DevExpress.DashboardCommon.Measure();
        DevExpress.DashboardCommon.ValueMap valueMap1 = new DevExpress.DashboardCommon.ValueMap();
        DevExpress.DashboardCommon.UniformScale uniformScale1 = new DevExpress.DashboardCommon.UniformScale();
        DevExpress.DashboardCommon.DeltaMap deltaMap1 = new DevExpress.DashboardCommon.DeltaMap();
        DevExpress.DashboardCommon.Dimension dimension2 = new DevExpress.DashboardCommon.Dimension();
        DevExpress.DashboardCommon.GridDimensionColumn gridDimensionColumn1 = new DevExpress.DashboardCommon.GridDimensionColumn();
        DevExpress.DashboardCommon.Measure measure4 = new DevExpress.DashboardCommon.Measure();
        DevExpress.DashboardCommon.Measure measure5 = new DevExpress.DashboardCommon.Measure();
        DevExpress.DashboardCommon.GridDeltaColumn gridDeltaColumn1 = new DevExpress.DashboardCommon.GridDeltaColumn();
        DevExpress.DashboardCommon.DashboardLayoutGroup dashboardLayoutGroup1 = new DevExpress.DashboardCommon.DashboardLayoutGroup();
        DevExpress.DashboardCommon.DashboardLayoutItem dashboardLayoutItem1 = new DevExpress.DashboardCommon.DashboardLayoutItem();
        DevExpress.DashboardCommon.DashboardLayoutItem dashboardLayoutItem2 = new DevExpress.DashboardCommon.DashboardLayoutItem();
        this.choroplethMapDashboardItem1 = new DevExpress.DashboardCommon.ChoroplethMapDashboardItem();
        this.gridDashboardItem1 = new DevExpress.DashboardCommon.GridDashboardItem();
        this.dashboardExtractDataSource1 = new DevExpress.DashboardCommon.DashboardExtractDataSource();
        ((System.ComponentModel.ISupportInitialize)(this.choroplethMapDashboardItem1)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(dimension1)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(measure1)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(measure2)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(measure3)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.gridDashboardItem1)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(dimension2)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(measure4)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(measure5)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.dashboardExtractDataSource1)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
        //
        // choroplethMapDashboardItem1
        //
        this.choroplethMapDashboardItem1.Area = DevExpress.DashboardCommon.ShapefileArea.USA;
        dimension1.DataMember = "State";
        this.choroplethMapDashboardItem1.AttributeDimension = dimension1;
        this.choroplethMapDashboardItem1.AttributeName = "NAME";
        this.choroplethMapDashboardItem1.ComponentName = "choroplethMapDashboardItem1";
        measure1.DataMember = "RevenueYTD";
        measure2.DataMember = "RevenueYTD";
        measure3.DataMember = "RevenueYTDTarget";
        this.choroplethMapDashboardItem1.DataItemRepository.Clear();
        this.choroplethMapDashboardItem1.DataItemRepository.Add(dimension1, "DataItem0");
        this.choroplethMapDashboardItem1.DataItemRepository.Add(measure1, "DataItem1");
        this.choroplethMapDashboardItem1.DataItemRepository.Add(measure2, "DataItem2");
        this.choroplethMapDashboardItem1.DataItemRepository.Add(measure3, "DataItem3");
        this.choroplethMapDashboardItem1.DataMember = "";
        this.choroplethMapDashboardItem1.DataSource = this.dashboardExtractDataSource1;
        this.choroplethMapDashboardItem1.InteractivityOptions.IgnoreMasterFilters = false;
        this.choroplethMapDashboardItem1.InteractivityOptions.MasterFilterMode = DevExpress.DashboardCommon.DashboardItemMasterFilterMode.Multiple;
        valueMap1.Name = "Revenue YTD";
        valueMap1.Scale = uniformScale1;
        valueMap1.ValueName = "Revenue";
        valueMap1.AddDataItem("Value", measure1);
        deltaMap1.ActualValueName = "Revenue";
        deltaMap1.DeltaName = "vs Target";
        deltaMap1.Name = "Revenue YTD vs Target";
        deltaMap1.AddDataItem("ActualValue", measure2);
        deltaMap1.AddDataItem("TargetValue", measure3);
        this.choroplethMapDashboardItem1.Maps.AddRange(new DevExpress.DashboardCommon.ChoroplethMap[] {
        valueMap1,
        deltaMap1});
        this.choroplethMapDashboardItem1.Name = "Sales by State";
        this.choroplethMapDashboardItem1.ShapeTitleAttributeName = "ABBREV";
        this.choroplethMapDashboardItem1.ShowCaption = true;
        this.choroplethMapDashboardItem1.Viewport.BottomLatitude = 22.722765208057595D;
        this.choroplethMapDashboardItem1.Viewport.CenterPointLatitude = 37.208247632673633D;
        this.choroplethMapDashboardItem1.Viewport.CenterPointLongitude = -95.848499902568008D;
        this.choroplethMapDashboardItem1.Viewport.LeftLongitude = -124.70997774915153D;
        this.choroplethMapDashboardItem1.Viewport.RightLongitude = -66.987022055984482D;
        this.choroplethMapDashboardItem1.Viewport.TopLatitude = 49.369672064487254D;
        //
        // gridDashboardItem1
        //
        dimension2.DataMember = "Product";
        gridDimensionColumn1.Weight = 73.43096234309624D;
        gridDimensionColumn1.WidthType = DevExpress.DashboardCommon.GridColumnFixedWidthType.Weight;
        gridDimensionColumn1.AddDataItem("Dimension", dimension2);
        measure4.DataMember = "UnitsSoldYTD";
        measure5.DataMember = "UnitsSoldYTDTarget";
        gridDeltaColumn1.Name = "UnitsSold YTD vs Target";
        gridDeltaColumn1.Weight = 76.56903765690376D;
        gridDeltaColumn1.WidthType = DevExpress.DashboardCommon.GridColumnFixedWidthType.Weight;
        gridDeltaColumn1.AddDataItem("ActualValue", measure4);
        gridDeltaColumn1.AddDataItem("TargetValue", measure5);
        this.gridDashboardItem1.Columns.AddRange(new DevExpress.DashboardCommon.GridColumnBase[] {
        gridDimensionColumn1,
        gridDeltaColumn1});
        this.gridDashboardItem1.ComponentName = "gridDashboardItem1";
        this.gridDashboardItem1.DataItemRepository.Clear();
        this.gridDashboardItem1.DataItemRepository.Add(dimension2, "DataItem0");
        this.gridDashboardItem1.DataItemRepository.Add(measure4, "DataItem1");
        this.gridDashboardItem1.DataItemRepository.Add(measure5, "DataItem2");
        this.gridDashboardItem1.DataMember = "";
        this.gridDashboardItem1.DataSource = this.dashboardExtractDataSource1;
        this.gridDashboardItem1.GridOptions.ColumnWidthMode = DevExpress.DashboardCommon.GridColumnWidthMode.Manual;
        this.gridDashboardItem1.InteractivityOptions.IgnoreMasterFilters = false;
        this.gridDashboardItem1.Name = "Product Units Sold";
        this.gridDashboardItem1.ShowCaption = true;
        //
        // dashboardExtractDataSource1
        //
        this.dashboardExtractDataSource1.ComponentName = "dashboardExtractDataSource1";
        this.dashboardExtractDataSource1.ExtractSourceOptions.Filter = "";
        this.dashboardExtractDataSource1.ExtractSourceOptions.RowCount = -1;
        this.dashboardExtractDataSource1.FileName = "|DataDirectory|\\Data\\Sales.dat";
        this.dashboardExtractDataSource1.Name = "Extract Data Source 1";
        //
        // SampleDashboard
        //
        this.CurrencyCultureName = null;
        this.DataSources.AddRange(new DevExpress.DashboardCommon.IDashboardDataSource[] {
        this.dashboardExtractDataSource1});
        this.Items.AddRange(new DevExpress.DashboardCommon.DashboardItem[] {
        this.choroplethMapDashboardItem1,
        this.gridDashboardItem1});
        dashboardLayoutItem1.DashboardItem = this.choroplethMapDashboardItem1;
        dashboardLayoutItem1.Weight = 62.659846547314579D;
        dashboardLayoutItem2.DashboardItem = this.gridDashboardItem1;
        dashboardLayoutItem2.Weight = 37.340153452685421D;
        dashboardLayoutGroup1.ChildNodes.AddRange(new DevExpress.DashboardCommon.DashboardLayoutNode[] {
        dashboardLayoutItem1,
        dashboardLayoutItem2});
        dashboardLayoutGroup1.DashboardItem = null;
        this.LayoutRoot = dashboardLayoutGroup1;
        this.Title.Text = "Sample Dashboard";
        ((System.ComponentModel.ISupportInitialize)(dimension1)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(measure1)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(measure2)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(measure3)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.choroplethMapDashboardItem1)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(dimension2)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(measure4)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(measure5)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.gridDashboardItem1)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.dashboardExtractDataSource1)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

    }

    #endregion

    private DevExpress.DashboardCommon.ChoroplethMapDashboardItem choroplethMapDashboardItem1;
    private DevExpress.DashboardCommon.GridDashboardItem gridDashboardItem1;
    private DevExpress.DashboardCommon.DashboardExtractDataSource dashboardExtractDataSource1;
}
