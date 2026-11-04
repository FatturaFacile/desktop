// <copyright file="LoginViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Desktop.Wpf.Localization;
using FatturaFacile.Desktop.Wpf.Services;
using FatturaFacile.Identity.Application.Commands;
using MediatR;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR mediator.</param>
    /// <param name="userContext">The desktop user session context.</param>
    /// <param name="tenantContext">The desktop tenant session context.</param>
    /// <param name="localizer">The desktop localizer.</param>
    /// <param name="navigationService">The navigation service.</param>
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

    /// <summary>
    /// Occurs when authentication succeeds.
    /// </summary>
    public event Action? LoginSucceeded;

    /// <summary>
    /// Gets or sets the target tenant organization identifier slug.
    /// </summary>
    public string TenantSlug
    {
        get => _tenantSlug;
        set => SetProperty(ref _tenantSlug, value);
    }

    /// <summary>
    /// Gets or sets the user login email address.
    /// </summary>
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    /// <summary>
    /// Gets or sets the user password.
    /// </summary>
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    /// <summary>
    /// Gets or sets the login failure error message, if any.
    /// </summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    /// <summary>
    /// Gets the localization service instance.
    /// </summary>
    public IDesktopLocalizer Localizer => _localizer;

    /// <summary>
    /// Gets the asynchronous command that executes authentication.
    /// </summary>
    public IAsyncRelayCommand SignInCommand { get; }

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
                Password);

            var result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                ErrorMessage = result.Error;
                StatusMessage = null;
                return;
            }

            var auth = result.Value;
            _tenantContext.SetTenant(auth.TenantId, auth.UserId.ToString());
            _userContext.SetUser(auth.UserId, auth.TenantId, auth.PreferredLanguage);
            _localizer.SetCulture(auth.PreferredLanguage);

            StatusMessage = null;
            LoginSucceeded?.Invoke();
            _navigationService.NavigateTo<DashboardViewModel>();
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
