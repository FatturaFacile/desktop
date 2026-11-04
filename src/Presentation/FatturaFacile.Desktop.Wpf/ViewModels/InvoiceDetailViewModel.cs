// <copyright file="InvoiceDetailViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
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
public sealed class InvoiceDetailViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly DesktopTenantContext _tenantContext;
    private readonly IDesktopLocalizer _localizer;
    private readonly INavigationService _navigationService;

    private string _invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100, 999)}";
    private DateTime _date = DateTime.Today;
    private DateTime? _dueDate = DateTime.Today.AddDays(30);
    private string _currency = "CHF";
    private Guid _contactId = Guid.NewGuid();
    private string _notes = string.Empty;
    private decimal _totalNet;
    private decimal _totalTax;
    private decimal _roundingAmount;
    private decimal _totalGross;
    private bool _showQrPreview;
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
        _localizer = localizer;
        _navigationService = navigationService;
        Title = _localizer["InvoiceDetail.Title"];

        AddItemCommand = new RelayCommand(ExecuteAddItem);
        RemoveItemCommand = new RelayCommand<InvoiceItemRowModel>(ExecuteRemoveItem);
        SaveInvoiceCommand = new AsyncRelayCommand(ExecuteSaveInvoiceAsync, () => !IsBusy);
        CancelCommand = new RelayCommand(() => _navigationService.NavigateTo<InvoiceListViewModel>());
        ToggleQrPreviewCommand = new RelayCommand(ExecuteToggleQrPreview);

        // Add a default sample line item
        Items.Add(new InvoiceItemRowModel(this)
        {
            Description = "Consulting & Software Engineering Services",
            Quantity = 10,
            UnitPrice = 160.00m,
            TaxRate = 8.1m,// Swiss standard VAT rate (8.1%)
        });

        RecalculateTotals();
    }

    /// <summary>
    /// Gets the desktop localization service.
    /// </summary>
    public IDesktopLocalizer Localizer => _localizer;

    /// <summary>
    /// Gets the collection of editable line item rows in the invoice.
    /// </summary>
    public ObservableCollection<InvoiceItemRowModel> Items { get; } = [];

    /// <summary>
    /// Gets or sets the unique invoice reference number.
    /// </summary>
    public string InvoiceNumber
    {
        get => _invoiceNumber;
        set => SetProperty(ref _invoiceNumber, value);
    }

    /// <summary>
    /// Gets or sets the invoice issue date.
    /// </summary>
    public DateTime Date
    {
        get => _date;
        set => SetProperty(ref _date, value);
    }

    /// <summary>
    /// Gets or sets the payment due date.
    /// </summary>
    public DateTime? DueDate
    {
        get => _dueDate;
        set => SetProperty(ref _dueDate, value);
    }

    /// <summary>
    /// Gets or sets the 3-letter currency code (CHF, EUR, USD).
    /// </summary>
    public string Currency
    {
        get => _currency;
        set
        {
            if (SetProperty(ref _currency, value))
            {
                RecalculateTotals();
            }
        }
    }

    /// <summary>
    /// Gets or sets the recipient customer identifier.
    /// </summary>
    public Guid ContactId
    {
        get => _contactId;
        set => SetProperty(ref _contactId, value);
    }

    /// <summary>
    /// Gets or sets optional notes and payment terms.
    /// </summary>
    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    /// <summary>
    /// Gets or sets the computed net total amount.
    /// </summary>
    public decimal TotalNet
    {
        get => _totalNet;
        set => SetProperty(ref _totalNet, value);
    }

    /// <summary>
    /// Gets or sets the computed total VAT/tax amount.
    /// </summary>
    public decimal TotalTax
    {
        get => _totalTax;
        set => SetProperty(ref _totalTax, value);
    }

    /// <summary>
    /// Gets or sets the Swiss 0.05 rounding adjustment amount.
    /// </summary>
    public decimal RoundingAmount
    {
        get => _roundingAmount;
        set => SetProperty(ref _roundingAmount, value);
    }

    /// <summary>
    /// Gets or sets the gross payable amount.
    /// </summary>
    public decimal TotalGross
    {
        get => _totalGross;
        set => SetProperty(ref _totalGross, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the Swiss QR-Bill visual preview is displayed.
    /// </summary>
    public bool ShowQrPreview
    {
        get => _showQrPreview;
        set => SetProperty(ref _showQrPreview, value);
    }

    /// <summary>
    /// Gets or sets the formatted textual representation of the Swiss QR-Bill payload.
    /// </summary>
    public string QrBillText
    {
        get => _qrBillText;
        set => SetProperty(ref _qrBillText, value);
    }

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

    private void ExecuteAddItem()
    {
        Items.Add(new InvoiceItemRowModel(this)
        {
            Description = "New item / Service",
            Quantity = 1,
            UnitPrice = 100.00m,
            TaxRate = string.Equals(Currency, "CHF", StringComparison.OrdinalIgnoreCase) ? 8.1m : 22.0m,
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

    /// <summary>
    /// Recalculates all line totals and invoice summaries adhering to Swiss 5-centimes rounding rules.
    /// </summary>
    public void RecalculateTotals()
    {
        decimal subtotal = 0m;
        decimal tax = 0m;

        foreach (var item in Items)
        {
            var raw = item.Quantity * item.UnitPrice;
            if (item.DiscountPercentage > 0)
            {
                raw -= raw * (item.DiscountPercentage / 100m);
            }
            var lineTotal = decimal.Round(raw, 2, MidpointRounding.AwayFromZero);
            var lineTax = decimal.Round(lineTotal * (item.TaxRate / 100m), 2, MidpointRounding.AwayFromZero);

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
            TotalGross = decimal.Round(unrounded * 20m, 0, MidpointRounding.AwayFromZero) / 20m;
            RoundingAmount = decimal.Round(TotalGross - unrounded, 2, MidpointRounding.AwayFromZero);
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
            $"{TotalGross:0.00}\r\n" +
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
