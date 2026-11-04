// <copyright file="ShellViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Desktop.Wpf.Localization;
using FatturaFacile.Desktop.Wpf.Services;

namespace FatturaFacile.Desktop.Wpf.ViewModels;

/// <summary>
/// Root enterprise ERP shell view model managing navigation, header bar, active view, and language selection.
/// </summary>
internal sealed partial class ShellViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly DesktopUserContext _userContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShellViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">The navigation coordinator.</param>
    /// <param name="userContext">The desktop user session context.</param>
    /// <param name="localizer">The desktop localizer.</param>
    public ShellViewModel(
        INavigationService navigationService,
        DesktopUserContext userContext,
        IDesktopLocalizer localizer)
    {
        _navigationService = navigationService;
        _userContext = userContext;
        Localizer = localizer;

        Title = Localizer["App.Title"];
        SelectedLanguage = Localizer.CurrentCulture;

        _navigationService.CurrentViewModelChanged += (_, _) => CurrentContent = _navigationService.CurrentViewModel;

        // Initialize Navigation Sections
        InitNavigationTree();

        ToggleSidebarCommand = new RelayCommand(() => IsSidebarExpanded = !IsSidebarExpanded);
        NavigateCommand = new RelayCommand<string>(ExecuteNavigation);
        SignOutCommand = new RelayCommand(ExecuteSignOut);
        QuickNewInvoiceCommand = new RelayCommand(() => _navigationService.NavigateTo<InvoiceDetailViewModel>());
    }

    [ObservableProperty]
    public partial ViewModelBase? CurrentContent { get; set; }

    [ObservableProperty]
    public partial string ActiveTenantName { get; set; } = "Default Enterprise AG";

    [ObservableProperty]
    public partial string ActiveUserName { get; set; } = "Admin User";

    [ObservableProperty]
    public partial string SelectedLanguage { get; set; }

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsSidebarExpanded { get; set; } = true;

    [ObservableProperty]
    public partial bool IsAuthenticated { get; set; }

    /// <summary>
    /// Gets the localization service instance.
    /// </summary>
    public IDesktopLocalizer Localizer { get; }

    /// <summary>
    /// Gets the hierarchical collection of navigation categories and menu items.
    /// </summary>
    public ObservableCollection<NavigationGroupItem> NavigationGroups { get; } = [];

    /// <summary>
    /// Gets the list of supported culture language codes.
    /// </summary>
    public ReadOnlyCollection<string> AvailableLanguages { get; } = new(new[] { "en", "it", "de", "fr" });

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

    /// <summary>
    /// Activates the authenticated shell context for the given tenant and user.
    /// </summary>
    /// <param name="tenantName">The authenticated tenant organization name or slug.</param>
    /// <param name="userName">The authenticated username or email.</param>
    public void OnAuthenticated(string tenantName, string userName)
    {
        ActiveTenantName = tenantName;
        ActiveUserName = userName;
        IsAuthenticated = true;
    }

    partial void OnSelectedLanguageChanged(string value)
    {
        Localizer.SetCulture(value);
        _userContext.SetCulture(value);
        Title = Localizer["App.Title"];
        InitNavigationTree(); // refresh localized nav titles
    }

    private void InitNavigationTree()
    {
        NavigationGroups.Clear();

        // 1. Dashboard
        var dashboardGroup = new NavigationGroupItem(Localizer["Nav.Dashboard"], "Home");
        dashboardGroup.Items.Add(new NavigationLeafItem(Localizer["Nav.Dashboard"], "Dashboard", "ViewDashboard"));
        NavigationGroups.Add(dashboardGroup);

        // 2. Sales & Invoicing
        var salesGroup = new NavigationGroupItem(Localizer["Nav.SalesInvoicing"], "Receipt");
        salesGroup.Items.Add(new NavigationLeafItem(Localizer["Nav.Invoices"], "Invoices", "FileDocumentOutline"));
        salesGroup.Items.Add(new NavigationLeafItem(Localizer["Nav.SwissQrBill"], "SwissQrBill", "Qrcode"));
        salesGroup.Items.Add(new NavigationLeafItem(Localizer["Nav.ItalianSdi"], "ItalianSdi", "Xml"));
        salesGroup.Items.Add(new NavigationLeafItem(Localizer["Nav.CreditNotes"], "CreditNotes", "FileUndoOutline"));
        NavigationGroups.Add(salesGroup);

        // 3. Purchases & Expenses
        var purchaseGroup = new NavigationGroupItem(Localizer["Nav.PurchasesExpenses"], "CartOutline");
        purchaseGroup.Items.Add(new NavigationLeafItem(Localizer["Nav.SupplierInvoices"], "SupplierInvoices", "TruckDeliveryOutline"));
        purchaseGroup.Items.Add(new NavigationLeafItem(Localizer["Nav.Expenses"], "Expenses", "CashMinus"));
        NavigationGroups.Add(purchaseGroup);

        // 4. Accounting & Finance
        var accountingGroup = new NavigationGroupItem(Localizer["Nav.AccountingFinance"], "BookOutline");
        accountingGroup.Items.Add(new NavigationLeafItem(Localizer["Nav.GeneralLedger"], "GeneralLedger", "BookOpenOutline"));
        accountingGroup.Items.Add(new NavigationLeafItem(Localizer["Nav.ChartOfAccounts"], "ChartOfAccounts", "FormatListNumbered"));
        accountingGroup.Items.Add(new NavigationLeafItem(Localizer["Nav.VatReconciliation"], "VatReconciliation", "PercentOutline"));
        NavigationGroups.Add(accountingGroup);

        // 5. CRM & Contacts
        var crmGroup = new NavigationGroupItem(Localizer["Nav.CrmContacts"], "AccountGroupOutline");
        crmGroup.Items.Add(new NavigationLeafItem(Localizer["Nav.Customers"], "Customers", "AccountOutline"));
        crmGroup.Items.Add(new NavigationLeafItem(Localizer["Nav.Suppliers"], "Suppliers", "Domain"));
        NavigationGroups.Add(crmGroup);

        // 6. Documents & DMS
        var dmsGroup = new NavigationGroupItem(Localizer["Nav.DocumentsDms"], "FolderOutline");
        dmsGroup.Items.Add(new NavigationLeafItem(Localizer["Nav.Archive"], "DmsArchive", "ArchiveOutline"));
        NavigationGroups.Add(dmsGroup);

        // 7. Settings
        var settingsGroup = new NavigationGroupItem(Localizer["Nav.SettingsAdmin"], "CogOutline");
        settingsGroup.Items.Add(new NavigationLeafItem(Localizer["Nav.CompanyProfile"], "CompanyProfile", "OfficeBuilding"));
        settingsGroup.Items.Add(new NavigationLeafItem(Localizer["Nav.Preferences"], "Preferences", "Tune"));
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
