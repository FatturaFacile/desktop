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

public sealed partial class MainViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILocalizationService _localizationService;
    private readonly DispatcherTimer _inactivityTimer;

    /// <summary>
    /// ViewModels
    /// </summary>
    public DashboardViewModel DashboardVM { get; }
    public ClientsViewModel ClientsVM { get; }
    public InvoicesViewModel InvoicesVM { get; }
    public TireHotelViewModel TireHotelVM { get; }
    public TimeTrackingViewModel TimeTrackingVM { get; }
    public LocalizationAdminViewModel LocalizationAdminVM { get; }
    public LoginViewModel LoginVM { get; }

    [ObservableProperty]
    public partial ViewModelBase CurrentViewModel { get; set; }

    [ObservableProperty]
    public partial bool IsSidebarCollapsed { get; set; }

    [ObservableProperty]
    public partial bool IsAuthenticated { get; set; }

    [ObservableProperty]
    public partial bool IsSessionLocked { get; set; }

    [ObservableProperty]
    public partial string CurrentUsername { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CurrentUserRole { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<TenantDto> Tenants { get; set; } = [];

    [ObservableProperty]
    public partial TenantDto? CurrentTenant { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<string> Languages { get; set; } = ["it", "de", "fr", "en"];

    [ObservableProperty]
    public partial string CurrentLanguage { get; set; } = "it";

    [ObservableProperty]
    public partial bool IsDarkTheme { get; set; } = true;

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
        _mediator = mediator;
        _tenantProvider = tenantProvider;
        _currentUserService = currentUserService;
        _localizationService = localizationService;

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
        _inactivityTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(30) };
        _inactivityTimer.Tick += (s, e) => LockSession();
    }

    public override async Task InitializeAsync()
    {
        await LoginVM.InitializeAsync();
        Tenants = LoginVM.Tenants;
        CurrentTenant = LoginVM.SelectedTenant;

        CurrentLanguage = _localizationService.CurrentLocale;
    }

    private Task OnLoginSuccessful(UserDto user)
    {
        IsAuthenticated = true;
        IsSessionLocked = false;
        CurrentUsername = user.FullName;
        CurrentUserRole = user.Role.ToString();

        CurrentTenant = Tenants.FirstOrDefault(t => t.Id == user.TenantId);

        _inactivityTimer.Start();

        return NavigateToDashboardAsync();
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
    public void ChangeLanguage(string locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
        {
            return;
        }

        CurrentLanguage = locale;
        LocalizationManager.Instance.ChangeLanguage(locale);
    }

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
            var switched = await _mediator.Send(new SwitchTenantCommand(tenant.Id));
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
    }

    /// <summary>
    /// Navigation Commands
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public Task NavigateToDashboardAsync()
    {
        CurrentViewModel = DashboardVM;
        return DashboardVM.InitializeAsync();
    }

    [RelayCommand]
    public Task NavigateToClientsAsync()
    {
        CurrentViewModel = ClientsVM;
        return ClientsVM.InitializeAsync();
    }

    [RelayCommand]
    public Task NavigateToInvoicesAsync()
    {
        CurrentViewModel = InvoicesVM;
        return InvoicesVM.InitializeAsync();
    }

    [RelayCommand]
    public Task NavigateToTireHotelAsync()
    {
        CurrentViewModel = TireHotelVM;
        return TireHotelVM.InitializeAsync();
    }

    [RelayCommand]
    public Task NavigateToTimeTrackingAsync()
    {
        CurrentViewModel = TimeTrackingVM;
        return TimeTrackingVM.InitializeAsync();
    }

    [RelayCommand]
    public Task NavigateToLocalizationAdminAsync()
    {
        CurrentViewModel = LocalizationAdminVM;
        return LocalizationAdminVM.InitializeAsync();
    }
}
