// <copyright file="MainViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevExpress.Mvvm;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Administration;
using FatturaFacile.Services;
using FatturaFacile.Wpf.Common.Contracts;
using FatturaFacile.Wpf.Common.Services;
using MediatR;

namespace FatturaFacile.ViewModels;

/// <summary>
/// Root view model for the shell window managing navigation, authentication lifecycle,
/// multi-tenant context switching, localization selection, and workspace layout commands.
/// </summary>
public sealed partial class MainViewModel : ViewModelBase, ISupportServices
{
    private readonly ISender _mediator;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILocalizationService _localizationService;
    private readonly IUserStateService _userStateService;
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DispatcherTimer _inactivityTimer;

    // Cache locale dei moduli risolti on-demand
    private DashboardViewModel? _dashboardVM;
    private ClientsViewModel? _clientsVM;
    private InvoicesViewModel? _invoicesVM;
    private TireHotelViewModel? _tireHotelVM;
    private TimeTrackingViewModel? _timeTrackingVM;
    private LocalizationAdminViewModel? _localizationAdminVM;
    private LoginViewModel? _loginVM;

    /// <summary>
    /// Occurs when a request to reset the view's visual docking layout to factory defaults is triggered.
    /// </summary>
    public event Func<Task>? RequestResetLayoutView;

    private readonly IServiceContainer _serviceContainer = new ServiceContainer(null);

    /// <summary>
    /// Gets the service container providing access to DevExpress UI services such as dialog and message box services.
    /// </summary>
    IServiceContainer ISupportServices.ServiceContainer => _serviceContainer;

