// <copyright file="ProductsViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Products.Queries;
using FatturaFacile.Domain.Common;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// View model managing warehouse articles, product catalog, and price lists.
/// </summary>
public sealed partial class ProductsViewModel : ObservableObject
{
    private readonly ISender _sender;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private ObservableCollection<ProductDto> _products = new();

    [ObservableProperty]
    private ProductDto? _selectedProduct;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductsViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender instance.</param>
    /// <param name="currentTenantService">The current tenant context service.</param>
    /// <param name="localizationService">The localization service.</param>
    public ProductsViewModel(
        ISender sender,
        ICurrentTenantService currentTenantService,
        ILocalizationService localizationService)
    {
        _sender = sender;
        _currentTenantService = currentTenantService;
        _localizationService = localizationService;
        _statusMessage = localizationService["Common_Ready"];

        _currentTenantService.CurrentTenantChanged += (_, _) => _ = OnTenantChangedAsync();
    }

    /// <summary>
    /// Loads product catalog for active tenant.
    /// </summary>
    /// <returns>A task representing the load operation.</returns>
    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        IsLoading = true;
        StatusMessage = _localizationService["Common_Loading"];
        try
        {
            var products = await _sender.Send(new GetProductsQuery());
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }

            SelectedProduct = Products.FirstOrDefault();
            StatusMessage = $"{Products.Count} {_localizationService["Nav_Products"]}.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"{_localizationService["Common_Error"]}: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OnTenantChangedAsync()
    {
        SelectedProduct = null;
        Products.Clear();
        await LoadProductsAsync();
    }
}
