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

    public IDesktopLocalizer Localizer => _localizer;

    public string TotalRevenue { get; private set; } = "CHF 148'520.00";
    public string OpenInvoicesTotal { get; private set; } = "CHF 24'350.50";
    public string OverdueInvoicesTotal { get; private set; } = "CHF 4'120.00";
    public int ActiveCustomersCount { get; private set; } = 42;

    public ObservableCollection<RecentActivityItem> RecentActivities { get; } = [];

    public IRelayCommand NavigateToInvoicesCommand { get; }
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

public sealed record RecentActivityItem(string InvoiceNumber, string Customer, string Amount, string Status, string Time);
