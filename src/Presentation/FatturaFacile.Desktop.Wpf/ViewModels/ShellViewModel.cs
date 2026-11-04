using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Desktop.Wpf.Localization;
using FatturaFacile.Desktop.Wpf.Services;

namespace FatturaFacile.Desktop.Wpf.ViewModels;

/// <summary>
/// Root enterprise ERP shell view model managing navigation, header bar, active view, and language selection.
/// </summary>
public sealed class ShellViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly DesktopUserContext _userContext;
    private readonly DesktopTenantContext _tenantContext;
    private readonly IDesktopLocalizer _localizer;

    private ViewModelBase? _currentContent;
    private string _activeTenantName = "Default Enterprise AG";
    private string _activeUserName = "Admin User";
    private string _selectedLanguage = "en";
    private string _searchQuery = string.Empty;
    private bool _isSidebarExpanded = true;
    private bool _isAuthenticated;

    public ShellViewModel(
        INavigationService navigationService,
        DesktopUserContext userContext,
        DesktopTenantContext tenantContext,
        IDesktopLocalizer localizer)
    {
        _navigationService = navigationService;
        _userContext = userContext;
        _tenantContext = tenantContext;
        _localizer = localizer;

        Title = _localizer["App.Title"];
        _selectedLanguage = _localizer.CurrentCulture;

        _navigationService.CurrentViewModelChanged += vm =>
        {
            CurrentContent = vm;
        };

        // Initialize Navigation Sections
        InitNavigationTree();

        ToggleSidebarCommand = new RelayCommand(() => IsSidebarExpanded = !IsSidebarExpanded);
        NavigateCommand = new RelayCommand<string>(ExecuteNavigation);
        SignOutCommand = new RelayCommand(ExecuteSignOut);
        QuickNewInvoiceCommand = new RelayCommand(() => _navigationService.NavigateTo<InvoiceDetailViewModel>());
    }

    public IDesktopLocalizer Localizer => _localizer;

    public ViewModelBase? CurrentContent
    {
        get => _currentContent;
        set => SetProperty(ref _currentContent, value);
    }

    public string ActiveTenantName
    {
        get => _activeTenantName;
        set => SetProperty(ref _activeTenantName, value);
    }

    public string ActiveUserName
    {
        get => _activeUserName;
        set => SetProperty(ref _activeUserName, value);
    }

    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (SetProperty(ref _selectedLanguage, value))
            {
                _localizer.SetCulture(value);
                _userContext.SetCulture(value);
                Title = _localizer["App.Title"];
                InitNavigationTree();
            }
        }
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }

    public bool IsSidebarExpanded
    {
        get => _isSidebarExpanded;
        set => SetProperty(ref _isSidebarExpanded, value);
    }

    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        set => SetProperty(ref _isAuthenticated, value);
    }

    public ObservableCollection<NavigationGroupItem> NavigationGroups { get; } = [];

    public IRelayCommand ToggleSidebarCommand { get; }
    public IRelayCommand<string> NavigateCommand { get; }
    public IRelayCommand SignOutCommand { get; }
    public IRelayCommand QuickNewInvoiceCommand { get; }

    public void OnAuthenticated(string tenantName, string userName)
    {
        ActiveTenantName = tenantName;
        ActiveUserName = userName;
        IsAuthenticated = true;
    }

    private void ExecuteNavigation(string? destination)
    {
        if (string.IsNullOrWhiteSpace(destination)) return;

        switch (destination)
        {
            case "Dashboard":
                _navigationService.NavigateTo<DashboardViewModel>();
                break;
            case "Invoices":
                _navigationService.NavigateTo<InvoiceListViewModel>();
                break;
            case "NewInvoice":
                _navigationService.NavigateTo<InvoiceDetailViewModel>();
                break;
        }
    }

    private void ExecuteSignOut()
    {
        _userContext.Clear();
        IsAuthenticated = false;
        _navigationService.NavigateTo<LoginViewModel>();
    }

    private void InitNavigationTree()
    {
        NavigationGroups.Clear();

        // 1. Dashboard
        NavigationGroups.Add(new NavigationGroupItem(
            _localizer["Nav.Dashboard"],
            "Home",
            [
                new NavigationLeafItem(_localizer["Nav.Dashboard"], "Dashboard")
            ]));

        // 2. Sales & Invoicing
        NavigationGroups.Add(new NavigationGroupItem(
            _localizer["Nav.SalesInvoicing"],
            "Receipt",
            [
                new NavigationLeafItem(_localizer["Nav.Invoices"], "Invoices"),
                new NavigationLeafItem(_localizer["Nav.Proformas"], "Invoices"),
                new NavigationLeafItem(_localizer["Nav.CreditNotes"], "Invoices"),
                new NavigationLeafItem(_localizer["Nav.SwissQrBill"], "Invoices"),
                new NavigationLeafItem(_localizer["Nav.ItalianSdi"], "Invoices")
            ]));

        // 3. Purchases & Expenses
        NavigationGroups.Add(new NavigationGroupItem(
            _localizer["Nav.PurchasesExpenses"],
            "ShoppingCart",
            [
                new NavigationLeafItem(_localizer["Nav.SupplierInvoices"], "Dashboard"),
                new NavigationLeafItem(_localizer["Nav.Expenses"], "Dashboard"),
                new NavigationLeafItem(_localizer["Nav.VendorOrders"], "Dashboard")
            ]));

        // 4. Accounting & Finance
        NavigationGroups.Add(new NavigationGroupItem(
            _localizer["Nav.AccountingFinance"],
            "Finance",
            [
                new NavigationLeafItem(_localizer["Nav.GeneralLedger"], "Dashboard"),
                new NavigationLeafItem(_localizer["Nav.ChartOfAccounts"], "Dashboard"),
                new NavigationLeafItem(_localizer["Nav.VatReconciliation"], "Dashboard"),
                new NavigationLeafItem(_localizer["Nav.BalanceSheet"], "Dashboard")
            ]));

        // 5. CRM & Contacts
        NavigationGroups.Add(new NavigationGroupItem(
            _localizer["Nav.CrmContacts"],
            "AccountMultiple",
            [
                new NavigationLeafItem(_localizer["Nav.Customers"], "Dashboard"),
                new NavigationLeafItem(_localizer["Nav.Vendors"], "Dashboard"),
                new NavigationLeafItem(_localizer["Nav.ContactPersons"], "Dashboard"),
                new NavigationLeafItem(_localizer["Nav.Leads"], "Dashboard")
            ]));

        // 6. Warehouse & Products
        NavigationGroups.Add(new NavigationGroupItem(
            _localizer["Nav.WarehouseProducts"],
            "PackageVariant",
            [
                new NavigationLeafItem(_localizer["Nav.ProductCatalog"], "Dashboard"),
                new NavigationLeafItem(_localizer["Nav.StockLevels"], "Dashboard"),
                new NavigationLeafItem(_localizer["Nav.PriceLists"], "Dashboard")
            ]));

        // 7. Administration & Security
        NavigationGroups.Add(new NavigationGroupItem(
            _localizer["Nav.AdministrationSecurity"],
            "ShieldAccount",
            [
                new NavigationLeafItem(_localizer["Nav.UserManagement"], "Dashboard"),
                new NavigationLeafItem(_localizer["Nav.RolePermissions"], "Dashboard"),
                new NavigationLeafItem(_localizer["Nav.TenantSettings"], "Dashboard"),
                new NavigationLeafItem(_localizer["Nav.AuditLog"], "Dashboard"),
                new NavigationLeafItem(_localizer["Nav.BackupRestore"], "Dashboard")
            ]));
    }
}

public sealed class NavigationGroupItem(string header, string icon, IEnumerable<NavigationLeafItem> items)
{
    public string Header { get; } = header;
    public string Icon { get; } = icon;
    public ObservableCollection<NavigationLeafItem> Items { get; } = new(items);
}

public sealed class NavigationLeafItem(string title, string destination)
{
    public string Title { get; } = title;
    public string Destination { get; } = destination;
}
