// <copyright file="MainViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DevExpress.Mvvm;

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Administration;
using FatturaFacile.Wpf.Common.Contracts;
using FatturaFacile.Wpf.Common.Services;

using MediatR;

namespace FatturaFacile.ViewModels;

/// <summary>
/// Root view model for the shell window managing navigation, authentication lifecycle,
/// multi-tenant context switching, localization selection, and workspace layout commands.
/// </summary>
internal sealed partial class MainViewModel : ViewModelBase, ISupportServices
{

    /// <summary>
    /// The CQRS mediator sender for dispatching commands and queries.
    /// </summary>
    private readonly ISender _mediator;

    /// <summary>
    /// The provider service supplying the active tenant context.
    /// </summary>
    private readonly ITenantProvider _tenantProvider;

    /// <summary>
    /// The service tracking the currently authenticated user session.
    /// </summary>
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// The localization service for managing multi-language UI strings.
    /// </summary>
    private readonly ILocalizationService _localizationService;

    /// <summary>
    /// The persistence service managing user window and workspace layout states.
    /// </summary>
    private readonly IUserStateService _userStateService;

    /// <summary>
    /// The factory resolving feature view models on demand.
    /// </summary>
    private readonly IViewModelFactory _viewModelFactory;

    /// <summary>
    /// The dispatcher timer triggering automatic session locking upon inactivity.
    /// </summary>
    private readonly DispatcherTimer _inactivityTimer;

    /// <summary>
    /// The internal service container managing registered UI services.
    /// </summary>
    private readonly IServiceContainer _serviceContainer = new ServiceContainer(owner: null);

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

