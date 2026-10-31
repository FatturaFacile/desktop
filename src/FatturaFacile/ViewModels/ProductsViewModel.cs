// <copyright file="ProductsViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Products;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// View model governing the product and service catalog view, displaying billable inventory items and unit prices.
/// </summary>
public partial class ProductsViewModel : ViewModelBase
{
    private readonly ISender _mediator;

    [ObservableProperty]
    private ObservableCollection<ProductDto> _products = new();

    [ObservableProperty]
    private ProductDto? _selectedProduct;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductsViewModel"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR sender instance.</param>
    public ProductsViewModel(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <inheritdoc />
    public override async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            var list = await _mediator.Send(new GetProductsQuery());
            Products.Clear();
            foreach (var p in list) Products.Add(p);
            SelectedProduct = Products.FirstOrDefault();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
