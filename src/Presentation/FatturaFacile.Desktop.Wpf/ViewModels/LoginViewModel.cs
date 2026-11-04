using CommunityToolkit.Mvvm.Input;
using MediatR;
using FatturaFacile.Identity.Application.Commands;
using FatturaFacile.Desktop.Wpf.Localization;
using FatturaFacile.Desktop.Wpf.Services;

namespace FatturaFacile.Desktop.Wpf.ViewModels;

/// <summary>
/// Handles offline in-process user authentication using Argon2id and sets up user/tenant session contexts.
/// </summary>
public sealed class LoginViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly DesktopUserContext _userContext;
    private readonly DesktopTenantContext _tenantContext;
    private readonly IDesktopLocalizer _localizer;
    private readonly INavigationService _navigationService;

    private string _tenantSlug = "default-corp";
    private string _email = "admin@fatturafacile.ch";
    private string _password = string.Empty;
    private string? _errorMessage;

    public LoginViewModel(
        IMediator mediator,
        DesktopUserContext userContext,
        DesktopTenantContext tenantContext,
        IDesktopLocalizer localizer,
        INavigationService navigationService)
    {
        _mediator = mediator;
        _userContext = userContext;
        _tenantContext = tenantContext;
        _localizer = localizer;
        _navigationService = navigationService;
        Title = _localizer["Login.Title"];

        SignInCommand = new AsyncRelayCommand(ExecuteSignInAsync, () => !IsBusy);
    }

    public string TenantSlug
    {
        get => _tenantSlug;
        set => SetProperty(ref _tenantSlug, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public IDesktopLocalizer Localizer => _localizer;
    public IAsyncRelayCommand SignInCommand { get; }

    public event Action? LoginSucceeded;

    private async Task ExecuteSignInAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        StatusMessage = _localizer["Login.SigningIn"];

        try
        {
            var command = new LoginUserCommand(
                TenantSlug.Trim(),
                Email.Trim(),
                Password,
                "127.0.0.1",
                "FatturaFacile-Desktop-Wpf/1.0"
            );

            var result = await _mediator.Send(command);

            if (result.IsSuccess && result.Value != null)
            {
                var auth = result.Value;
                _tenantContext.SetTenant(auth.TenantId, auth.UserId.ToString());
                _userContext.SetUser(auth.UserId, auth.TenantId, auth.PreferredLanguage);
                _localizer.SetCulture(auth.PreferredLanguage);

                StatusMessage = null;
                LoginSucceeded?.Invoke();
                _navigationService.NavigateTo<DashboardViewModel>();
            }
            else
            {
                ErrorMessage = result.Error ?? _localizer["Login.Error"];
                StatusMessage = null;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            StatusMessage = null;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
