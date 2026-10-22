using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using DevExpress.DevAV.Common.ViewModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
#if NET
using System.Collections.Generic;
using DevExpress.AIIntegration.Extensions;
using DevExpress.DevAV.AiTools;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
#endif

namespace DevExpress.DevAV.ViewModels {
    partial class ProductCollectionViewModel : ISupportFiltering<Product> {
        ViewSettingsViewModel viewSettings;

        protected IDocumentManagerService AnalysisWindowedDocumentUIService { get { return this.GetRequiredService<IDocumentManagerService>("AnalysisWindowedDocumentUIService"); } }

        [BindableProperty]
        public virtual string FilterString { get; set; }
#if NET
        IEnumerable<PromptToExpressionRequestBase.ColumnInfo> ISupportFiltering.GetMetadata() {
            return GetProps();
        }
        IEnumerable<PromptToExpressionRequestBase.ColumnInfo> GetProps() {
            return new List<PromptToExpressionRequestBase.ColumnInfo> {
                new (nameof(Product.Name), "Product Name", string.Empty, typeof(string), Enumerable.Empty<PromptToExpressionRequestBase.ColumnInfo>()),
                new (nameof(Product.Cost), "Cost", string.Empty, typeof(decimal), Enumerable.Empty<PromptToExpressionRequestBase.ColumnInfo>()),
                new (nameof(Product.SalePrice), "Sale Price", string.Empty, typeof(decimal), Enumerable.Empty<PromptToExpressionRequestBase.ColumnInfo>()),
                new (nameof(Product.RetailPrice), "Retail Price", string.Empty, typeof(decimal), Enumerable.Empty<PromptToExpressionRequestBase.ColumnInfo>()),
                new (nameof(Product.CurrentInventory), "Inventory", string.Empty, typeof(int?), Enumerable.Empty<PromptToExpressionRequestBase.ColumnInfo>()),
                new (nameof(Product.Backorder), "Backorder", string.Empty, typeof(int), Enumerable.Empty<PromptToExpressionRequestBase.ColumnInfo>()),
                new (nameof(Product.Category), "Category", string.Empty, typeof(ProductCategory), Enumerable.Empty<PromptToExpressionRequestBase.ColumnInfo>()),
                new (nameof(Product.ProductionStart), "Start", string.Empty, typeof(DateTime), Enumerable.Empty<PromptToExpressionRequestBase.ColumnInfo>()),
                new (nameof(Product.Available), "Available", string.Empty, typeof(bool), Enumerable.Empty<PromptToExpressionRequestBase.ColumnInfo>()),
                new (nameof(Product.ConsumerRating), "Consumer Rating", string.Empty, typeof(double), Enumerable.Empty<PromptToExpressionRequestBase.ColumnInfo>()),
                new (nameof(Product.Manufacturing), "Manufacturing", string.Empty, typeof(int), Enumerable.Empty<PromptToExpressionRequestBase.ColumnInfo>())
            };
        }
#endif

        public virtual Stream SelectedPdfStream { get; set; }
        public ViewSettingsViewModel ViewSettings {
            get {
                if(viewSettings == null)
                    viewSettings = ViewSettingsViewModel.Create(CollectionViewKind.ListView, this);
                return viewSettings;
            }
        }
        public override void New() { ShowProductEditForm(); }
        public override void Edit(Product entity) { ShowProductEditForm(); }
        protected override void OnSelectedEntityChanged() {
            base.OnSelectedEntityChanged();
            if(SelectedEntity != null && SelectedEntity.Catalog != null)
                SelectedPdfStream = SelectedEntity.Catalog[0].PdfStream;
            SetDefaultReport(ReportInfoFactory.ProductProfile(SelectedEntity));
        }
        protected override void OnEntitiesAssigned(Func<Product> getSelectedEntityCallback) {
            base.OnEntitiesAssigned(getSelectedEntityCallback);
            if(Entities.Any() && SelectedEntity == null)
                SelectedEntity = Entities.OrderBy(x => x.RetailPrice).FirstOrDefault();
            ViewSettings.IsDataPaneVisible = Entities.Any();
        }

        public void PrintOrderDetail() {
            ShowReport(ReportInfoFactory.ProductOrders(SelectedEntity.OrderItems, CreateUnitOfWork().States.ToList()), "Orders");
        }
        public bool CanPrintOrderDetail() {
            return SelectedEntity != null;
        }
        public void PrintSpecificationSummary() {
            ShowReport(ReportInfoFactory.ProductProfile(SelectedEntity), "Profile");
        }
        public bool CanPrintSpecificationSummary() {
            return SelectedEntity != null;
        }
        public void PrintSalesSummary() {
            ShowReport(ReportInfoFactory.ProductSalesSummary(SelectedEntity.OrderItems), "SalesSummary");
        }
        public bool CanPrintSalesSummary() {
            return SelectedEntity != null;
        }
        public void PrintTopSalesperson() {
            ShowReport(ReportInfoFactory.ProductTopSalesPerson(SelectedEntity.OrderItems), "TopSalesPerson");
        }
        public bool CanPrintTopSalesperson() {
            return SelectedEntity != null;
        }

        void ShowReport(IReportInfo reportInfo, string reportId) {
            ExportService.ShowReport(reportInfo);
            PrintService.ShowReport(reportInfo);
            Logger.Log(string.Format("OutlookInspiredApp: Create Report : Products: {0}", reportId));
        }
        void SetDefaultReport(IReportInfo reportInfo) {
            try {
                if(this.IsInDesignMode()) return;
                ExportService?.SetDefaultReport(reportInfo);
                PrintService?.SetDefaultReport(reportInfo);
            }
            catch { }
        }
        IReportService PrintService { get { return this.GetRequiredService<IReportService>("PrintService"); } }
        IReportService ExportService { get { return this.GetRequiredService<IReportService>("ExportService"); } }

        public void ShowMap() {
            ProductMapViewModel mapViewModel = ViewModelSource.Create(() => new ProductMapViewModel());
            Logger.Log("OutlookInspiredApp: View ProductCollection Map");
            this.GetRequiredService<IDocumentManagerService>().CreateDocument("CitiesMapView", mapViewModel, SelectedEntity.Id, this).Show();
        }
        public bool CanShowMap() {
            return SelectedEntity != null;
        }
        void ShowProductEditForm() {
            MessageBoxService.Show(@"You can easily create custom edit forms using the 40+ controls that ship as part of the DevExpress Data Editors Library. To see what you can build, activate the Employees module.",
                "Edit Products", MessageButton.OK, MessageIcon.Asterisk, MessageResult.OK);
        }
        public void CreateCustomFilter() {
            Messenger.Default.Send(new CreateCustomFilterMessage<Product>());
        }

        public void ShowAnalysis() {
            AnalysisWindowedDocumentUIService.CreateDocument("ProductAnalysis", null, this).Show();
        }
        public override void Delete(Product entity) {
            MessageBoxService.ShowMessage("To ensure data integrity, the Products module doesn't allow records to be deleted. Record deletion is supported by the Employees module.", "Delete Product", MessageButton.OK);
        }

        #region ISupportFiltering

        Expression<Func<Product, bool>> ISupportFiltering<Product>.FilterExpression {
            get { return FilterExpression; }
            set { FilterExpression = value; }
        }

        #endregion

    }
}
