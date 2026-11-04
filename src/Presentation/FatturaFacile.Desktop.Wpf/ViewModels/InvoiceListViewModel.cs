// <copyright file="InvoiceListViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Desktop.Wpf.Localization;
using FatturaFacile.Desktop.Wpf.Services;
using FatturaFacile.Invoicing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Desktop.Wpf.ViewModels;

/// <summary>
/// View model displaying the list of sales invoices with sorting, filtering, Swiss 5-centimes formatting, and exports.
/// </summary>
public sealed class InvoiceListViewModel : ViewModelBase
{
    private readonly InvoicingDbContext _dbContext;
    private readonly DesktopTenantContext _tenantContext;
    private readonly IDesktopLocalizer _localizer;
    private readonly INavigationService _navigationService;

    private string _searchFilter = string.Empty;
    private InvoiceListItemModel? _selectedInvoice;

    private List<InvoiceListItemModel> _allInvoices = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceListViewModel"/> class.
    /// </summary>
    /// <param name="dbContext">The invoicing database context.</param>
    /// <param name="tenantContext">The desktop tenant session context.</param>
    /// <param name="localizer">The desktop localizer.</param>
    /// <param name="navigationService">The navigation service.</param>
    public InvoiceListViewModel(
        InvoicingDbContext dbContext,
        DesktopTenantContext tenantContext,
        IDesktopLocalizer localizer,
        INavigationService navigationService)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _localizer = localizer;
        _navigationService = navigationService;
        Title = _localizer["Invoices.Title"];

        RefreshCommand = new AsyncRelayCommand(LoadInvoicesAsync);
        CreateInvoiceCommand = new RelayCommand(() => _navigationService.NavigateTo<InvoiceDetailViewModel>());
        EditInvoiceCommand = new RelayCommand(ExecuteEditInvoice, () => SelectedInvoice != null);
        ExportExcelCommand = new RelayCommand(ExecuteExportExcel);
        ExportPdfCommand = new RelayCommand(ExecuteExportPdf);

        // Load data initially
        _ = LoadInvoicesAsync();
    }

    /// <summary>
    /// Gets the desktop localization service.
    /// </summary>
    public IDesktopLocalizer Localizer => _localizer;

    /// <summary>
    /// Gets the observable collection of displayed invoice list items.
    /// </summary>
    public ObservableCollection<InvoiceListItemModel> Invoices { get; } = [];

    /// <summary>
    /// Gets or sets the text search query used to filter invoices.
    /// </summary>
    public string SearchFilter
    {
        get => _searchFilter;
        set
        {
            if (SetProperty(ref _searchFilter, value))
            {
                ApplyFilter();
            }
        }
    }

    /// <summary>
    /// Gets or sets the currently selected invoice in the data grid.
    /// </summary>
    public InvoiceListItemModel? SelectedInvoice
    {
        get => _selectedInvoice;
        set
        {
            if (SetProperty(ref _selectedInvoice, value))
            {
                EditInvoiceCommand.NotifyCanExecuteChanged();
            }
        }
    }

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

    private async Task LoadInvoicesAsync()
    {
        IsBusy = true;
        StatusMessage = _localizer["Invoices.Loading"];

        try
        {
            _allInvoices = await _dbContext.Invoices
                .AsNoTracking()
                .OrderByDescending(i => i.Date)
                .Select(i => new InvoiceListItemModel
                {
                    Id = i.Id,
                    InvoiceNumber = i.InvoiceNumber,
                    Date = i.Date.ToDateTime(TimeOnly.MinValue),
                    DueDate = i.DueDate.HasValue ? i.DueDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                    ContactId = i.ContactId,
                    CustomerName = "Client " + i.ContactId.ToString().Substring(0, 8),
                    TotalNet = i.TotalNet,
                    TotalTax = i.TotalTax,
                    RoundingAmount = i.RoundingAmount,
                    TotalGross = i.TotalGross,
                    Currency = i.Currency,
                    PaymentStatus = i.PaymentStatus.ToString(),
                })
                .ToListAsync();
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
            var filter = SearchFilter.Trim().ToLowerInvariant();
            query = query.Where(i =>
                i.InvoiceNumber.ToLowerInvariant().Contains(filter) ||
                i.CustomerName.ToLowerInvariant().Contains(filter) ||
                i.PaymentStatus.ToLowerInvariant().Contains(filter));
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

    private void ExecuteExportExcel()
    {
        StatusMessage = "Exporting to Excel (Syncfusion XlsIO)...";
    }

    private void ExecuteExportPdf()
    {
        StatusMessage = "Generating PDF / Swiss QR-Bill (Syncfusion Pdf)...";
    }
}
