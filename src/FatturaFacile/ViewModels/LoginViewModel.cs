// <copyright file="LoginViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Administration;
using MediatR;

namespace FatturaFacile.ViewModels;

/// <summary>
/// Represents login view model.
/// </summary>
/// <param name="mediator">The mediator.</param>
public sealed partial class LoginViewModel(ISender mediator) : ViewModelBase
{
    private readonly ISender mediator = mediator;

    public event Action<UserDto>? LoginSuccessful;

    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    [ObservableProperty]
    public partial string Username { get; set; } = "admin";

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    [ObservableProperty]
    public partial string Password { get; set; } = "Admin123!";

    /// <summary>
    /// Gets or sets the tenants.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<TenantDto> Tenants { get; set; } = [];

    /// <summary>
    /// Gets or sets the selected tenant.
    /// </summary>
    [ObservableProperty]
    public partial TenantDto? SelectedTenant { get; set; }

    /// <summary>
    /// Initialize Async.
    /// </summary>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public override async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            var tenantList = await mediator.Send(new GetTenantsQuery());
            Tenants = [with(tenantList)];
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
    /// Login Async.
    /// </summary>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    [RelayCommand]
    public async Task LoginAsync()
    {
        if (SelectedTenant == null)
        {
            ErrorMessage = "Selezionare un'azienda / Tenant.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var user = await mediator.Send(
                new LoginCommand(
                    Username,
                    Password,
                    SelectedTenant.Id));
            LoginSuccessful?.Invoke(user);
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
