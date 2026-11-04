// <copyright file="DashboardViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Desktop.Wpf.Localization;
using FatturaFacile.Desktop.Wpf.Services;

namespace FatturaFacile.Desktop.Wpf.ViewModels;

/// <summary>
/// Executive dashboard view model showing key financial performance indicators and recent activity.
/// </summary>
public sealed class DashboardViewModel : ViewModelBase
{
    private readonly IDesktopLocalizer _localizer;
    private readonly INavigationService _navigationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardViewModel"/> class.
    /// </summary>
    /// <param name="localizer">The desktop localization service.</param>
    /// <param name="navigationService">The navigation service.</param>
    public DashboardViewModel(
        IDesktopLocalizer localizer,
        INavigationService navigationService)
    {
        _localizer = localizer;
        _navigationService = navigationService;
        Title = _localizer["Nav.Dashboard"];

        NavigateToInvoicesCommand = new RelayCommand(() => _navigationService.NavigateTo<InvoiceListViewModel>());
        CreateInvoiceCommand = new RelayCommand(() => _navigationService.NavigateTo<InvoiceDetailViewModel>());

        LoadSampleKpis();
    }

    /// <summary>
    /// Gets the localization service instance for UI binding.
    /// </summary>
    public IDesktopLocalizer Localizer => _localizer;

    /// <summary>
    /// Gets the formatted total revenue KPI metric string.
    /// </summary>
    public string TotalRevenue { get; } = "CHF 148'520.00";

    /// <summary>
    /// Gets the formatted open unpaid invoices KPI metric string.
    /// </summary>
    public string OpenInvoicesTotal { get; } = "CHF 24'350.50";

    /// <summary>
    /// Gets the formatted overdue invoices KPI metric string.
    /// </summary>
    public string OverdueInvoicesTotal { get; } = "CHF 4'120.00";

    /// <summary>
    /// Gets the count of active customer relationships.
    /// </summary>
    public int ActiveCustomersCount { get; } = 42;

    /// <summary>
    /// Gets the collection of recent business transaction activity items.
    /// </summary>
    public ObservableCollection<RecentActivityItem> RecentActivities { get; } = [];

    /// <summary>
    /// Gets the command that navigates to the invoice list view.
    /// </summary>
    public IRelayCommand NavigateToInvoicesCommand { get; }

    /// <summary>
    /// Gets the command that navigates to the invoice creation view.
    /// </summary>
    public IRelayCommand CreateInvoiceCommand { get; }

    private void LoadSampleKpis()
    {
        RecentActivities.Clear();
        RecentActivities.Add(new RecentActivityItem("INV-2026-0042", "Acme Corporation AG", "CHF 12'450.00", "Paid", "Today, 10:15"));
        RecentActivities.Add(new RecentActivityItem("INV-2026-0041", "Helvetia Digital Sàrl", "CHF 3'820.00", "Unpaid", "Yesterday, 16:45"));
        RecentActivities.Add(new RecentActivityItem("INV-2026-0040", "SwissTech Innovations SA", "CHF 8'080.50", "Paid", "01 Nov 2026"));
        RecentActivities.Add(new RecentActivityItem("INV-2026-0039", "Zurich Consulting Group", "CHF 1'200.00", "Overdue", "28 Oct 2026"));
    }
}
