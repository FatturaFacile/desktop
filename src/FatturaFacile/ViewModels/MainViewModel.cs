// <copyright file="MainViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Administration;
using FatturaFacile.Services;
using MediatR;

namespace FatturaFacile.ViewModels;

/// <summary>
/// Represents main view model.
/// </summary>
public sealed partial class MainViewModel : ViewModelBase
{
    private readonly ISender mediator;

    private readonly ITenantProvider tenantProvider;

    private readonly ICurrentUserService currentUserService;

    private readonly ILocalizationService localizationService;

    private readonly DispatcherTimer inactivityTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    /// <param name="tenantProvider">The tenant provider.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="localizationService">The localization service.</param>
    /// <param name="dashboardVM">The dashboard v m.</param>
    /// <param name="clientsVM">The clients v m.</param>
    /// <param name="invoicesVM">The invoices v m.</param>
    /// <param name="tireHotelVM">The tire hotel v m.</param>
    /// <param name="timeTrackingVM">The time tracking v m.</param>
    /// <param name="localizationAdminVM">The localization admin v m.</param>
    /// <param name="loginVM">The login v m.</param>
    public MainViewModel(
        ISender mediator,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUserService,
        ILocalizationService localizationService,
        DashboardViewModel dashboardVM,
        ClientsViewModel clientsVM,
        InvoicesViewModel invoicesVM,
        TireHotelViewModel tireHotelVM,
        TimeTrackingViewModel timeTrackingVM,
        LocalizationAdminViewModel localizationAdminVM,
        LoginViewModel loginVM)
    {
        this.mediator = mediator;
        this.tenantProvider = tenantProvider;
        this.currentUserService = currentUserService;
        this.localizationService = localizationService;

        DashboardVM = dashboardVM;
        ClientsVM = clientsVM;
        InvoicesVM = invoicesVM;
        TireHotelVM = tireHotelVM;
        TimeTrackingVM = timeTrackingVM;
        LocalizationAdminVM = localizationAdminVM;
        LoginVM = loginVM;
        CurrentViewModel = dashboardVM;

        LoginVM.LoginSuccessful += OnLoginSuccessful;

        // Inactivity Timer (30 minutes default, extensible)
        inactivityTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(30) };
        inactivityTimer.Tick += (s, e) => LockSession();
    }

    /// <summary>
    /// Gets the viewModels.
    /// </summary>
    public DashboardViewModel DashboardVM { get; }

    /// <summary>
    /// Gets the clients v m.
    /// </summary>
    public ClientsViewModel ClientsVM { get; }

    /// <summary>
    /// Gets the invoices v m.
    /// </summary>
    public InvoicesViewModel InvoicesVM { get; }

    /// <summary>
    /// Gets the tire hotel v m.
    /// </summary>
    public TireHotelViewModel TireHotelVM { get; }

    /// <summary>
    /// Gets the time tracking v m.
    /// </summary>
    public TimeTrackingViewModel TimeTrackingVM { get; }

    /// <summary>
    /// Gets the localization admin v m.
    /// </summary>
    public LocalizationAdminViewModel LocalizationAdminVM { get; }

    /// <summary>
    /// Gets the login v m.
    /// </summary>
    public LoginViewModel LoginVM { get; }

    /// <summary>
    /// Gets or sets the current view model.
    /// </summary>
    [ObservableProperty]
    public partial ViewModelBase CurrentViewModel { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether sidebar collapsed.
    /// </summary>
    [ObservableProperty]
    public partial bool IsSidebarCollapsed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether authenticated.
    /// </summary>
    [ObservableProperty]
    public partial bool IsAuthenticated { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether session locked.
    /// </summary>
    [ObservableProperty]
    public partial bool IsSessionLocked { get; set; }

    /// <summary>
    /// Gets or sets the current username.
    /// </summary>
    [ObservableProperty]
    public partial string CurrentUsername { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current user role.
    /// </summary>
    [ObservableProperty]
    public partial string CurrentUserRole { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenants.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<TenantDto> Tenants { get; set; } = [];

    /// <summary>
    /// Gets or sets the current tenant.
    /// </summary>
    [ObservableProperty]
    public partial TenantDto? CurrentTenant { get; set; }

    /// <summary>
    /// Gets or sets the languages.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<string> Languages { get; set; } = ["it", "de", "fr", "en"];

    /// <summary>
    /// Gets or sets the current language.
    /// </summary>
    [ObservableProperty]
    public partial string CurrentLanguage { get; set; } = "it";

    /// <summary>
    /// Gets or sets a value indicating whether dark theme.
    /// </summary>
    [ObservableProperty]
    public partial bool IsDarkTheme { get; set; } = true;

    /// <summary>
    /// Initialize Async.
    /// </summary>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public override async Task InitializeAsync()
    {
        await LoginVM.InitializeAsync();
        Tenants = LoginVM.Tenants;
        CurrentTenant = LoginVM.SelectedTenant;

        CurrentLanguage = localizationService.CurrentLocale;
    }

    /// <summary>
    /// Toggle Sidebar.
    /// </summary>
    [RelayCommand]
    public void ToggleSidebar() => IsSidebarCollapsed = !IsSidebarCollapsed;

    /// <summary>
    /// Toggle Theme.
    /// </summary>
    [RelayCommand]
    public void ToggleTheme()
    {
        ThemeManager.Instance.ToggleTheme();
        IsDarkTheme = ThemeManager.Instance.IsDarkTheme;
    }

    /// <summary>
    /// Change Language.
    /// </summary>
    /// <param name="locale">The locale.</param>
    [RelayCommand]
    public void ChangeLanguage(
        string locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
        {
            return;
        }

        CurrentLanguage = locale;
        LocalizationManager.Instance.ChangeLanguage(locale);
    }

    /// <summary>
    /// Switch Tenant Async.
    /// </summary>
    /// <param name="tenant">The tenant.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    [RelayCommand]
    public async Task SwitchTenantAsync(
        TenantDto? tenant)
    {
        if (tenant == null || tenant.Id == tenantProvider.CurrentTenantId)
        {
            return;
        }

        IsBusy = true;
        try
        {
            _ = await mediator.Send(new SwitchTenantCommand(tenant.Id));
            CurrentTenant = tenant;

            // Refresh currently visible module with new tenant data
            await CurrentViewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Lock Session.
    /// </summary>
    [RelayCommand]
    public void LockSession() => IsSessionLocked = true;

    /// <summary>
    /// Unlock Session.
    /// </summary>
    [RelayCommand]
    public void UnlockSession()
    {
        IsSessionLocked = false;
        inactivityTimer.Stop();
        inactivityTimer.Start();
    }

    /// <summary>
    /// Logout.
    /// </summary>
    [RelayCommand]
    public void Logout()
    {
        IsAuthenticated = false;
        IsSessionLocked = false;
        inactivityTimer.Stop();
        currentUserService.Clear();
    }

    /// <summary>
    /// Navigation Commands.
    /// </summary>
    /// <returns>A task representing the result of the asynchronous operation.</returns>
    [RelayCommand]
    public Task NavigateToDashboardAsync()
    {
        CurrentViewModel = DashboardVM;
        return DashboardVM.InitializeAsync();
    }

    /// <summary>
    /// Navigate To Clients Async.
    /// </summary>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    [RelayCommand]
    public Task NavigateToClientsAsync()
    {
        CurrentViewModel = ClientsVM;
        return ClientsVM.InitializeAsync();
    }

    /// <summary>
    /// Navigate To Invoices Async.
    /// </summary>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    [RelayCommand]
    public Task NavigateToInvoicesAsync()
    {
        CurrentViewModel = InvoicesVM;
        return InvoicesVM.InitializeAsync();
    }

    /// <summary>
    /// Navigate To Tire Hotel Async.
    /// </summary>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    [RelayCommand]
    public Task NavigateToTireHotelAsync()
    {
        CurrentViewModel = TireHotelVM;
        return TireHotelVM.InitializeAsync();
    }

    /// <summary>
    /// Navigate To Time Tracking Async.
    /// </summary>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    [RelayCommand]
    public Task NavigateToTimeTrackingAsync()
    {
        CurrentViewModel = TimeTrackingVM;
        return TimeTrackingVM.InitializeAsync();
    }

    /// <summary>
    /// Navigate To Localization Admin Async.
    /// </summary>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    [RelayCommand]
    public Task NavigateToLocalizationAdminAsync()
    {
        CurrentViewModel = LocalizationAdminVM;
        return LocalizationAdminVM.InitializeAsync();
    }

    /// <summary>
    /// Handles the successful login event.
    /// </summary>
    /// <param name="user">The user dto.</param>
    private void OnLoginSuccessful(
        UserDto user)
    {
        IsAuthenticated = true;
        IsSessionLocked = false;
        CurrentUsername = user.FullName;
        CurrentUserRole = user.Role.ToString();

        CurrentTenant = Tenants.FirstOrDefault(t => t.Id == user.TenantId);

        inactivityTimer.Start();

        _ = NavigateToDashboardAsync();
    }
}




//[GenerateViewModel]
//public partial class MainViewModel
//{

//    [GenerateProperty]
//    ObservableCollection<Customer>? customers;

//    [GenerateProperty]
//    bool isInitialized;

//    readonly IDataService dataService;
//    public MainViewModel(IDataService dataService)
//    {
//        this.dataService = dataService;
//    }

//    [GenerateCommand]
//    async Task InitializeAsync()
//    {
//        Customers = new ObservableCollection<Customer>(await dataService.GetCustomersAsync());
//        IsInitialized = true;
//    }

//    bool CanInitialize() => !IsInitialized;

//    void OnUsernameChanged(string oldUsername) => InitializeAsyncCommand.RaiseCanExecuteChanged();
//}




//public class MainViewModel
//{
//    public static MainViewModel Create()
//    {
//        return ViewModelSource.Create(() => new MainViewModel());
//    }
//}
