// <copyright file="DashboardViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Features.Contacts;
using FatturaFacile.Application.Features.Invoices;
using FatturaFacile.Application.Ports;
using FatturaFacile.Wpf.Navigation;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// View model presenting executive metrics, key performance indicators (KPIs), year-to-date revenue,
/// outstanding receivables, and quick navigation actions on the main application dashboard.
/// </summary>
public partial class DashboardViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly INavigationService _navigationService;
    private readonly ICurrentTenantService _currentTenantService;

    [ObservableProperty]
    private decimal _revenueYtd;

    [ObservableProperty]
    private int _invoicesCount;

    [ObservableProperty]
    private decimal _unpaidAmount;

    [ObservableProperty]
    private int _customersCount;

    [ObservableProperty]
    private string _currency = "CHF";

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardViewModel"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR sender instance for dispatching queries.</param>
    /// <param name="navigationService">The shell navigation service.</param>
    /// <param name="currentTenantService">The ambient tenant context service.</param>
    public DashboardViewModel(
        ISender mediator,
        INavigationService navigationService,
        ICurrentTenantService currentTenantService)
    {
        _mediator = mediator;
        _navigationService = navigationService;
        _currentTenantService = currentTenantService;
        _currency = _currentTenantService.CurrentTenant?.DefaultCurrency ?? "CHF";

        _currentTenantService.TenantChanged += async (_, _) =>
        {
            _currency = _currentTenantService.CurrentTenant?.DefaultCurrency ?? "CHF";
            await InitializeAsync();
        };
    }

    /// <inheritdoc />
    public override async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            var invoices = await _mediator.Send(new GetInvoicesListQuery());
            InvoicesCount = invoices.Count;
            RevenueYtd = invoices.Where(i => i.PaymentStatus == "Paid").Sum(i => i.TotalGross);
            UnpaidAmount = invoices.Where(i => i.PaymentStatus != "Paid").Sum(i => i.TotalGross);

            var customers = await _mediator.Send(new GetCustomersQuery());
            CustomersCount = customers.Count;
        }
        catch
        {
            // Fallback gracefully on initialization error
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Navigates the application shell view to the invoice list management view.
    /// </summary>
    [RelayCommand]
    public void GoToInvoices()
    {
        _navigationService.NavigateTo<InvoicesListViewModel>();
    }
}
