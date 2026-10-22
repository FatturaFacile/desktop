using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DevExpress.DevAV;
using DevExpress.DevAV.Common.ViewModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
#if NET
using DevExpress.AIIntegration.Extensions;
using DevExpress.DevAV.AiTools;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
#endif

namespace DevExpress.DevAV.ViewModels {
    partial class CustomerCollectionViewModel : ISupportFiltering<Customer> {
        const string title = "Edit Customers";
        const string text = @"You can easily create custom edit forms using the 40+ controls that ship as part of the DevExpress Data Editors Library. To see what you can build, activate the Employees module.";

        [BindableProperty]
        public virtual string FilterString { get; set; }
#if NET
        IEnumerable<PromptToExpressionRequestBase.ColumnInfo> metadata = null;
        IEnumerable<PromptToExpressionRequestBase.ColumnInfo> ISupportFiltering.GetMetadata() {
            return metadata ??= GetProps();
        }

        IEnumerable<PromptToExpressionRequestBase.ColumnInfo> GetProps() {
            return new List<PromptToExpressionRequestBase.ColumnInfo> {
                new(nameof(Customer.Name), "Customer", string.Empty, typeof(string), Enumerable.Empty<PromptToExpressionRequestBase.ColumnInfo>()),
                new(nameof(Customer.BillingAddress.Line), "Address", string.Empty, typeof(string), Enumerable.Empty<PromptToExpressionRequestBase.ColumnInfo>()),
                new(nameof(Customer.BillingAddress.City), "City", string.Empty, typeof(string), Enumerable.Empty<PromptToExpressionRequestBase.ColumnInfo>()),
                new(nameof(Customer.BillingAddress.State), "State", string.Empty, typeof(StateEnum), Enumerable.Empty<PromptToExpressionRequestBase.ColumnInfo>()),
                new(nameof(Customer.BillingAddress.ZipCode), "Zip Code", string.Empty, typeof(string), Enumerable.Empty<PromptToExpressionRequestBase.ColumnInfo>()),
                new(nameof(Customer.Fax), "Fax", string.Empty, typeof(string), Enumerable.Empty<PromptToExpressionRequestBase.ColumnInfo>()),
                new(nameof(Customer.Website), "Website", string.Empty, typeof(string), Enumerable.Empty<PromptToExpressionRequestBase.ColumnInfo>()),
                new(nameof(Customer.TotalStores), "Stores", string.Empty, typeof(int), Enumerable.Empty<PromptToExpressionRequestBase.ColumnInfo>()),
                new(nameof(Customer.Status), "Status", string.Empty, typeof(CustomerStatus), Enumerable.Empty<PromptToExpressionRequestBase.ColumnInfo>()),
                new(nameof(Customer.AnnualRevenue), "Annual Revenue", string.Empty, typeof(decimal), Enumerable.Empty<PromptToExpressionRequestBase.ColumnInfo>())
            };
        }

#endif
        ViewSettingsViewModel viewSettings;
        public ViewSettingsViewModel ViewSettings {
            get {
                if(viewSettings == null)
                    viewSettings = ViewSettingsViewModel.Create(CollectionViewKind.ListView, this);
                return viewSettings;
            }
        }
        public void ShowSalesMap(Customer customer) {
            CustomerMapViewModel mapViewModel = ViewModelSource.Create(() => new CustomerMapViewModel());
            Logger.Log("OutlookInspiredApp: View Sales Map");
            this.GetRequiredService<IDocumentManagerService>().CreateDocument("CitiesMapView", mapViewModel, customer.Id, this).Show();
        }
        public bool CanShowSalesMap(Customer customer) {
            return customer != null;
        }

        public void PrintCustomerProfile() {
            ShowReport(ReportInfoFactory.CusomerProfile(SelectedEntity), "Profile");
        }
        public bool CanPrintCustomerProfile() {
            return SelectedEntity != null;
        }
        public void PrintDirectory() {
            ShowReport(ReportInfoFactory.CustomerContactsDirectory(SelectedEntity), "ContactsDirectory");
        }
        public bool CanPrintDirectory() {
            return SelectedEntity != null;
        }
        public void PrintOrderDetailReport() {
            ShowReport(ReportInfoFactory.CustomerSalesDetailReport(QueriesHelper.GetCustomerSaleDetails(SelectedEntity.Id, CreateUnitOfWork().OrderItems)), "SalesDetail");
        }
        public bool CanPrintOrderDetailReport() {
            return SelectedEntity != null;
        }
        public void PrintOrderSummaryReport() {
            ShowReport(ReportInfoFactory.CustomerSalesSummaryReport(QueriesHelper.GetCustomerSaleOrderItemDetails(SelectedEntity.Id, CreateUnitOfWork().OrderItems)), "SalesSummary");
        }
        public bool CanPrintOrderSummaryReport() {
            return SelectedEntity != null;
        }
        public void PrintLocationsDirectory() {
            ShowReport(ReportInfoFactory.CustomerLocationsDirectory(Entities), "LocationDirectory");
        }

        protected override void OnSelectedEntityChanged() {
            base.OnSelectedEntityChanged();
            SetDefaultReport(ReportInfoFactory.CusomerProfile(SelectedEntity));
        }
        protected override void OnEntitiesAssigned(Func<Customer> getSelectedEntityCallback) {
            base.OnEntitiesAssigned(getSelectedEntityCallback);
            if(Entities.Any() && SelectedEntity == null)
                SelectedEntity = Entities.OrderBy(x => x.Name).FirstOrDefault();
        }
        void ShowReport(IReportInfo reportInfo, string reportId) {
            ExportService.ShowReport(reportInfo);
            PrintService.ShowReport(reportInfo);
            Logger.Log(string.Format("OutlookInspiredApp: Create Report : Customers: {0}", reportId));
        }
        void SetDefaultReport(IReportInfo reportInfo) {
            if(this.IsInDesignMode()) return;
            ExportService.SetDefaultReport(reportInfo);
            PrintService.SetDefaultReport(reportInfo);
        }
        IReportService PrintService { get { return this.GetRequiredService<IReportService>("PrintService"); } }
        IReportService ExportService { get { return this.GetRequiredService<IReportService>("ExportService"); } }

        public override void Edit(Customer entity) {
            ShowCustomerEditForm();
        }
        public override void New() {
            ShowCustomerEditForm();
        }
        void ShowCustomerEditForm() {
            MessageBoxService.Show(text, title, MessageButton.OK, MessageIcon.Asterisk, MessageResult.OK);
        }

        public void CreateCustomFilter() {
            Messenger.Default.Send(new CreateCustomFilterMessage<Customer>());
        }

        public void ShowAnalysis() {
            IDocumentManagerService documentManagerService = this.GetRequiredService<IDocumentManagerService>("AnalysisWindowedDocumentUIService");
            documentManagerService.CreateDocument("CustomerAnalysis", null, this).Show();
        }
        public override void Delete(Customer entity) {
            MessageBoxService.ShowMessage("To ensure data integrity, the Customers module doesn't allow records to be deleted. Record deletion is supported by the Employees module.", "Delete Customer", MessageButton.OK);
        }

        #region ISupportFiltering

        Expression<Func<Customer, bool>> ISupportFiltering<Customer>.FilterExpression {
            get { return FilterExpression; }
            set { FilterExpression = value; }
        }

        #endregion

    }
}
