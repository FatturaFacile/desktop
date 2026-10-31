// <copyright file="LoginViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Tenants;
using FatturaFacile.Application.Ports;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// View model controlling Windows authentication sign-in, credential validation,
/// and automated routing between single-tenant direct launch and multi-tenant selection.
/// </summary>
public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authService;
    private readonly ISender _mediator;

    [ObservableProperty]
    private string _username = Environment.UserName;

    [ObservableProperty]
    private string _domain = Environment.UserDomainName;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>
    /// Event raised when the authenticated user possesses access to multiple workspaces, requiring selection.
    /// </summary>
    public event Action<IReadOnlyList<TenantDto>>? MultipleTenantsDetected;

    /// <summary>
    /// Event raised when the user belongs to exactly one tenant workspace and was auto-selected.
    /// </summary>
    public event Action? SingleTenantAutoSelected;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
    /// </summary>
    /// <param name="authService">The Windows authentication service.</param>
    /// <param name="mediator">The MediatR sender dependency.</param>
    public LoginViewModel(IAuthenticationService authService, ISender mediator)
    {
        _authService = authService;
        _mediator = mediator;
    }

    /// <summary>
    /// Validates user credentials against Windows security authorities and verifies authorized tenant workspaces.
    /// </summary>
    [RelayCommand]
    public async Task AuthenticateAsync()
    {
        ErrorMessage = null;
        IsBusy = true;
        BusyMessage = "Verifying Windows credentials...";

        try
        {
            var authResult = await _authService.AuthenticateWindowsUserAsync(Username, Domain, Password);
            if (!authResult.IsSuccess)
            {
                ErrorMessage = authResult.ErrorMessage ?? "Invalid credentials.";
                return;
            }

            BusyMessage = "Loading accessible enterprise tenants...";
            var tenants = await _mediator.Send(new GetTenantsForUserQuery(authResult.Username ?? Username));

            if (tenants.Count == 0)
            {
                ErrorMessage = $"No tenants associated with Windows user '{Username}'.";
                return;
            }

            if (tenants.Count == 1)
            {
                // Auto-bypass single tenant
                _ = await _mediator.Send(new SelectTenantCommand(tenants[0].Id));
                SingleTenantAutoSelected?.Invoke();
            }
            else
            {
                MultipleTenantsDetected?.Invoke(tenants);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Authentication error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
