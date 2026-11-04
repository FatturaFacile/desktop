// <copyright file="ShellViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

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

    /// <summary>
    /// Initializes a new instance of the <see cref="ShellViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">The navigation coordinator.</param>
    /// <param name="userContext">The desktop user session context.</param>
    /// <param name="tenantContext">The desktop tenant session context.</param>
    /// <param name="localizer">The desktop localizer.</param>
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

    /// <summary>
    /// Gets the localization service instance.
    /// </summary>
    public IDesktopLocalizer Localizer => _localizer;

    /// <summary>
    /// Gets or sets the currently displayed view model inside the workspace frame.
    /// </summary>
    public ViewModelBase? CurrentContent
    {
        get => _currentContent;
        set => SetProperty(ref _currentContent, value);
    }

    /// <summary>
    /// Gets or sets the active tenant company name displayed in the header bar.
    /// </summary>
    public string ActiveTenantName
    {
        get => _activeTenantName;
        set => SetProperty(ref _activeTenantName, value);
    }

    /// <summary>
    /// Gets or sets the active username displayed in the user profile badge.
    /// </summary>
    public string ActiveUserName
    {
        get => _activeUserName;
        set => SetProperty(ref _activeUserName, value);
    }

    /// <summary>
    /// Gets or sets the selected culture code in the language picker dropdown.
    /// </summary>
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
                InitNavigationTree(); // refresh localized nav titles
            }
        }
    }

    /// <summary>
    /// Gets or sets the global search query text entered in the header bar.
    /// </summary>
    public string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the navigation sidebar drawer is expanded.
    /// </summary>
    public bool IsSidebarExpanded
    {
        get => _isSidebarExpanded;
        set => SetProperty(ref _isSidebarExpanded, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether a user has authenticated into the desktop shell.
    /// </summary>
    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        set => SetProperty(ref _isAuthenticated, value);
    }

    /// <summary>
    /// Gets the hierarchical collection of navigation categories and menu items.
    /// </summary>
    public ObservableCollection<NavigationGroupItem> NavigationGroups { get; } = [];

    /// <summary>
    /// Gets the list of supported culture language codes.
    /// </summary>
    public List<string> AvailableLanguages { get; } = ["en", "it", "de", "fr"];

    /// <summary>
    /// Gets the command that toggles the sidebar drawer expanded state.
    /// </summary>
    public IRelayCommand ToggleSidebarCommand { get; }

    /// <summary>
    /// Gets the command that executes navigation based on a destination key.
    /// </summary>
    public IRelayCommand<string> NavigateCommand { get; }

    /// <summary>
    /// Gets the command that logs out the current user session.
    /// </summary>
    public IRelayCommand SignOutCommand { get; }

    /// <summary>
    /// Gets the command that immediately opens the new invoice creation form.
    /// </summary>
    public IRelayCommand QuickNewInvoiceCommand { get; }

    private void InitNavigationTree()
    {
        NavigationGroups.Clear();

        // 1. Dashboard
        var dashboardGroup = new NavigationGroupItem(_localizer["Nav.Dashboard"], "Home");
        dashboardGroup.Items.Add(new NavigationLeafItem(_localizer["Nav.Dashboard"], "Dashboard", "ViewDashboard"));
        NavigationGroups.Add(dashboardGroup);

        // 2. Sales & Invoicing
        var salesGroup = new NavigationGroupItem(_localizer["Nav.SalesInvoicing"], "Receipt");
        salesGroup.Items.Add(new NavigationLeafItem(_localizer["Nav.Invoices"], "Invoices", "FileDocumentOutline"));
        salesGroup.Items.Add(new NavigationLeafItem(_localizer["Nav.SwissQrBill"], "SwissQrBill", "Qrcode"));
        salesGroup.Items.Add(new NavigationLeafItem(_localizer["Nav.ItalianSdi"], "ItalianSdi", "Xml"));
        salesGroup.Items.Add(new NavigationLeafItem(_localizer["Nav.CreditNotes"], "CreditNotes", "FileUndoOutline"));
        NavigationGroups.Add(salesGroup);

        // 3. Purchases & Expenses
        var purchaseGroup = new NavigationGroupItem(_localizer["Nav.PurchasesExpenses"], "CartOutline");
        purchaseGroup.Items.Add(new NavigationLeafItem(_localizer["Nav.SupplierInvoices"], "SupplierInvoices", "TruckDeliveryOutline"));
        purchaseGroup.Items.Add(new NavigationLeafItem(_localizer["Nav.Expenses"], "Expenses", "CashMinus"));
        NavigationGroups.Add(purchaseGroup);

        // 4. Accounting & Finance
        var accountingGroup = new NavigationGroupItem(_localizer["Nav.AccountingFinance"], "BookOutline");
        accountingGroup.Items.Add(new NavigationLeafItem(_localizer["Nav.GeneralLedger"], "GeneralLedger", "BookOpenOutline"));
        accountingGroup.Items.Add(new NavigationLeafItem(_localizer["Nav.ChartOfAccounts"], "ChartOfAccounts", "FormatListNumbered"));
        accountingGroup.Items.Add(new NavigationLeafItem(_localizer["Nav.VatReconciliation"], "VatReconciliation", "PercentOutline"));
        NavigationGroups.Add(accountingGroup);

        // 5. CRM & Contacts
        var crmGroup = new NavigationGroupItem(_localizer["Nav.CrmContacts"], "AccountGroupOutline");
        crmGroup.Items.Add(new NavigationLeafItem(_localizer["Nav.Customers"], "Customers", "AccountOutline"));
        crmGroup.Items.Add(new NavigationLeafItem(_localizer["Nav.Suppliers"], "Suppliers", "Domain"));
        NavigationGroups.Add(crmGroup);

        // 6. Documents & DMS
        var dmsGroup = new NavigationGroupItem(_localizer["Nav.DocumentsDms"], "FolderOutline");
        dmsGroup.Items.Add(new NavigationLeafItem(_localizer["Nav.Archive"], "DmsArchive", "ArchiveOutline"));
        NavigationGroups.Add(dmsGroup);

        // 7. Settings
        var settingsGroup = new NavigationGroupItem(_localizer["Nav.SettingsAdmin"], "CogOutline");
        settingsGroup.Items.Add(new NavigationLeafItem(_localizer["Nav.CompanyProfile"], "CompanyProfile", "OfficeBuilding"));
        settingsGroup.Items.Add(new NavigationLeafItem(_localizer["Nav.Preferences"], "Preferences", "Tune"));
        NavigationGroups.Add(settingsGroup);
    }

    private void ExecuteNavigation(string? target)
    {
        switch (target)
        {
            case "Dashboard":
                _navigationService.NavigateTo<DashboardViewModel>();
                break;
            case "Invoices":
                _navigationService.NavigateTo<InvoiceListViewModel>();
                break;
            case "SwissQrBill":
                _navigationService.NavigateTo<InvoiceDetailViewModel>();
                break;
            default:
                _navigationService.NavigateTo<DashboardViewModel>();
                break;
        }
    }

    private void ExecuteSignOut()
    {
        _userContext.Clear();
        IsAuthenticated = false;
        _navigationService.NavigateTo<LoginViewModel>();
    }
}
