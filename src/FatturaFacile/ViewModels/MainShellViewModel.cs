// <copyright file="MainShellViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Ports;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.Navigation;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Top-level shell view model coordinating primary navigation, sidebar responsiveness,
/// active tenant status banner, visual theme toggling, and global modal triggers.
/// </summary>
public partial class MainShellViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IThemeManagerService _themeManager;

    [ObservableProperty]
    private ViewModelBase? _currentViewModel;

    [ObservableProperty]
    private TenantDto? _currentTenant;

    [ObservableProperty]
    private bool _isNavExpanded = true;

    /// <summary>
    /// Event raised when a user requests to switch the active tenant workspace.
    /// </summary>
    public event Action? RequestTenantSwitch;

    /// <summary>
    /// Event raised when a user requests opening the new invoice creation dialog.
    /// </summary>
    public event Action? RequestNewInvoiceDialog;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainShellViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">The navigation coordinator.</param>
    /// <param name="currentTenantService">The ambient tenant context service.</param>
    /// <param name="themeManager">The UI theme manager service.</param>
    /// <param name="invoicesVm">The invoice list view model instance.</param>
    /// <param name="settingsVm">The settings view model instance.</param>
    public MainShellViewModel(
        INavigationService navigationService,
        ICurrentTenantService currentTenantService,
        IThemeManagerService themeManager,
        InvoicesListViewModel invoicesVm,
        SettingsViewModel settingsVm)
    {
        _navigationService = navigationService;
        _currentTenantService = currentTenantService;
        _themeManager = themeManager;
        _currentTenant = _currentTenantService.CurrentTenant;

        _currentTenantService.TenantChanged += (_, _) =>
        {
            CurrentTenant = _currentTenantService.CurrentTenant;
        };

        _navigationService.Navigated += (_, vm) =>
        {
            CurrentViewModel = vm;
        };

        invoicesVm.RequestNewInvoiceDialog += () => RequestNewInvoiceDialog?.Invoke();
        settingsVm.RequestTenantSwitch += () => RequestTenantSwitch?.Invoke();

        // Default initial view
        _navigationService.NavigateTo<InvoicesListViewModel>();
    }

    /// <summary>
    /// Toggles the collapsed/expanded state of the side navigation drawer.
    /// </summary>
    [RelayCommand]
    public void ToggleNav()
    {
        IsNavExpanded = !IsNavExpanded;
    }

    /// <summary>
    /// Navigates to the executive Dashboard view.
    /// </summary>
    [RelayCommand]
    public void NavigateDashboard() => _navigationService.NavigateTo<DashboardViewModel>();

    /// <summary>
    /// Navigates to the Invoices list management view.
    /// </summary>
    [RelayCommand]
    public void NavigateInvoices() => _navigationService.NavigateTo<InvoicesListViewModel>();

    /// <summary>
    /// Navigates to the Customers directory view.
    /// </summary>
    [RelayCommand]
    public void NavigateCustomers() => _navigationService.NavigateTo<CustomersViewModel>();

    /// <summary>
    /// Navigates to the Products catalog view.
    /// </summary>
    [RelayCommand]
    public void NavigateProducts() => _navigationService.NavigateTo<ProductsViewModel>();

    /// <summary>
    /// Navigates to the Database Backups configuration view.
    /// </summary>
    [RelayCommand]
    public void NavigateBackups() => _navigationService.NavigateTo<BackupSettingsViewModel>();

    /// <summary>
    /// Navigates to the Application Settings view.
    /// </summary>
    [RelayCommand]
    public void NavigateSettings() => _navigationService.NavigateTo<SettingsViewModel>();

    /// <summary>
    /// Triggers the tenant switching workflow.
    /// </summary>
    [RelayCommand]
    public void SwitchTenant()
    {
        RequestTenantSwitch?.Invoke();
    }

    /// <summary>
    /// Changes the application user interface culture language.
    /// </summary>
    /// <param name="code">The language code (e.g. "it-IT", "en-US", "de-CH").</param>
    [RelayCommand]
    public void SetLanguage(string code)
    {
        LocalizationManager.Instance.SetCulture(code);
    }

    /// <summary>
    /// Alternates the visual theme between Light and Dark mode.
    /// </summary>
    [RelayCommand]
    public void ToggleTheme()
    {
        var next = _themeManager.IsDark ? AppTheme.Light : AppTheme.Dark;
        _themeManager.SetTheme(next);
    }
}
