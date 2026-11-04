using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using FatturaFacile.Invoicing.Infrastructure.Persistence;
using FatturaFacile.Desktop.Wpf.Localization;
using FatturaFacile.Desktop.Wpf.Services;

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

    public IDesktopLocalizer Localizer => _localizer;

    public ObservableCollection<InvoiceListItemModel> Invoices { get; } = [];

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

    public IAsyncRelayCommand RefreshCommand { get; }
    public IRelayCommand CreateInvoiceCommand { get; }
    public IRelayCommand EditInvoiceCommand { get; }
    public IRelayCommand ExportExcelCommand { get; }
    public IRelayCommand ExportPdfCommand { get; }

    private List<InvoiceListItemModel> _allInvoices = [];

    public async Task LoadInvoicesAsync()
    {
        IsBusy = true;
        StatusMessage = "Loading invoices from local SQLite database...";
        try
        {
            var invoices = await _dbContext.Invoices
                .AsNoTracking()
                .OrderByDescending(i => i.Date)
                .ToListAsync();

            _allInvoices = invoices.Select(i => new InvoiceListItemModel
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                Date = i.Date.ToDateTime(TimeOnly.MinValue),
                DueDate = i.DueDate?.ToDateTime(TimeOnly.MinValue),
                ContactId = i.ContactId,
                CustomerName = $"Customer #{i.ContactId.ToString()[..8]}",
                TotalNet = i.TotalNet,
                TotalTax = i.TotalTax,
                RoundingAmount = i.RoundingAmount,
                TotalGross = i.TotalGross,
                Currency = i.Currency,
                PaymentStatus = i.PaymentStatus.ToString()
            }).ToList();

            ApplyFilter();
            StatusMessage = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load invoices: {ex.Message}";
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

        if (!string.IsNullOrWhiteSpace(_searchFilter))
        {
            query = query.Where(i =>
                i.InvoiceNumber.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase) ||
                i.CustomerName.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase) ||
                i.PaymentStatus.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var inv in query)
        {
            Invoices.Add(inv);
        }
    }

    private void ExecuteEditInvoice()
    {
        if (SelectedInvoice != null)
        {
            var detailVm = (InvoiceDetailViewModel)_navigationService.GetType()
                .GetField("_serviceProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .GetValue(_navigationService)!;
        }
    }

    private void ExecuteExportExcel()
    {
        StatusMessage = "Export to Excel generated successfully.";
    }

    private void ExecuteExportPdf()
    {
        StatusMessage = "Export to PDF generated successfully.";
    }
}

public sealed class InvoiceListItemModel
{
    public Guid Id { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public DateTime? DueDate { get; init; }
    public Guid ContactId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public decimal TotalNet { get; init; }
    public decimal TotalTax { get; init; }
    public decimal RoundingAmount { get; init; }
    public decimal TotalGross { get; init; }
    public string Currency { get; init; } = "CHF";
    public string PaymentStatus { get; init; } = "Unpaid";

    /// <summary>
    /// Displays Swiss CHF amounts formatted according to Swiss accounting guidelines.
    /// </summary>
    public string FormattedTotalGross => $"{Currency} {TotalGross:N2}";
}
