// <copyright file="InvoiceDetailViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using System.Security.Cryptography;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Desktop.Wpf.Localization;
using FatturaFacile.Desktop.Wpf.Services;
using FatturaFacile.Invoicing.Application.Commands;
using FatturaFacile.Invoicing.Application.DTOs;
using FatturaFacile.Invoicing.Domain.Enums;
using MediatR;

namespace FatturaFacile.Desktop.Wpf.ViewModels;

/// <summary>
/// View model for creating and viewing invoices with line items, Swiss 5-centimes calculation, and QR-Bill preview.
/// </summary>
internal sealed partial class InvoiceDetailViewModel : ViewModelBase
{
    private const int DefaultDuePeriodDays = 30;
    private const decimal SwissVatRate = 8.1m;
    private const decimal ItalianVatRate = 22.0m;
    private const decimal SwissFiveCentimesMultiplier = 20m;
    private const int MinInvoiceRandom = 100;
    private const int MaxInvoiceRandom = 1000;
    private const decimal HundredPercent = 100m;
    private const decimal DefaultItemUnitPrice = 100.00m;
    private const int CurrencyDecimals = 2;

    private readonly IMediator _mediator;
    private readonly DesktopTenantContext _tenantContext;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _invoiceNumber = $"INV-{DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}-{RandomNumberGenerator.GetInt32(MinInvoiceRandom, MaxInvoiceRandom).ToString(CultureInfo.InvariantCulture)}";

    [ObservableProperty]
    private DateTime _date = DateTime.Today;

    [ObservableProperty]
    private DateTime? _dueDate = DateTime.Today.AddDays(DefaultDuePeriodDays);

    [ObservableProperty]
    private string _currency = "CHF";

    [ObservableProperty]
    private Guid _contactId = Guid.NewGuid();

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private decimal _totalNet;

    [ObservableProperty]
    private decimal _totalTax;

    [ObservableProperty]
    private decimal _roundingAmount;

    [ObservableProperty]
    private decimal _totalGross;

    [ObservableProperty]
    private bool _showQrPreview;

    [ObservableProperty]
    private string _qrBillText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceDetailViewModel"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR mediator.</param>
    /// <param name="tenantContext">The desktop tenant session context.</param>
    /// <param name="localizer">The desktop localizer.</param>
    /// <param name="navigationService">The navigation service.</param>
    public InvoiceDetailViewModel(
        IMediator mediator,
        DesktopTenantContext tenantContext,
        IDesktopLocalizer localizer,
        INavigationService navigationService)
    {
        _mediator = mediator;
        _tenantContext = tenantContext;
        Localizer = localizer;
        _navigationService = navigationService;
        Title = Localizer["InvoiceDetail.Title"];

        AddItemCommand = new RelayCommand(ExecuteAddItem);
        RemoveItemCommand = new RelayCommand<InvoiceItemRowModel>(ExecuteRemoveItem);
        SaveInvoiceCommand = new AsyncRelayCommand(ExecuteSaveInvoiceAsync, () => !IsBusy);
        CancelCommand = new RelayCommand(() => _navigationService.NavigateTo<InvoiceListViewModel>());
        ToggleQrPreviewCommand = new RelayCommand(ExecuteToggleQrPreview);

        // Add a default sample line item
        const decimal sampleQuantity = 10m;
        const decimal sampleUnitPrice = 160.00m;
        Items.Add(new InvoiceItemRowModel(this)
        {
            Description = "Consulting & Software Engineering Services",
            Quantity = sampleQuantity,
            UnitPrice = sampleUnitPrice,
            TaxRate = SwissVatRate, // Swiss standard VAT rate (8.1%)
        });

        RecalculateTotals();
    }

    /// <summary>
    /// Gets the desktop localization service.
    /// </summary>
    public IDesktopLocalizer Localizer { get; }

    /// <summary>
    /// Gets the collection of editable line item rows in the invoice.
    /// </summary>
    public ObservableCollection<InvoiceItemRowModel> Items { get; } = [];

    /// <summary>
    /// Gets the command that adds a new empty item row.
    /// </summary>
    public IRelayCommand AddItemCommand { get; }

    /// <summary>
    /// Gets the command that removes a specific item row.
    /// </summary>
    public IRelayCommand<InvoiceItemRowModel> RemoveItemCommand { get; }

    /// <summary>
    /// Gets the asynchronous command that persists the invoice.
    /// </summary>
    public IAsyncRelayCommand SaveInvoiceCommand { get; }

    /// <summary>
    /// Gets the command that cancels creation and returns to the invoice list.
    /// </summary>
    public IRelayCommand CancelCommand { get; }

    /// <summary>
    /// Gets the command that toggles the QR-Bill preview panel.
    /// </summary>
    public IRelayCommand ToggleQrPreviewCommand { get; }

