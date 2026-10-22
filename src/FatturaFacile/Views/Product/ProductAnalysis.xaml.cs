using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DevExpress.DevAV.ViewModels;
using DevExpress.Spreadsheet;

namespace DevExpress.DevAV.Views.Product {
    public partial class ProductAnalysis : UserControl {
        AnalysisPeriod.DatePeriod dataPeriod;
        AnalysisPeriod.DatePeriod reportPeriod;
        public ProductAnalysis() {
            InitializeComponent();
            SubscribeEvents();
            InitializeCurrentDateTimePeriod();
        }

        void SubscribeEvents() {
            Loaded += OnLoaded;
            spreadsheetControl.DocumentLoaded += OnDocumentLoaded;
        }

        void OnLoaded(object sender, RoutedEventArgs e) {
            ((Mvvm.ISplashScreenService)splashScreenService).ShowSplashScreen(string.Empty);
            LoadTemplate();
        }
        void LoadTemplate() {
            using(var stream = AnalysisTemplatesHelper.GetAnalysisTemplate(AnalysisTemplate.ProductSales))
                spreadsheetControl.LoadDocument(stream);
        }

        void OnDocumentLoaded(object sender, EventArgs e) {
            LoadData();
            ((Mvvm.ISplashScreenService)splashScreenService).HideSplashScreen();
        }
        void InitializeCurrentDateTimePeriod() {
            ProductAnalysisViewModel viewModel = (ProductAnalysisViewModel)this.DataContext;
            var maxDate = viewModel.GetMaxOrdersDate();
            dataPeriod = AnalysisPeriod.CalculateAnalysisPeriod(maxDate);
            reportPeriod = AnalysisPeriod.CalculateReportPeriod(maxDate);
        }
        void LoadData() {
            ProductAnalysisViewModel ViewModel = (ProductAnalysisViewModel)this.DataContext;
            spreadsheetControl.Document.BeginUpdate();
            var defName = spreadsheetControl.Document.DefinedNames.GetDefinedName("ReportYear");
            if(defName != null)
                defName.RefersTo = "=" + reportPeriod.End.Year.ToString(System.Globalization.CultureInfo.InvariantCulture);

            var financialReportWorksheet = spreadsheetControl.Document.Worksheets["Financial Report"];
            var financialReportItems = ViewModel.GetFinancialReport(dataPeriod.Start, dataPeriod.End).ToList(); 
            var frProducts = financialReportItems
                .Select(i => i.ProductName)
                .Distinct()
                .OrderBy(i => i).ToList();
            financialReportWorksheet.Import(frProducts, 17, 1, true);
            foreach(ProductsAnalysis.Item item in financialReportItems) {
                int rowOffset = frProducts.IndexOf(item.ProductName);
                int columnOffset = AnalysisPeriod.MonthOffset(dataPeriod.Start, item.Date) / 12;
                if(rowOffset < 0 || columnOffset < 0) continue;
                financialReportWorksheet.Cells[17 + rowOffset, 3 + columnOffset * 2].SetValue(item.Total);
            }

            var financialDataWorksheet = spreadsheetControl.Document.Worksheets["Financial Data"];
            var financialDataItems = ViewModel.GetFinancialData(dataPeriod.Start, dataPeriod.End).ToList(); 
            foreach(ProductsAnalysis.Item item in financialDataItems) {
                int rowOffset = AnalysisPeriod.MonthOffset(dataPeriod.Start, item.Date);
                int columnOffset = GetColumnIndex(item.ProductCategory);
                if(rowOffset < 0 || columnOffset < 0) continue;
                financialDataWorksheet.Cells[6 + rowOffset, 3 + columnOffset].SetValue(item.Total);
            }
            spreadsheetControl.Document.Worksheets.ActiveWorksheet = financialReportWorksheet;
            spreadsheetControl.Document.EndUpdate();
        }
        int GetColumnIndex(ProductCategory category) {
            switch(category) {
                case ProductCategory.Televisions:
                    return 0;
                case ProductCategory.Monitors:
                    return 1;
                case ProductCategory.VideoPlayers:
                    return 2;
                case ProductCategory.Automation:
                    return 3;
                default:
                    return -1;
            }
        }
    }
}
