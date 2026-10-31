// <copyright file="TenantSelectionViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Tenants;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// View model controlling multi-tenant workspace selection, enabling operators to choose an active organization.
/// </summary>
public partial class TenantSelectionViewModel : ViewModelBase
{
    private readonly ISender _mediator;

    [ObservableProperty]
    private ObservableCollection<TenantDto> _tenants = new();

    [ObservableProperty]
    private TenantDto? _selectedTenant;

    /// <summary>
    /// Event raised after a tenant workspace has been selected and activated in the ambient context.
    /// </summary>
    public event Action<TenantDto>? TenantConfirmed;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantSelectionViewModel"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR sender dependency.</param>
    public TenantSelectionViewModel(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Populates the observable collection of available workspaces with authorized tenant records.
    /// </summary>
    /// <param name="tenants">The collection of authorized tenant DTOs.</param>
    public void LoadTenants(IEnumerable<TenantDto> tenants)
    {
        Tenants.Clear();
        foreach (var t in tenants)
        {
            Tenants.Add(t);
        }

        SelectedTenant = Tenants.FirstOrDefault();
    }

    /// <summary>
    /// Activates the currently highlighted tenant workspace via <see cref="SelectTenantCommand"/>.
    /// </summary>
    [RelayCommand]
    public async Task ConfirmSelectionAsync()
    {
        if (SelectedTenant == null) return;

        IsBusy = true;
        try
        {
            var dto = await _mediator.Send(new SelectTenantCommand(SelectedTenant.Id));
            TenantConfirmed?.Invoke(dto);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
