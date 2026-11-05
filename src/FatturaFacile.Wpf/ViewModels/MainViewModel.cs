// <copyright file="MainViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Tenants.Commands;
using FatturaFacile.Application.Features.Tenants.Queries;
using FatturaFacile.Domain.Common;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Root view model for the main application shell, coordinating tenant switching, session security, and dynamic localization.
/// </summary>
internal sealed partial class MainViewModel : ObservableObject
{
    private readonly ISender _sender;
    private readonly IUserAuthenticationService _authService;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private ObservableCollection<TenantDto> _tenants = [];

    [ObservableProperty]
    private TenantDto? _selectedTenant;

    [ObservableProperty]
    private TenantDto? _loginSelectedTenant;

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _activeModuleName = string.Empty;

    [ObservableProperty]
    private string _hardwareSecurityStatus = string.Empty;

    [ObservableProperty]
    private bool _isLocked;

    [ObservableProperty]
    private bool _isLoggedOut;

    [ObservableProperty]
    private string _lockStatusMessage = string.Empty;

    [ObservableProperty]
    private string _loginStatusMessage = string.Empty;

    [ObservableProperty]
    private CultureInfo _selectedCulture;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender instance.</param>
    /// <param name="authService">The hardware-backed authentication service.</param>
    /// <param name="currentTenantService">The active tenant context service.</param>
    /// <param name="localizationService">The dynamic localization service.</param>
    /// <param name="invoicing">Invoicing view model.</param>
    /// <param name="accounting">Accounting view model.</param>
    /// <param name="contacts">Contacts view model.</param>
    /// <param name="products">Products view model.</param>
    /// <param name="payroll">Payroll view model.</param>
    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "DI composition point wiring the shell to one view model per ERP module.")]
    public MainViewModel(
        ISender sender,
        IUserAuthenticationService authService,
        ICurrentTenantService currentTenantService,
        ILocalizationService localizationService,
        InvoicingViewModel invoicing,
        AccountingViewModel accounting,
        ContactsViewModel contacts,
        ProductsViewModel products,
        PayrollViewModel payroll)
    {
        _sender = sender;
        _authService = authService;
        _currentTenantService = currentTenantService;
        _localizationService = localizationService;

        Invoicing = invoicing;
        Accounting = accounting;
        Contacts = contacts;
        Products = products;
        Payroll = payroll;

        _currentView = invoicing;
        _selectedCulture = localizationService.CurrentCulture;
        _localizationService.CultureChanged += (_, _) => OnCultureChanged();
        _currentTenantService.CurrentTenantChanged += (_, _) => OnTenantChanged();

        UpdateLocalizedShellStrings();
    }

    /// <summary>
    /// Gets the invoicing view model.
    /// </summary>
    public InvoicingViewModel Invoicing { get; }

    /// <summary>
    /// Gets the accounting view model.
    /// </summary>
    public AccountingViewModel Accounting { get; }

    /// <summary>
    /// Gets the contacts view model.
    /// </summary>
    public ContactsViewModel Contacts { get; }

    /// <summary>
    /// Gets the products view model.
    /// </summary>
    public ProductsViewModel Products { get; }

    /// <summary>
    /// Gets the payroll view model.
    /// </summary>
    public PayrollViewModel Payroll { get; }

    /// <summary>
    /// Gets the supported application cultures.
    /// </summary>
    public IReadOnlyList<CultureInfo> SupportedCultures => _localizationService.SupportedCultures;

    /// <summary>
    /// Asynchronously initializes tenant list and displays active module.
    /// </summary>
    /// <returns>A task representing the initialization operation.</returns>
    public async Task InitializeAsync()
    {
        var tenants = await _sender.Send(new GetTenantsQuery());
        Tenants.Clear();
        foreach (var tenant in tenants)
        {
            Tenants.Add(tenant);
        }

        SelectedTenant = Tenants.FirstOrDefault(t => t.Id == _currentTenantService.TenantId) ?? Tenants.FirstOrDefault();
        LoginSelectedTenant = SelectedTenant;

        await NavigateToModuleAsync("Invoicing");
    }

    /// <summary>
    /// Navigates to the designated ERP module.
    /// </summary>
    /// <param name="moduleName">Target module identifier.</param>
    /// <returns>A task representing navigation operation.</returns>
    [RelayCommand]
    public async Task NavigateToModuleAsync(string? moduleName)
    {
        switch (moduleName)
        {
            case "Accounting":
                CurrentView = Accounting;
                ActiveModuleName = _localizationService["Nav_Accounting"];
                await Accounting.LoadDataAsync();
                break;
            case "Contacts":
                CurrentView = Contacts;
                ActiveModuleName = _localizationService["Nav_Contacts"];
                await Contacts.LoadContactsAsync();
                break;
            case "Products":
                CurrentView = Products;
                ActiveModuleName = _localizationService["Nav_Products"];
                await Products.LoadProductsAsync();
                break;
            case "Payroll":
                CurrentView = Payroll;
                ActiveModuleName = _localizationService["Nav_Payroll"];
                await Payroll.LoadEmployeesAsync();
                break;
            default:
                CurrentView = Invoicing;
                ActiveModuleName = _localizationService["Nav_Invoicing"];
                await Invoicing.LoadInvoicesAsync();
                break;
        }
    }

    /// <summary>
    /// Handles tenant switching by updating active tenant context and refreshing all loaded modules.
    /// </summary>
    /// <param name="newTenant">The tenant selected by user.</param>
    /// <returns>A task representing the switch operation.</returns>
    [RelayCommand]
    public async Task SwitchTenantAsync(TenantDto? newTenant)
    {
        if (newTenant == null || newTenant.Id == _currentTenantService.TenantId)
        {
            return;
        }

        SelectedTenant = newTenant;
        LoginSelectedTenant = newTenant;

        _ = await _sender.Send(new SwitchTenantCommand(newTenant.Id));
    }

    /// <summary>
    /// Locks the desktop application workspace displaying the secure biometric unlock overlay.
    /// </summary>
    [RelayCommand]
    public void LockApplication()
    {
        IsLocked = true;
        LockStatusMessage = string.Empty;
    }

    /// <summary>
    /// Prompts user for Windows Hello / PIN biometric verification to unlock application workspace.
    /// </summary>
    /// <returns>A task representing biometric unlock operation.</returns>
    [RelayCommand]
    public async Task UnlockApplicationAsync()
    {
        var reason = _localizationService["Lock_BiometricReason"];
        var verified = await _authService.AuthenticateCurrentUserAsync(reason);
        if (verified)
        {
            IsLocked = false;
            LockStatusMessage = string.Empty;
        }
        else
        {
            LockStatusMessage = _localizationService["Lock_Failed"];
        }
    }

    /// <summary>
    /// Performs complete session logout, wiping in-memory tenant session and displaying secure login screen.
    /// </summary>
    [RelayCommand]
    public void Logout()
    {
        IsLoggedOut = true;
        IsLocked = false;
        LoginStatusMessage = string.Empty;
        LoginSelectedTenant = SelectedTenant;
    }

    /// <summary>
    /// Authenticates user and restores ERP session for the chosen tenant.
    /// </summary>
    /// <returns>A task representing login verification.</returns>
    [RelayCommand]
    public async Task LoginAsync()
    {
        var reason = _localizationService["Login_BiometricReason"];
        var verified = await _authService.AuthenticateCurrentUserAsync(reason);
        if (verified)
        {
            IsLoggedOut = false;
            LoginStatusMessage = string.Empty;

            if (LoginSelectedTenant != null && LoginSelectedTenant.Id != _currentTenantService.TenantId)
            {
                await SwitchTenantAsync(LoginSelectedTenant);
            }
            else if (SelectedTenant != null)
            {
                await RefreshActiveViewAsync();
            }
        }
        else
        {
            LoginStatusMessage = _localizationService["Login_Failed"];
        }
    }

    partial void OnSelectedTenantChanged(TenantDto? oldValue, TenantDto? newValue)
    {
        if (newValue != null && oldValue != null && oldValue.Id != newValue.Id)
        {
            _ = SwitchTenantAsync(newValue);
        }
    }

    partial void OnSelectedCultureChanged(CultureInfo? oldValue, CultureInfo newValue)
    {
        if (newValue != null && !Equals(oldValue, newValue))
        {
            _localizationService.SetCulture(newValue);
        }
    }

    private void OnCultureChanged() => UpdateLocalizedShellStrings();

    private void OnTenantChanged()
    {
        if (SelectedTenant?.Id != _currentTenantService.TenantId)
        {
            SelectedTenant = Tenants.FirstOrDefault(t => t.Id == _currentTenantService.TenantId);
        }

        _ = RefreshActiveViewAsync();
    }

    private async Task RefreshActiveViewAsync()
    {
        if (ReferenceEquals(CurrentView, Invoicing))
        {
            await Invoicing.LoadInvoicesAsync();
        }
        else if (ReferenceEquals(CurrentView, Accounting))
        {
            await Accounting.LoadDataAsync();
        }
        else if (ReferenceEquals(CurrentView, Contacts))
        {
            await Contacts.LoadContactsAsync();
        }
        else if (ReferenceEquals(CurrentView, Products))
        {
            await Products.LoadProductsAsync();
        }
        else if (ReferenceEquals(CurrentView, Payroll))
        {
            await Payroll.LoadEmployeesAsync();
        }
    }

    private void UpdateLocalizedShellStrings()
    {
        HardwareSecurityStatus = _localizationService["Shell_HardwareSecurity"];

        if (ReferenceEquals(CurrentView, Invoicing))
        {
            ActiveModuleName = _localizationService["Nav_Invoicing"];
        }
        else if (ReferenceEquals(CurrentView, Accounting))
        {
            ActiveModuleName = _localizationService["Nav_Accounting"];
        }
        else if (ReferenceEquals(CurrentView, Contacts))
        {
            ActiveModuleName = _localizationService["Nav_Contacts"];
        }
        else if (ReferenceEquals(CurrentView, Products))
        {
            ActiveModuleName = _localizationService["Nav_Products"];
        }
        else if (ReferenceEquals(CurrentView, Payroll))
        {
            ActiveModuleName = _localizationService["Nav_Payroll"];
        }
    }
}
