// <copyright file="InvoicesListViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Features.Invoices;
using FatturaFacile.Application.Ports;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// View model controlling the invoices list grid, providing search/filtering, aggregate totals calculation,
/// refresh commands, and new invoice dialog triggers.
/// </summary>
public partial class InvoicesListViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly IMapper _mapper;
    private readonly ICurrentTenantService _currentTenantService;
    private List<InvoiceRowViewModel> _allInvoices = new();

    [ObservableProperty]
    private ObservableCollection<InvoiceRowViewModel> _invoices = new();

    [ObservableProperty]
    private InvoiceRowViewModel? _selectedInvoice;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private decimal _totalGrossSum;

    [ObservableProperty]
    private decimal _totalNetSum;

    [ObservableProperty]
    private decimal _totalTaxSum;

    [ObservableProperty]
    private int _invoicesCount;

    [ObservableProperty]
    private string _currency = "CHF";

    /// <summary>
    /// Event raised when the user clicks the command to compose a new invoice.
    /// </summary>
    public event Action? RequestNewInvoiceDialog;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoicesListViewModel"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR sender dependency.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="currentTenantService">The ambient tenant context service.</param>
    public InvoicesListViewModel(
        ISender mediator,
        IMapper mapper,
        ICurrentTenantService currentTenantService)
    {
        _mediator = mediator;
        _mapper = mapper;
        _currentTenantService = currentTenantService;
        _currency = _currentTenantService.CurrentTenant?.DefaultCurrency ?? "CHF";

        _currentTenantService.TenantChanged += async (_, _) =>
        {
            _currency = _currentTenantService.CurrentTenant?.DefaultCurrency ?? "CHF";
            await LoadInvoicesAsync();
        };
    }

    /// <inheritdoc />
    public override async Task InitializeAsync()
    {
        await LoadInvoicesAsync();
    }

    /// <summary>
    /// Queries the backend data store for the tenant's invoice list items and updates the presentation collection.
    /// </summary>
    [RelayCommand]
    public async Task LoadInvoicesAsync()
    {
        IsBusy = true;
        BusyMessage = "Loading invoices from SQLite database...";

        try
        {
            var dtos = await _mediator.Send(new GetInvoicesListQuery());
            _allInvoices = dtos.Select(d => _mapper.Map<InvoiceRowViewModel>(d)).ToList();

            ApplyFilter();
        }
        catch (Exception ex)
        {
            BusyMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        IEnumerable<InvoiceRowViewModel> filtered = _allInvoices;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var query = SearchText.Trim().ToLowerInvariant();
            filtered = filtered.Where(i =>
                i.InvoiceNumber.ToLowerInvariant().Contains(query) ||
                i.CustomerName.ToLowerInvariant().Contains(query) ||
                i.PaymentStatus.ToLowerInvariant().Contains(query));
        }

        var list = filtered.ToList();

        Invoices.Clear();
        foreach (var inv in list)
        {
            Invoices.Add(inv);
        }

        InvoicesCount = list.Count;
        TotalGrossSum = list.Sum(i => i.TotalGross);
        TotalNetSum = list.Sum(i => i.TotalNet);
        TotalTaxSum = list.Sum(i => i.TotalTax);
    }

    /// <summary>
    /// Raises the <see cref="RequestNewInvoiceDialog"/> event to display the invoice composer dialog.
    /// </summary>
    [RelayCommand]
    public void OpenNewInvoiceDialog()
    {
        RequestNewInvoiceDialog?.Invoke();
    }
}
