// <copyright file="ProductsViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Records;
using FatturaFacile.Application.Features.Records;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Wpf.ViewModels.Payroll;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Products and services: list, creation and editing (prices, VAT rate, stock settings).
/// </summary>
internal sealed partial class ProductsViewModel : PayrollSectionViewModel
{
    [ObservableProperty]
    private Product? _selectedProduct;

    [ObservableProperty]
    private Product _editing = NewProduct();

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductsViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender instance.</param>
    /// <param name="currentTenantService">The current tenant context service.</param>
    /// <param name="localizationService">The localization service.</param>
    public ProductsViewModel(ISender sender, ICurrentTenantService currentTenantService, ILocalizationService localizationService)
        : base(sender, localizationService)
    {
        ArgumentNullException.ThrowIfNull(currentTenantService);
        currentTenantService.CurrentTenantChanged += (_, _) => _ = LoadAsync();
    }

    /// <summary>
    /// Gets the records.
    /// </summary>
    public ObservableCollection<Product> Products { get; } = [];

    /// <summary>
    /// Loads the records (used by the shell navigation).
    /// </summary>
    /// <returns>A task representing the load.</returns>
    public Task LoadProductsAsync() => LoadAsync();

    /// <inheritdoc/>
    [RelayCommand]
    public override Task LoadAsync() => RunAsync(async () =>
    {
        var selectedId = SelectedProduct?.Id;
        var records = await Sender.Send(new GetRecordsQuery<Product>());
        Products.Clear();

        foreach (var record in records.OrderBy(static r => r.Name, StringComparer.CurrentCultureIgnoreCase))
        {
            Products.Add(record);
        }

        SelectedProduct = Products.FirstOrDefault(r => r.Id == selectedId) ?? Products.FirstOrDefault();
    });

    private static Product NewProduct() => new() { Id = Guid.Empty, UnitMeasure = "PZ", TaxRate = 8.1m };

    partial void OnSelectedProductChanged(Product? value) => Editing = value is null ? NewProduct() : RecordCopier.Clone(value);

    [RelayCommand]
    private void New()
    {
        SelectedProduct = null;
        Editing = NewProduct();
    }

    [RelayCommand]
    private Task SaveAsync() => RunAsync(
        async () =>
        {
            var id = await Sender.Send(new SaveRecordCommand<Product>(Editing));
            await LoadAsync();
            SelectedProduct = Products.FirstOrDefault(r => r.Id == id);
        },
        "Common_Saved");

    [RelayCommand]
    private Task DeleteAsync() => SelectedProduct is null
        ? Task.CompletedTask
        : RunAsync(
            async () =>
            {
                _ = await Sender.Send(new DeleteRecordCommand<Product>(SelectedProduct.Id));
                await LoadAsync();
            },
            "Common_Deleted");
}
