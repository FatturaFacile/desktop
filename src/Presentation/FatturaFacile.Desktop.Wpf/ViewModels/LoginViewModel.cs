// <copyright file="LoginViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Desktop.Wpf.Localization;
using FatturaFacile.Desktop.Wpf.Services;
using FatturaFacile.Identity.Application.Commands;
using MediatR;

namespace FatturaFacile.Desktop.Wpf.ViewModels;

/// <summary>
/// Handles offline in-process user authentication using Argon2id and sets up user/tenant session contexts.
/// </summary>
internal sealed partial class LoginViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly DesktopUserContext _userContext;
    private readonly DesktopTenantContext _tenantContext;
    private readonly INavigationService _navigationService;

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
        Localizer = localizer;
        _navigationService = navigationService;
        Title = Localizer["Login.Title"];

        SignInCommand = new AsyncRelayCommand(ExecuteSignInAsync, () => !IsBusy);
    }

    /// <summary>
    /// Occurs when authentication succeeds.
    /// </summary>
    public event EventHandler? LoginSucceeded;

    [ObservableProperty]
    public partial string TenantSlug { get; set; } = "default-corp";

    [ObservableProperty]
    public partial string Email { get; set; } = "admin@fatturafacile.ch";

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets the localization service instance.
    /// </summary>
    public IDesktopLocalizer Localizer { get; }

    /// <summary>
    /// Gets the asynchronous command that executes authentication.
    /// </summary>
    public IAsyncRelayCommand SignInCommand { get; }

    private async Task ExecuteSignInAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        StatusMessage = Localizer["Login.SigningIn"];

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
            Localizer.SetCulture(auth.PreferredLanguage);

            StatusMessage = null;
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
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