        _inactivityTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(minutes: 30),
        };

        _inactivityTimer.Tick += (s, e) => LockSession();
    }

    // Local cache for on-demand resolved modules

    /// <summary>
    /// Cached instance of the dashboard view model.
    /// </summary>

    /// <summary>
    /// Cached instance of the login view model.
    /// </summary>

    /// <summary>
    /// Occurs when a request to reset the view's visual docking layout to factory defaults is triggered.
    /// </summary>
    public event Func<Task>? RequestResetLayoutView;

    /// <summary>
    /// Gets the service container providing access to DevExpress UI services such as dialog and message box services.
    /// </summary>
    IServiceContainer ISupportServices.ServiceContainer => _serviceContainer;

    // Lazy on-demand resolution via IViewModelFactory with local cache

    /// <summary>
    /// Gets the lazily initialized <see cref="LoginViewModel"/> instance responsible for user authentication and tenant selection.
    /// </summary>
    public LoginViewModel LoginVM => field ??= _viewModelFactory.Create<LoginViewModel>();

    /// <summary>
    /// Gets the lazily initialized <see cref="DashboardViewModel"/> instance displaying executive KPIs and analytics.
    /// </summary>
    [AllowNull]
    public DashboardViewModel DashboardVM { get => field ??= _viewModelFactory.Create<DashboardViewModel>(); private set; }

    /// <summary>
    /// Gets the lazily initialized <see cref="ClientsViewModel"/> instance for managing customer directories and contacts.
    /// </summary>
    [AllowNull]
    public ClientsViewModel ClientsVM { get => field ??= _viewModelFactory.Create<ClientsViewModel>(); private set; }

    /// <summary>
    /// Gets the lazily initialized <see cref="InvoicesViewModel"/> instance for invoice generation, lifecycle tracking, and SDI compliance.
    /// </summary>
    [AllowNull]
    public InvoicesViewModel InvoicesVM { get => field ??= _viewModelFactory.Create<InvoicesViewModel>(); private set; }

    /// <summary>
    /// Gets the lazily initialized <see cref="TireHotelViewModel"/> instance for managing seasonal tire storage and vehicle servicing.
    /// </summary>
    [AllowNull]
    public TireHotelViewModel TireHotelVM { get => field ??= _viewModelFactory.Create<TireHotelViewModel>(); private set; }

    /// <summary>
    /// Gets the lazily initialized <see cref="TimeTrackingViewModel"/> instance for logging billable work hours and activities.
    /// </summary>
    [AllowNull]
    public TimeTrackingViewModel TimeTrackingVM
    {
        get =>
        field ??= _viewModelFactory.Create<TimeTrackingViewModel>(); private set;
    }

    /// <summary>
    /// Gets the lazily initialized <see cref="LocalizationAdminViewModel"/> instance for administering localized strings and translations.
    /// </summary>
    [AllowNull]
    public LocalizationAdminViewModel LocalizationAdminVM
    {
        get =>
        field ??= _viewModelFactory.Create<LocalizationAdminViewModel>(); private set;
    }

    /// <summary>
    /// Gets or sets the actively displayed feature view model rendered in the shell workspace region.
    /// </summary>
    [ObservableProperty]
    public partial ViewModelBase? CurrentViewModel { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the navigation sidebar is collapsed into icon-only mode.
    /// </summary>
    [ObservableProperty]
    public partial bool IsSidebarCollapsed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the current operator has completed authentication.
    /// </summary>
    [ObservableProperty]
    public partial bool IsAuthenticated { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the session is currently locked due to user inactivity or explicit request.
    /// </summary>
    [ObservableProperty]
    public partial bool IsSessionLocked { get; set; }

    /// <summary>
    /// Gets or sets the display name or full name of the currently authenticated user.
    /// </summary>
    [ObservableProperty]
    public partial string CurrentUsername { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary role description assigned to the currently authenticated user.
    /// </summary>
    [ObservableProperty]
    public partial string CurrentUserRole { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of available tenant organizations accessible to the operator.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<TenantDto> Tenants { get; set; } = [];

    /// <summary>
    /// Gets or sets the currently selected tenant organization context.
    /// </summary>
    [ObservableProperty]
    public partial TenantDto? CurrentTenant { get; set; }

    /// <summary>
    /// Gets or sets the list of supported UI culture language codes.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<string> Languages { get; set; } = ["it", "de", "fr", "en"];

    /// <summary>
    /// Gets or sets the current active culture/language code applied to UI localizations.
    /// </summary>
    [ObservableProperty]
    public partial string CurrentLanguage { get; set; } = "it";

    /// <summary>
    /// Gets or sets a value indicating whether the application is displaying a dark visual theme.
    /// </summary>
    [ObservableProperty]
    public partial bool IsDarkTheme { get; set; } = true;

    /// <summary>
    /// Gets the message box service resolved from the service container.
    /// </summary>
    private IMessageBoxService MessageBoxService => _serviceContainer.GetService<IMessageBoxService>();

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
    /// Activates the executive dashboard module and triggers its asynchronous data loading.
    /// </summary>
    /// <returns>A task representing the navigation and initialization sequence.</returns>
    [RelayCommand]
    public Task NavigateToDashboardAsync()
    {
        CurrentViewModel = DashboardVM;
        return DashboardVM.InitializeAsync();
    }

    /// <summary>
    /// Activates the clients management module and loads customer records asynchronously.
    /// </summary>
    /// <returns>A task representing the navigation and initialization sequence.</returns>
    [RelayCommand]
    public Task NavigateToClientsAsync()
    {
        CurrentViewModel = ClientsVM;
        return ClientsVM.InitializeAsync();
    }

    /// <summary>
    /// Activates the electronic invoicing module and loads active invoice records asynchronously.
    /// </summary>
    /// <returns>A task representing the navigation and initialization sequence.</returns>
    [RelayCommand]
    public Task NavigateToInvoicesAsync()
    {
        CurrentViewModel = InvoicesVM;
        return InvoicesVM.InitializeAsync();
    }

    /// <summary>
    /// Activates the tire hotel management module and loads storage records asynchronously.
    /// </summary>
    /// <returns>A task representing the navigation and initialization sequence.</returns>
    [RelayCommand]
    public Task NavigateToTireHotelAsync()
    {
        CurrentViewModel = TireHotelVM;
        return TireHotelVM.InitializeAsync();
    }

    /// <summary>
    /// Activates the billable time and activity tracking module asynchronously.
    /// </summary>
    /// <returns>A task representing the navigation and initialization sequence.</returns>
    [RelayCommand]
    public Task NavigateToTimeTrackingAsync()
    {
        CurrentViewModel = TimeTrackingVM;
        return TimeTrackingVM.InitializeAsync();
    }

    /// <summary>
    /// Activates the localization administrator module for maintaining localized strings.
    /// </summary>
    /// <returns>A task representing the navigation and initialization sequence.</returns>
    [RelayCommand]
    public Task NavigateToLocalizationAdminAsync()
    {
        CurrentViewModel = LocalizationAdminVM;
        return LocalizationAdminVM.InitializeAsync();
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
    public void ChangeLanguage(string locale)
    {
        if (string.IsNullOrWhiteSpace(value: locale))
        {
            return;
        }

        CurrentLanguage = locale;
        LocalizationManager.Instance.ChangeLanguage(locale: locale);
    }

    /// <summary>
    /// Dispatches a tenant switch CQRS command to update tenant context and reloads the active view.
    /// </summary>
    /// <param name="tenant">The target tenant descriptor to transition context to.</param>
    /// <returns>A task representing the asynchronous tenant transition and view refresh.</returns>
    [RelayCommand]
    public async Task SwitchTenantAsync(TenantDto? tenant)
    {
        if (tenant == null || tenant.Id == _tenantProvider.CurrentTenantId)
        {
            return;
        }

        IsBusy = true;

        try
        {
            _ = await _mediator.Send(request: new SwitchTenantCommand(TargetTenantId: tenant.Id));
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

        DashboardVM = null;
        ClientsVM = null;
        InvoicesVM = null;
        TireHotelVM = null;
        TimeTrackingVM = null;
        LocalizationAdminVM = null;
        CurrentViewModel = null;
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
                                                   "Are you sure you want to restore the default layout?\nAll window customizations will be lost.",
                                                   caption: "Reset Layout",
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
                                          messageBoxText: "The default layout has been successfully restored.",
                                          caption: "Operation Completed",
                                          button: MessageButton.OK,
                                          icon: MessageIcon.Information);
    }

    /// <summary>
    /// Handles the successful login event by storing session claims, starting the inactivity timer,
    /// and navigating to the executive dashboard.
    /// </summary>
    /// <param name="user">The authenticated user identity data transfer object.</param>
    private void OnLoginSuccessful(UserDto user)
    {
        IsAuthenticated = true;
        IsSessionLocked = false;
        CurrentUsername = user.FullName;
        CurrentUserRole = user.Role.ToString();

        CurrentTenant = Tenants.FirstOrDefault(predicate: t => t.Id == user.TenantId);
        _inactivityTimer.Start();

        _ = NavigateToDashboardAsync();
    }

}
