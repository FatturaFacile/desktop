using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using FatturaFacile.Invoicing.Application.Commands;
using FatturaFacile.Invoicing.Application.DTOs;
using FatturaFacile.Invoicing.Domain.Enums;
using FatturaFacile.Desktop.Wpf.Localization;
using FatturaFacile.Desktop.Wpf.Services;

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
            TaxRate = 8.1m // Swiss standard VAT rate (8.1%)
        });

        RecalculateTotals();
    }

    public IDesktopLocalizer Localizer => _localizer;

    public ObservableCollection<InvoiceItemRowModel> Items { get; } = [];

    public string InvoiceNumber
    {
        get => _invoiceNumber;
        set => SetProperty(ref _invoiceNumber, value);
    }

    public DateTime Date
    {
        get => _date;
        set => SetProperty(ref _date, value);
    }

    public DateTime? DueDate
    {
        get => _dueDate;
        set => SetProperty(ref _dueDate, value);
    }

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

    public Guid ContactId
    {
        get => _contactId;
        set => SetProperty(ref _contactId, value);
    }

    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public decimal TotalNet
    {
        get => _totalNet;
        private set => SetProperty(ref _totalNet, value);
    }

    public decimal TotalTax
    {
        get => _totalTax;
        private set => SetProperty(ref _totalTax, value);
    }

    public decimal RoundingAmount
    {
        get => _roundingAmount;
        private set => SetProperty(ref _roundingAmount, value);
    }

    public decimal TotalGross
    {
        get => _totalGross;
        private set => SetProperty(ref _totalGross, value);
    }

    public bool ShowQrPreview
    {
        get => _showQrPreview;
        set => SetProperty(ref _showQrPreview, value);
    }

    public string QrBillText
    {
        get => _qrBillText;
        set => SetProperty(ref _qrBillText, value);
    }

    public IRelayCommand AddItemCommand { get; }
    public IRelayCommand<InvoiceItemRowModel> RemoveItemCommand { get; }
    public IAsyncRelayCommand SaveInvoiceCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand ToggleQrPreviewCommand { get; }

    public void RecalculateTotals()
    {
        var net = 0m;
        var tax = 0m;

        foreach (var item in Items)
        {
            var lineNet = decimal.Round(item.Quantity * item.UnitPrice * (1m - (item.DiscountPercentage / 100m)), 2, MidpointRounding.AwayFromZero);
            var lineTax = decimal.Round(lineNet * (item.TaxRate / 100m), 2, MidpointRounding.AwayFromZero);
            item.LineTotal = lineNet;
            net += lineNet;
            tax += lineTax;
        }

        TotalNet = decimal.Round(net, 2, MidpointRounding.AwayFromZero);
        TotalTax = decimal.Round(tax, 2, MidpointRounding.AwayFromZero);
        var unroundedGross = TotalNet + TotalTax;

        if (string.Equals(Currency, "CHF", StringComparison.OrdinalIgnoreCase))
        {
            // Swiss CHF 0.05 rounding rule (round to nearest 0.05 CHF)
            TotalGross = decimal.Round(unroundedGross * 20m, 0, MidpointRounding.AwayFromZero) / 20m;
            RoundingAmount = decimal.Round(TotalGross - unroundedGross, 2, MidpointRounding.AwayFromZero);
        }
        else
        {
            TotalGross = unroundedGross;
            RoundingAmount = 0m;
        }
    }

    private void ExecuteAddItem()
    {
        Items.Add(new InvoiceItemRowModel(this)
        {
            Description = "New item",
            Quantity = 1,
            UnitPrice = 100m,
            TaxRate = 8.1m
        });
        RecalculateTotals();
    }

    private void ExecuteRemoveItem(InvoiceItemRowModel? item)
    {
        if (item != null && Items.Contains(item))
        {
            Items.Remove(item);
            RecalculateTotals();
        }
    }

    private async Task ExecuteSaveInvoiceAsync()
    {
        IsBusy = true;
        StatusMessage = "Persisting invoice directly to local SQLite database...";

        try
        {
            var itemInputs = Items.Select(i => new CreateInvoiceItemInput(
                null,
                i.Description,
                i.Quantity,
                i.UnitPrice,
                i.TaxRate,
                i.DiscountPercentage
            )).ToList();

            var command = new CreateInvoiceCommand(
                _tenantContext.TenantId != Guid.Empty ? _tenantContext.TenantId : Guid.NewGuid(),
                ContactId,
                InvoiceType.Standard,
                InvoiceNumber,
                DateOnly.FromDateTime(Date),
                DueDate.HasValue ? DateOnly.FromDateTime(DueDate.Value) : null,
                Currency,
                Jurisdiction.CH,
                itemInputs,
                Notes: Notes
            );

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                StatusMessage = "Invoice created successfully.";
                _navigationService.NavigateTo<InvoiceListViewModel>();
            }
            else
            {
                StatusMessage = $"Failed to save invoice: {result.Error}";
            }
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

    private void ExecuteToggleQrPreview()
    {
        ShowQrPreview = !ShowQrPreview;
        if (ShowQrPreview)
        {
            // Format Swiss QR-Bill data payload representation (SPC specification)
            QrBillText =
                $"=== SWISS QR-BILL (ISO 20022 - SIX INTERBANK CLEARING) ===\n" +
                $"Currency: {Currency}\n" +
                $"Amount: {TotalGross:N2}\n" +
                $"Payable to: FatturaFacile Demo AG, Bahnhofstrasse 100, 8001 Zurich\n" +
                $"IBAN: CH44 0000 0000 0000 0000 0\n" +
                $"Reference: RF18 {InvoiceNumber.Replace("-", "")}\n" +
                $"Customer: ID #{ContactId.ToString()[..8]}\n" +
                $"Date: {Date:yyyy-MM-dd} | Due: {DueDate:yyyy-MM-dd}\n" +
                $"Round CHF 5c: {RoundingAmount:+0.00;-0.00;0.00}\n" +
                $"==========================================================";
        }
    }
}

public sealed class InvoiceItemRowModel : ObservableObject
{
    private readonly InvoiceDetailViewModel _parent;

    public InvoiceItemRowModel(InvoiceDetailViewModel parent)
    {
        _parent = parent;
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    private decimal _quantity = 1m;
    public decimal Quantity
    {
        get => _quantity;
        set
        {
            if (SetProperty(ref _quantity, value))
            {
                _parent.RecalculateTotals();
            }
        }
    }

    private decimal _unitPrice;
    public decimal UnitPrice
    {
        get => _unitPrice;
        set
        {
            if (SetProperty(ref _unitPrice, value))
            {
                _parent.RecalculateTotals();
            }
        }
    }

    private decimal _taxRate = 8.1m;
    public decimal TaxRate
    {
        get => _taxRate;
        set
        {
            if (SetProperty(ref _taxRate, value))
            {
                _parent.RecalculateTotals();
            }
        }
    }

    private decimal _discountPercentage;
    public decimal DiscountPercentage
    {
        get => _discountPercentage;
        set
        {
            if (SetProperty(ref _discountPercentage, value))
            {
                _parent.RecalculateTotals();
            }
        }
    }

    private decimal _lineTotal;
    public decimal LineTotal
    {
        get => _lineTotal;
        set => SetProperty(ref _lineTotal, value);
    }
}
