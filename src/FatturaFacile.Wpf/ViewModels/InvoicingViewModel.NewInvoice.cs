// <copyright file="InvoicingViewModel.NewInvoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Invoicing.Commands;
using FatturaFacile.Application.Features.Records;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Wpf.ViewModels;

/// <content>
/// New-invoice editor of the invoicing view model.
/// </content>
internal sealed partial class InvoicingViewModel
{
    [ObservableProperty]
    private CreateInvoiceCommand _newInvoice = new();

    [ObservableProperty]
    private InvoiceItemDto _newItem = new();

    [ObservableProperty]
    private InvoiceItemDto? _selectedNewItem;

    [ObservableProperty]
    private Guid? _newItemProductId;

    /// <summary>
    /// Gets the lines of the invoice being written.
    /// </summary>
    public ObservableCollection<InvoiceItemDto> NewItems { get; } = [];

    /// <summary>
    /// Gets the customers that can be invoiced.
    /// </summary>
    public ObservableCollection<Contact> Customers { get; } = [];

    /// <summary>
    /// Gets the active products of the catalogue.
    /// </summary>
    public ObservableCollection<Product> Products { get; } = [];

    /// <summary>
    /// Loads the invoices and prepares the new-invoice editor.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    public async Task LoadAllAsync()
    {
        await LoadInvoicesAsync();
        await PrepareNewInvoiceAsync();
    }

    /// <summary>
    /// Prepares an empty invoice, reloading customers and products.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    [RelayCommand]
    public async Task PrepareNewInvoiceAsync()
    {
        try
        {
            var contacts = await _sender.Send(new GetRecordsQuery<Contact>());
            var products = await _sender.Send(new GetRecordsQuery<Product>());
            Customers.Clear();
            foreach (var contact in contacts.Where(static c => !string.Equals(c.Type, "supplier", StringComparison.OrdinalIgnoreCase)).OrderBy(static c => c.DisplayName, StringComparer.CurrentCultureIgnoreCase))
            {
                Customers.Add(contact);
            }

            Products.Clear();
            foreach (var product in products.Where(static p => p.IsActive).OrderBy(static p => p.Name, StringComparer.CurrentCultureIgnoreCase))
            {
                Products.Add(product);
            }

            var year = DateTime.Today.Year;
            var sequence = Invoices.Count(i => i.Date.Year == year) + 1;
            NewInvoice = new CreateInvoiceCommand
            {
                InvoiceNumber = string.Create(CultureInfo.InvariantCulture, $"{year}-{sequence:000}"),
                Date = DateTime.Today,
                DueDate = DateTime.Today.AddDays(30),
                Currency = _currentTenantService.DefaultCurrency,
            };
            NewItems.Clear();
            NewItem = CreateEmptyItem();
            NewItemProductId = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"{_localizationService["Common_Error"]}: {ex.Message}";
        }
    }

    /// <summary>
    /// Adds the line being edited to the invoice.
    /// </summary>
    [RelayCommand]
    public void AddItem()
    {
        if (string.IsNullOrWhiteSpace(NewItem.Description) || NewItem.Quantity == 0m)
        {
            StatusMessage = _localizationService["Common_Error"];
            return;
        }

        NewItem.ProductId = NewItemProductId;
        NewItem.LineTotal = Math.Round(NewItem.Quantity * NewItem.UnitPrice * (1m - (NewItem.DiscountPercentage / 100m)), 2, MidpointRounding.AwayFromZero);
        NewItems.Add(NewItem);
        NewItem = CreateEmptyItem();
        NewItemProductId = null;
    }

    /// <summary>
    /// Removes the selected line.
    /// </summary>
    [RelayCommand]
    public void RemoveItem()
    {
        if (SelectedNewItem is not null)
        {
            _ = NewItems.Remove(SelectedNewItem);
        }
    }

    /// <summary>
    /// Creates the invoice as a draft.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    [RelayCommand]
    public async Task CreateInvoiceAsync()
    {
        IsLoading = true;
        try
        {
            NewInvoice.Items = [.. NewItems];
            var created = await _sender.Send(NewInvoice);
            await LoadInvoicesAsync();
            SelectedInvoice = Invoices.FirstOrDefault(i => i.Id == created.Id) ?? SelectedInvoice;
            await PrepareNewInvoiceAsync();
            StatusMessage = _localizationService["Invoicing_Created"];
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

    partial void OnNewItemProductIdChanged(Guid? value)
    {
        var product = value is null ? null : Products.FirstOrDefault(p => p.Id == value.Value);
        if (product is null)
        {
            return;
        }

        NewItem = new InvoiceItemDto
        {
            ProductId = product.Id,
            Description = product.Name,
            Quantity = NewItem.Quantity == 0m ? 1m : NewItem.Quantity,
            UnitPrice = product.SalePrice,
            TaxRate = product.TaxRate,
            Uom = string.IsNullOrWhiteSpace(product.UnitMeasure) ? "EA" : product.UnitMeasure,
        };
    }

    private InvoiceItemDto CreateEmptyItem() => new()
    {
        Quantity = 1m,
        TaxRate = IsSwiss ? 8.1m : 22m,
    };
}
