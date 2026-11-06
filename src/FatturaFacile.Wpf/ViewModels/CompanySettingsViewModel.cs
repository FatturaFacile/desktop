// <copyright file="CompanySettingsViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Tenants.Commands;
using FatturaFacile.Application.Features.Tenants.Queries;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Edits the master data of the active company (addresses, tax identifiers, bank accounts).
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CompanySettingsViewModel"/> class.
/// </remarks>
/// <param name="sender">The MediatR sender.</param>
/// <param name="localizationService">The localization service.</param>
internal sealed partial class CompanySettingsViewModel(ISender sender, ILocalizationService localizationService) : ObservableObject
{
    [ObservableProperty]
    private TenantDto? _company;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Gets the Italian fiscal regimes (RF01..RF19).
    /// </summary>
    public IReadOnlyList<string> TaxRegimes { get; } =
    [
        "RF01", "RF02", "RF04", "RF05", "RF06", "RF07", "RF08", "RF09", "RF10",
        "RF11", "RF12", "RF13", "RF14", "RF15", "RF16", "RF17", "RF18", "RF19",
    ];

    /// <summary>
    /// Loads the active company.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;

        try
        {
            Company = await sender.Send(new GetCurrentTenantQuery());
            StatusMessage = localizationService["Common_Ready"];
        }
        catch (Exception ex)
        {
            StatusMessage = $"{localizationService["Common_Error"]}: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Saves the company data.
    /// </summary>
    /// <returns>A task representing the save.</returns>
    [RelayCommand]
    public async Task SaveAsync()
    {
        if (Company is null)
        {
            return;
        }

        IsLoading = true;

        try
        {
            Company = await sender.Send(new UpdateTenantCommand { Tenant = Company });
            StatusMessage = localizationService["Common_Success"];
        }
        catch (Exception ex)
        {
            StatusMessage = $"{localizationService["Common_Error"]}: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
