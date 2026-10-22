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
/// Represents main view model.
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

    public event Func<Task>? RequestResetLayoutView;
    private readonly IServiceContainer _serviceContainer = new ServiceContainer(null);
    IServiceContainer ISupportServices.ServiceContainer => _serviceContainer;
    private IMessageBoxService MessageBoxService => _serviceContainer.GetService<IMessageBoxService>();

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
    public LoginViewModel LoginVM => _loginVM ??= _viewModelFactory.Create<LoginViewModel>();
    public DashboardViewModel DashboardVM => _dashboardVM ??= _viewModelFactory.Create<DashboardViewModel>();
    public ClientsViewModel ClientsVM => _clientsVM ??= _viewModelFactory.Create<ClientsViewModel>();
    public InvoicesViewModel InvoicesVM => _invoicesVM ??= _viewModelFactory.Create<InvoicesViewModel>();
    public TireHotelViewModel TireHotelVM => _tireHotelVM ??= _viewModelFactory.Create<TireHotelViewModel>();

    public TimeTrackingViewModel TimeTrackingVM =>
        _timeTrackingVM ??= _viewModelFactory.Create<TimeTrackingViewModel>();

    public LocalizationAdminViewModel LocalizationAdminVM =>
        _localizationAdminVM ??= _viewModelFactory.Create<LocalizationAdminViewModel>();

    [ObservableProperty] public partial ViewModelBase? CurrentViewModel { get; set; }

    [ObservableProperty] public partial bool IsSidebarCollapsed { get; set; }

    [ObservableProperty] public partial bool IsAuthenticated { get; set; }

    [ObservableProperty] public partial bool IsSessionLocked { get; set; }

    [ObservableProperty] public partial string CurrentUsername { get; set; } = string.Empty;

    [ObservableProperty] public partial string CurrentUserRole { get; set; } = string.Empty;

    [ObservableProperty] public partial ObservableCollection<TenantDto> Tenants { get; set; } = [];

    [ObservableProperty] public partial TenantDto? CurrentTenant { get; set; }

    [ObservableProperty] public partial ObservableCollection<string> Languages { get; set; } = ["it", "de", "fr", "en"];

    [ObservableProperty] public partial string CurrentLanguage { get; set; } = "it";

    [ObservableProperty] public partial bool IsDarkTheme { get; set; } = true;

    public override async Task InitializeAsync()
    {
        await LoginVM.InitializeAsync();
        Tenants = LoginVM.Tenants;
        CurrentTenant = LoginVM.SelectedTenant;
        CurrentLanguage = _localizationService.CurrentLocale;
    }

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

    [RelayCommand]
    public async Task NavigateToDashboardAsync()
    {
        CurrentViewModel = DashboardVM;
        await DashboardVM.InitializeAsync();
    }

    [RelayCommand]
    public async Task NavigateToClientsAsync()
    {
        CurrentViewModel = ClientsVM;
        await ClientsVM.InitializeAsync();
    }

    [RelayCommand]
    public async Task NavigateToInvoicesAsync()
    {
        CurrentViewModel = InvoicesVM;
        await InvoicesVM.InitializeAsync();
    }

    [RelayCommand]
    public async Task NavigateToTireHotelAsync()
    {
        CurrentViewModel = TireHotelVM;
        await TireHotelVM.InitializeAsync();
    }

    [RelayCommand]
    public async Task NavigateToTimeTrackingAsync()
    {
        CurrentViewModel = TimeTrackingVM;
        await TimeTrackingVM.InitializeAsync();
    }

    [RelayCommand]
    public async Task NavigateToLocalizationAdminAsync()
    {
        CurrentViewModel = LocalizationAdminVM;
        await LocalizationAdminVM.InitializeAsync();
    }

    [RelayCommand]
    public void ToggleSidebar() => IsSidebarCollapsed = !IsSidebarCollapsed;

    [RelayCommand]
    public void ToggleTheme()
    {
        ThemeManager.Instance.ToggleTheme();
        IsDarkTheme = ThemeManager.Instance.IsDarkTheme;
    }

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

    [RelayCommand]
    public void LockSession() => IsSessionLocked = true;

    [RelayCommand]
    public void UnlockSession()
    {
        IsSessionLocked = false;
        _inactivityTimer.Stop();
        _inactivityTimer.Start();
    }

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
