// <copyright file="ProductsViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Products;
using FatturaFacile.ViewModels;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// View model governing the product and service catalog view, displaying billable inventory items and unit prices.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ProductsViewModel"/> class.
/// </remarks>
/// <param name="mediator">The MediatR sender instance.</param>
public partial class ProductsViewModel(ISender mediator) : ViewModelBase
{
    private readonly ISender _mediator = mediator;

    [ObservableProperty]
    private ObservableCollection<ProductDto> _products = new();

    [ObservableProperty]
    private ProductDto? _selectedProduct;


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