    private IMessageBoxService MessageBoxService => _serviceContainer.GetService<IMessageBoxService>();

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class with required application and infrastructure services.
    /// Sets up session auto-lock inactivity timers and registers login state event listeners.
    /// </summary>
    /// <param name="mediator">The CQRS command and query bus mediator.</param>
    /// <param name="tenantProvider">The tenant context provider responsible for active tenant resolution.</param>
    /// <param name="currentUserService">The current user identity context service.</param>
    /// <param name="localizationService">The service providing localized resource lookup and language selection.</param>
    /// <param name="userStateService">The persistence service managing user layout settings and UI states.</param>
    /// <param name="viewModelFactory">The factory resolving child feature view models on-demand.</param>
    public MainViewModel(
        ISender mediator,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUserService,
        ILocalizationService localizationService,
        IUserStateService userStateService,
        IViewModelFactory viewModelFactory)
    {
        _mediator = mediator;
        _tenantProvider = tenantProvider;
        _currentUserService = currentUserService;
        _localizationService = localizationService;
        _userStateService = userStateService;
        _viewModelFactory = viewModelFactory;

        LoginVM.LoginSuccessful += OnLoginSuccessful;

        _inactivityTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(30) };
        _inactivityTimer.Tick += (s, e) => LockSession();
    }

    // Risoluzione lazy/on-demand tramite IViewModelFactory con cache
    /// <summary>
    /// Gets the lazily initialized <see cref="LoginViewModel"/> instance responsible for user authentication and tenant selection.
    /// </summary>
    public LoginViewModel LoginVM => _loginVM ??= _viewModelFactory.Create<LoginViewModel>();

    /// <summary>
    /// Gets the lazily initialized <see cref="DashboardViewModel"/> instance displaying executive KPIs and analytics.
    /// </summary>
    public DashboardViewModel DashboardVM => _dashboardVM ??= _viewModelFactory.Create<DashboardViewModel>();

    /// <summary>
    /// Gets the lazily initialized <see cref="ClientsViewModel"/> instance for managing customer directories and contacts.
    /// </summary>
    public ClientsViewModel ClientsVM => _clientsVM ??= _viewModelFactory.Create<ClientsViewModel>();

    /// <summary>
    /// Gets the lazily initialized <see cref="InvoicesViewModel"/> instance for invoice generation, lifecycle tracking, and SDI compliance.
    /// </summary>
    public InvoicesViewModel InvoicesVM => _invoicesVM ??= _viewModelFactory.Create<InvoicesViewModel>();

    /// <summary>
    /// Gets the lazily initialized <see cref="TireHotelViewModel"/> instance for managing seasonal tire storage and vehicle servicing.
    /// </summary>
    public TireHotelViewModel TireHotelVM => _tireHotelVM ??= _viewModelFactory.Create<TireHotelViewModel>();

    /// <summary>
    /// Gets the lazily initialized <see cref="TimeTrackingViewModel"/> instance for logging billable work hours and activities.
    /// </summary>
    public TimeTrackingViewModel TimeTrackingVM =>
        _timeTrackingVM ??= _viewModelFactory.Create<TimeTrackingViewModel>();

    /// <summary>
    /// Gets the lazily initialized <see cref="LocalizationAdminViewModel"/> instance for administering localized strings and translations.
    /// </summary>
    public LocalizationAdminViewModel LocalizationAdminVM =>
        _localizationAdminVM ??= _viewModelFactory.Create<LocalizationAdminViewModel>();

    /// <summary>
    /// Gets or sets the actively displayed feature view model rendered in the shell workspace region.
    /// </summary>
    [ObservableProperty] public partial ViewModelBase? CurrentViewModel { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the navigation sidebar is collapsed into icon-only mode.
    /// </summary>
    [ObservableProperty] public partial bool IsSidebarCollapsed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the current operator has completed authentication.
    /// </summary>
    [ObservableProperty] public partial bool IsAuthenticated { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the session is currently locked due to user inactivity or explicit request.
    /// </summary>
    [ObservableProperty] public partial bool IsSessionLocked { get; set; }

    /// <summary>
    /// Gets or sets the display name or full name of the currently authenticated user.
    /// </summary>
    [ObservableProperty] public partial string CurrentUsername { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary role description assigned to the currently authenticated user.
    /// </summary>
    [ObservableProperty] public partial string CurrentUserRole { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of available tenant organizations accessible to the operator.
    /// </summary>
    [ObservableProperty] public partial ObservableCollection<TenantDto> Tenants { get; set; } = [];

    /// <summary>
    /// Gets or sets the currently selected tenant organization context.
    /// </summary>
    [ObservableProperty] public partial TenantDto? CurrentTenant { get; set; }

    /// <summary>
    /// Gets or sets the list of supported UI culture language codes.
    /// </summary>
    [ObservableProperty] public partial ObservableCollection<string> Languages { get; set; } = ["it", "de", "fr", "en"];

    /// <summary>
    /// Gets or sets the current active culture/language code applied to UI localizations.
    /// </summary>
    [ObservableProperty] public partial string CurrentLanguage { get; set; } = "it";

    /// <summary>
    /// Gets or sets a value indicating whether the application is displaying a dark visual theme.
    /// </summary>
    [ObservableProperty] public partial bool IsDarkTheme { get; set; } = true;

    /// <summary>
    /// Initializes authentication states, available tenant lists, and current localization settings.
    /// </summary>
    /// <returns>A task representing the asynchronous initialization workflow.</returns>
    public override async Task InitializeAsync()
    {
        await LoginVM.InitializeAsync();
        Tenants = LoginVM.Tenants;
        CurrentTenant = LoginVM.SelectedTenant;
        CurrentLanguage = _localizationService.CurrentLocale;
    }

    /// <summary>
    /// Resets persisted docking layouts to default system templates after user confirmation.
    /// </summary>
    /// <returns>A task representing the asynchronous layout reset and event dispatching operation.</returns>
    [RelayCommand]
    private async Task ResetLayoutAsync()
    {
        var result = MessageBoxService.ShowMessage(
            messageBoxText:
            "Sei sicuro di voler ripristinare il layout predefinito?\nTutte le personalizzazioni delle finestre andranno perse.",
            caption: "Ripristino Layout",
            button: MessageButton.YesNo,
            icon: MessageIcon.Question,
            defaultResult: MessageResult.No);

        if (result != MessageResult.Yes)
        {
            return;
        }

        await _userStateService.ResetStateAsync();

        if (RequestResetLayoutView != null)
        {
            await RequestResetLayoutView.Invoke();
        }

        _ = MessageBoxService.ShowMessage(
            messageBoxText: "Il layout iniziale è stato ripristinato con successo.",
            caption: "Operazione completata",
            button: MessageButton.OK,
            icon: MessageIcon.Information);
    }

    /// <summary>
    /// Activates the executive dashboard module and triggers its asynchronous data loading.
    /// </summary>
    /// <returns>A task representing the navigation and initialization sequence.</returns>
    [RelayCommand]
    public async Task NavigateToDashboardAsync()
    {
        CurrentViewModel = DashboardVM;
        await DashboardVM.InitializeAsync();
    }

    /// <summary>
    /// Activates the clients management module and loads customer records asynchronously.
    /// </summary>
    /// <returns>A task representing the navigation and initialization sequence.</returns>
    [RelayCommand]
    public async Task NavigateToClientsAsync()
    {
        CurrentViewModel = ClientsVM;
        await ClientsVM.InitializeAsync();
    }

    /// <summary>
    /// Activates the electronic invoicing module and loads active invoice records asynchronously.
    /// </summary>
    /// <returns>A task representing the navigation and initialization sequence.</returns>
    [RelayCommand]
    public async Task NavigateToInvoicesAsync()
    {
        CurrentViewModel = InvoicesVM;
        await InvoicesVM.InitializeAsync();
    }

    /// <summary>
    /// Activates the tire hotel management module and loads storage records asynchronously.
    /// </summary>
    /// <returns>A task representing the navigation and initialization sequence.</returns>
    [RelayCommand]
    public async Task NavigateToTireHotelAsync()
    {
        CurrentViewModel = TireHotelVM;
        await TireHotelVM.InitializeAsync();
    }

    /// <summary>
    /// Activates the billable time and activity tracking module asynchronously.
    /// </summary>
    /// <returns>A task representing the navigation and initialization sequence.</returns>
    [RelayCommand]
    public async Task NavigateToTimeTrackingAsync()
    {
        CurrentViewModel = TimeTrackingVM;
        await TimeTrackingVM.InitializeAsync();
    }

    /// <summary>
    /// Activates the localization administrator module for maintaining localized strings.
    /// </summary>
    /// <returns>A task representing the navigation and initialization sequence.</returns>
    [RelayCommand]
    public async Task NavigateToLocalizationAdminAsync()
    {
        CurrentViewModel = LocalizationAdminVM;
        await LocalizationAdminVM.InitializeAsync();
    }

    /// <summary>
    /// Toggles the collapsed visual state of the main navigation sidebar.
    /// </summary>
    [RelayCommand]
    public void ToggleSidebar() => IsSidebarCollapsed = !IsSidebarCollapsed;

    /// <summary>
    /// Toggles between light and dark application themes via <see cref="ThemeManager"/>.
    /// </summary>
    [RelayCommand]
    public void ToggleTheme()
    {
        ThemeManager.Instance.ToggleTheme();
        IsDarkTheme = ThemeManager.Instance.IsDarkTheme;
    }

    /// <summary>
    /// Changes the active application locale and updates translated UI string resources dynamically.
    /// </summary>
    /// <param name="locale">The ISO culture code to switch to (e.g. "it", "de", "en").</param>
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
    /// Dispatches a tenant switch CQRS command to update tenant context and reloads the active view.
    /// </summary>
    /// <param name="tenant">The target tenant descriptor to transition context to.</param>
    /// <returns>A task representing the asynchronous tenant transition and view refresh.</returns>
    [RelayCommand]
    public async Task SwitchTenantAsync(
        TenantDto? tenant)
    {
        if (tenant == null || tenant.Id == _tenantProvider.CurrentTenantId)
        {
            return;
        }

        IsBusy = true;
        try
        {
            _ = await _mediator.Send(new SwitchTenantCommand(tenant.Id));
            CurrentTenant = tenant;

            if (CurrentViewModel != null)
            {
                await CurrentViewModel.InitializeAsync();
            }
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
    /// Locks the current interactive workspace session requiring operator re-authentication.
    /// </summary>
    [RelayCommand]
    public void LockSession() => IsSessionLocked = true;

    /// <summary>
    /// Unlocks the current session and resets the inactivity timeout timer.
    /// </summary>
    [RelayCommand]
    public void UnlockSession()
    {
        IsSessionLocked = false;
        _inactivityTimer.Stop();
        _inactivityTimer.Start();
    }

    /// <summary>
    /// Logs out the current user, clears security and tenant credentials, resets cached view models, and stops timers.
    /// </summary>
    [RelayCommand]
    public void Logout()
    {
        IsAuthenticated = false;
        IsSessionLocked = false;
        _inactivityTimer.Stop();
        _currentUserService.Clear();

        _dashboardVM = null;
        _clientsVM = null;
        _invoicesVM = null;
        _tireHotelVM = null;
        _timeTrackingVM = null;
        _localizationAdminVM = null;
        CurrentViewModel = null;
    }

    /// <summary>
    /// Handles the successful login event by storing session claims, starting the inactivity timer,
    /// and navigating to the executive dashboard.
    /// </summary>
    /// <param name="user">The authenticated user identity data transfer object.</param>
    private void OnLoginSuccessful(
        UserDto user)
    {
        IsAuthenticated = true;
        IsSessionLocked = false;
        CurrentUsername = user.FullName;
        CurrentUserRole = user.Role.ToString();

        CurrentTenant = Tenants.FirstOrDefault(t => t.Id == user.TenantId);
        _inactivityTimer.Start();

        _ = NavigateToDashboardAsync();
    }
}