    /// <summary>
    /// Recalculates all line totals and invoice summaries adhering to Swiss 5-centimes rounding rules.
    /// </summary>
    public void RecalculateTotals()
    {
        var subtotal = 0m;
        var tax = 0m;

        foreach (var item in Items)
        {
            var raw = item.Quantity * item.UnitPrice;
            if (item.DiscountPercentage > 0)
            {
                raw -= raw * (item.DiscountPercentage / HundredPercent);
            }

            var lineTotal = decimal.Round(raw, CurrencyDecimals, MidpointRounding.AwayFromZero);
            var lineTax = decimal.Round(lineTotal * (item.TaxRate / HundredPercent), CurrencyDecimals, MidpointRounding.AwayFromZero);

            item.LineTotal = lineTotal;
            subtotal += lineTotal;
            tax += lineTax;
        }

        TotalNet = subtotal;
        TotalTax = tax;

        var unrounded = subtotal + tax;

        // Swiss 5-centimes cash rounding rule for CHF
        if (string.Equals(Currency, "CHF", StringComparison.OrdinalIgnoreCase))
        {
            TotalGross = decimal.Round(unrounded * SwissFiveCentimesMultiplier, 0, MidpointRounding.AwayFromZero) / SwissFiveCentimesMultiplier;
            RoundingAmount = decimal.Round(TotalGross - unrounded, CurrencyDecimals, MidpointRounding.AwayFromZero);
        }
        else
        {
            TotalGross = unrounded;
            RoundingAmount = 0m;
        }

        if (ShowQrPreview)
        {
            GenerateQrBillPayload();
        }
    }

    partial void OnCurrencyChanged(string value) => RecalculateTotals();

    private void ExecuteAddItem()
    {
        Items.Add(new InvoiceItemRowModel(this)
        {
            Description = "New item / Service",
            Quantity = 1,
            UnitPrice = DefaultItemUnitPrice,
            TaxRate = string.Equals(Currency, "CHF", StringComparison.OrdinalIgnoreCase) ? SwissVatRate : ItalianVatRate,
        });
        RecalculateTotals();
    }

    private void ExecuteRemoveItem(InvoiceItemRowModel? item)
    {
        if (item != null)
        {
            _ = Items.Remove(item);
            RecalculateTotals();
        }
    }

    private void ExecuteToggleQrPreview()
    {
        ShowQrPreview = !ShowQrPreview;
        if (ShowQrPreview)
        {
            GenerateQrBillPayload();
        }
    }

    private void GenerateQrBillPayload()
    {
        QrBillText =
            "SPC\r\n" +
            "0200\r\n" +
            "1\r\n" +
            "CH4431999123000889012\r\n" +
            "S\r\n" +
            "Acme Enterprise AG\r\n" +
            "Bahnhofstrasse 10\r\n" +
            "8001 Zürich\r\n\r\n\r\n" +
            "CH\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n" +
            $"{TotalGross.ToString("0.00", CultureInfo.InvariantCulture)}\r\n" +
            $"{Currency}\r\n" +
            "K\r\n" +
            "Client Organization\r\n" +
            "Via San Gottardo 14\r\n" +
            "6900 Lugano\r\n\r\n\r\n" +
            "CH\r\n" +
            "NON\r\n\r\n" +
            $"Invoice {InvoiceNumber}\r\n" +
            "EPD\r\n";
    }

    private async Task ExecuteSaveInvoiceAsync()
    {
        if (Items.Count == 0)
        {
            StatusMessage = "Please add at least one line item.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Saving invoice...";

        try
        {
            var command = new CreateInvoiceCommand(
                _tenantContext.TenantId != Guid.Empty ? _tenantContext.TenantId : Guid.NewGuid(),
                ContactId,
                string.Equals(Currency, "CHF", StringComparison.OrdinalIgnoreCase) ? InvoiceType.ChQrBill : InvoiceType.Standard,
                InvoiceNumber.Trim(),
                DateOnly.FromDateTime(Date),
                DueDate.HasValue ? DateOnly.FromDateTime(DueDate.Value) : null,
                Currency,
                string.Equals(Currency, "CHF", StringComparison.OrdinalIgnoreCase) ? Jurisdiction.CH : Jurisdiction.IT,
                Items.Select(i => new CreateInvoiceItemInput(
                    ProductId: null,
                    i.Description,
                    i.Quantity,
                    i.UnitPrice,
                    i.TaxRate,
                    i.DiscountPercentage)).ToList(),
                SplitPayment: false,
                ReverseCharge: false,
                Notes);

            var result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                StatusMessage = $"Save failed: {result.Error}";
                return;
            }

            StatusMessage = "Invoice saved successfully!";
            _navigationService.NavigateTo<InvoiceListViewModel>();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
