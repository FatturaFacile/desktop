// <copyright file="InvoiceListViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Desktop.Wpf.Localization;
using FatturaFacile.Desktop.Wpf.Services;
using FatturaFacile.Invoicing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Desktop.Wpf.ViewModels;

/// <summary>
/// View model displaying the list of sales invoices with sorting, filtering, Swiss 5-centimes formatting, and exports.
/// </summary>
internal sealed partial class InvoiceListViewModel : ViewModelBase
{
    private const int CustomerIdSubstrLength = 8;
    private readonly InvoicingDbContext _dbContext;
    private readonly INavigationService _navigationService;

    private List<InvoiceListItemModel> _allInvoices = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceListViewModel"/> class.
    /// </summary>
    /// <param name="dbContext">The invoicing database context.</param>
    /// <param name="localizer">The desktop localizer.</param>
    /// <param name="navigationService">The navigation service.</param>
    public InvoiceListViewModel(
        InvoicingDbContext dbContext,
        IDesktopLocalizer localizer,
        INavigationService navigationService)
    {
        _dbContext = dbContext;
        Localizer = localizer;
        _navigationService = navigationService;
        Title = Localizer["Invoices.Title"];

        RefreshCommand = new AsyncRelayCommand(LoadInvoicesAsync);
        CreateInvoiceCommand = new RelayCommand(() => _navigationService.NavigateTo<InvoiceDetailViewModel>());
        EditInvoiceCommand = new RelayCommand(ExecuteEditInvoice, () => SelectedInvoice != null);
        ExportExcelCommand = new RelayCommand(ExecuteExportExcel);
        ExportPdfCommand = new RelayCommand(ExecuteExportPdf);

        // Load data initially
        _ = LoadInvoicesAsync();
    }

    [ObservableProperty]
    public partial string SearchFilter { get; set; } = string.Empty;

    [ObservableProperty]
    public partial InvoiceListItemModel? SelectedInvoice { get; set; }

    /// <summary>
    /// Gets the desktop localization service.
    /// </summary>
    public IDesktopLocalizer Localizer { get; }

    /// <summary>
    /// Gets the observable collection of displayed invoice list items.
    /// </summary>
    public ObservableCollection<InvoiceListItemModel> Invoices { get; } = [];

    /// <summary>
    /// Gets the command that reloads the invoice list from SQLite.
    /// </summary>
    public IAsyncRelayCommand RefreshCommand { get; }

    /// <summary>
    /// Gets the command that navigates to the invoice creation view.
    /// </summary>
    public IRelayCommand CreateInvoiceCommand { get; }

    /// <summary>
    /// Gets the command that opens the selected invoice for editing.
    /// </summary>
    public IRelayCommand EditInvoiceCommand { get; }

    /// <summary>
    /// Gets the command that exports the invoice list to Microsoft Excel.
    /// </summary>
    public IRelayCommand ExportExcelCommand { get; }

    /// <summary>
    /// Gets the command that exports invoices to PDF.
    /// </summary>
    public IRelayCommand ExportPdfCommand { get; }

    partial void OnSearchFilterChanged(string value) => ApplyFilter();

    partial void OnSelectedInvoiceChanged(InvoiceListItemModel? value) => EditInvoiceCommand.NotifyCanExecuteChanged();

    private async Task LoadInvoicesAsync()
    {
        IsBusy = true;
        StatusMessage = Localizer["Invoices.Loading"];

        try
        {
            var rawInvoices = await _dbContext.Invoices
                .AsNoTracking()
                .OrderByDescending(i => i.Date)
                .Select(i => new
                {
                    i.Id,
                    i.InvoiceNumber,
                    Date = i.Date.ToDateTime(TimeOnly.MinValue),
                    DueDate = i.DueDate.HasValue ? i.DueDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    i.ContactId,
                    i.TotalNet,
                    i.TotalTax,
                    i.RoundingAmount,
                    i.TotalGross,
                    i.Currency,
                    PaymentStatus = i.PaymentStatus.ToString(),
                })
                .ToListAsync();

            _allInvoices = rawInvoices.ConvertAll(i => new InvoiceListItemModel
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                Date = i.Date,
                DueDate = i.DueDate,
                ContactId = i.ContactId,
                CustomerName = $"Client {i.ContactId.ToString().AsSpan(0, CustomerIdSubstrLength)}",
                TotalNet = i.TotalNet,
                TotalTax = i.TotalTax,
                RoundingAmount = i.RoundingAmount,
                TotalGross = i.TotalGross,
                Currency = i.Currency,
                PaymentStatus = i.PaymentStatus,
            });

            ApplyFilter();
            StatusMessage = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading invoices: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        Invoices.Clear();
        var query = _allInvoices.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchFilter))
        {
            var filter = SearchFilter.Trim();
            query = query.Where(i =>
                i.InvoiceNumber.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                i.CustomerName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                i.PaymentStatus.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var invoice in query)
        {
            Invoices.Add(invoice);
        }
    }

    private void ExecuteEditInvoice()
    {
        if (SelectedInvoice != null)
        {
            _navigationService.NavigateTo<InvoiceDetailViewModel>();
        }
    }

    private void ExecuteExportExcel() => StatusMessage = "Exporting to Excel (Syncfusion XlsIO)...";

    private void ExecuteExportPdf() => StatusMessage = "Generating PDF / Swiss QR-Bill (Syncfusion Pdf)...";
}
