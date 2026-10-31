// <copyright file="CreateInvoiceViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Contacts;
using FatturaFacile.Application.Features.Invoices;
using FatturaFacile.Application.Features.Products;
using FatturaFacile.Application.Ports;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Services;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// View model governing the interactive invoice creation workflow, including real-time subtotal/VAT calculation,
/// dynamic line item management, customer selection, and command persistence.
/// </summary>
public partial class CreateInvoiceViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly DocumentTotalsCalculator _calculator = new();

    [ObservableProperty]
    private ObservableCollection<CustomerDto> _customers = new();

    [ObservableProperty]
    private CustomerDto? _selectedCustomer;

    [ObservableProperty]
    private ObservableCollection<ProductDto> _products = new();

    [ObservableProperty]
    private DateTime _date = DateTime.Today;

    [ObservableProperty]
    private DateTime _dueDate = DateTime.Today.AddDays(30);

    [ObservableProperty]
    private string _currency = "CHF";

    [ObservableProperty]
    private ObservableCollection<InvoiceItemInputViewModel> _items = new();

    [ObservableProperty]
    private decimal _totalNet;

    [ObservableProperty]
    private decimal _totalTax;

    [ObservableProperty]
    private decimal _totalGross;

    [ObservableProperty]
    private decimal _roundingAmount;

    [ObservableProperty]
    private string? _notes;

    /// <summary>
    /// Event raised after an invoice is successfully created and persisted.
    /// </summary>
    public event Action? InvoiceCreated;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateInvoiceViewModel"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR sender instance for dispatching queries and commands.</param>
    /// <param name="currentTenantService">The ambient tenant context service.</param>
    public CreateInvoiceViewModel(ISender mediator, ICurrentTenantService currentTenantService)
    {
        _mediator = mediator;
        _currentTenantService = currentTenantService;
        _currency = _currentTenantService.CurrentTenant?.DefaultCurrency ?? "CHF";

        AddItem();
    }

    /// <inheritdoc />
    public override async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            var custs = await _mediator.Send(new GetCustomersQuery());
            Customers.Clear();
            foreach (var c in custs) Customers.Add(c);
            SelectedCustomer = Customers.FirstOrDefault();

            var prods = await _mediator.Send(new GetProductsQuery());
            Products.Clear();
            foreach (var p in prods) Products.Add(p);

            Recalculate();
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Appends a new default line item row to the invoice item collection and recalculates totals.
    /// </summary>
    [RelayCommand]
    public void AddItem()
    {
        var item = new InvoiceItemInputViewModel(Recalculate)
        {
            TaxRate = Currency == "CHF" ? 8.1m : 22.0m
        };
        Items.Add(item);
        Recalculate();
    }

    /// <summary>
    /// Removes a specified line item row and updates document totals.
    /// </summary>
    /// <param name="item">The line item view model to remove.</param>
    [RelayCommand]
    public void RemoveItem(InvoiceItemInputViewModel item)
    {
        if (Items.Count > 1)
        {
            _ = Items.Remove(item);
            Recalculate();
        }
    }

    partial void OnCurrencyChanged(string value)
    {
        Recalculate();
    }

    /// <summary>
    /// Re-evaluates net totals, line taxes, Swiss QR-bill rounding adjustments, and invoice gross totals.
    /// </summary>
    public void Recalculate()
    {
        var inputs = Items.Select(i => new DocumentLineInput(
            i.ProductId,
            i.Description,
            i.Quantity,
            i.UnitPrice,
            i.TaxRate,
            i.DiscountPercentage
        )).ToList();

        var result = _calculator.Calculate(inputs, Currency);

        for (var i = 0; i < result.Items.Count && i < Items.Count; i++)
        {
            Items[i].LineTotal = result.Items[i].LineTotal;
        }

        TotalNet = result.TotalNet;
        TotalTax = result.TotalTax;
        TotalGross = result.TotalGross;
        RoundingAmount = result.RoundingAmount;
    }

    /// <summary>
    /// Validates and dispatches the <see cref="CreateInvoiceCommand"/> to persist the invoice to database storage.
    /// </summary>
    [RelayCommand]
    public async Task SaveInvoiceAsync()
    {
        if (SelectedCustomer == null)
        {
            BusyMessage = "Please select a customer.";
            return;
        }

        IsBusy = true;
        BusyMessage = "Saving invoice to SQLite...";

        try
        {
            var lineItems = Items.Select(i => new CreateInvoiceLineItemCommand(
                i.ProductId,
                i.Description,
                i.Quantity,
                i.UnitPrice,
                i.TaxRate,
                i.DiscountPercentage
            )).ToList();

            var command = new CreateInvoiceCommand(
                SelectedCustomer.Id,
                Date,
                DueDate,
                Currency,
                InvoiceType.Invoice,
                Notes,
                _currentTenantService.CurrentTenant?.Name,
                _currentTenantService.CurrentTenant?.Iban,
                _currentTenantService.CurrentTenant?.QrIban,
                lineItems
            );

            _ = await _mediator.Send(command);
            InvoiceCreated?.Invoke();
        }
        catch (Exception ex)
        {
            BusyMessage = $"Save error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
