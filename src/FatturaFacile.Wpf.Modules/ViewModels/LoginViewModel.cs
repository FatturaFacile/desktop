// <copyright file="LoginViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Administration;

using MediatR;

namespace FatturaFacile.ViewModels;

/// <summary>
/// Handles user authentication, credential validation, and tenant selection within a multi-tenant enterprise context.
/// </summary>
/// <param name="mediator">The MediatR sender used to execute authentication and tenant retrieval queries.</param>
public sealed partial class LoginViewModel(ISender mediator) : ViewModelBase
{

    /// <summary>
    /// The MediatR sender used to execute authentication and tenant retrieval queries.
    /// </summary>
    private readonly ISender mediator = mediator;

    /// <summary>
    /// Occurs when authentication succeeds, passing the authenticated <see cref="UserDto"/> profile.
    /// </summary>
    public event Action<UserDto>? LoginSuccessful;

    /// <summary>
    /// Gets or sets the username or identity credential entered by the user.
    /// </summary>
    [ObservableProperty]
    public partial string Username { get; set; } = "admin";

    /// <summary>
    /// Gets or sets the password credential entered by the user.
    /// </summary>
    [ObservableProperty]
    public partial string Password { get; set; } = "Admin123!";

    /// <summary>
    /// Gets or sets the collection of available business tenants available for login.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<TenantDto> Tenants { get; set; } = [];

    /// <summary>
    /// Gets or sets the tenant/organization selected for the authentication session, or <see langword="null"/> if none is chosen.
    /// </summary>
    [ObservableProperty]
    public partial TenantDto? SelectedTenant { get; set; }

    /// <summary>
    /// Asynchronously initializes the ViewModel by querying the list of available tenants.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous initialization operation.</returns>
    public override async Task InitializeAsync()
    {
        IsBusy = true;

        try
        {
            var tenantList = await mediator.Send(request: new GetTenantsQuery());
            Tenants        = [with(collection: tenantList)];
            SelectedTenant = Tenants.FirstOrDefault();
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
    /// Dispatches a <see cref="LoginCommand"/> to validate user credentials against the selected tenant and raises <see cref="LoginSuccessful"/> upon success.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous login operation.</returns>
    [RelayCommand]
    public async Task LoginAsync()
    {
        if (SelectedTenant == null)
        {
            ErrorMessage = "Selezionare un'azienda / Tenant.";

            return;
        }

        IsBusy       = true;
        ErrorMessage = null;

        try
        {
            var user = await mediator.Send(
                                           request: new LoginCommand(
                                                                     Username: Username,
                                                                     Password: Password,
                                                                     TenantId: SelectedTenant.Id));

            LoginSuccessful?.Invoke(obj: user);
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

}
