using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Dashboard;
using MediatR;

namespace FatturaFacile.ViewModels;

public sealed partial class DashboardViewModel(ISender mediator) : ViewModelBase
{
    private readonly ISender _mediator = mediator;

    [ObservableProperty]
    public partial DashboardKpiDto? KpiSummary { get; set; }

    public override Task InitializeAsync() => LoadKpiAsync();

    [RelayCommand]
    public async Task LoadKpiAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            KpiSummary = await _mediator.Send(new GetDashboardOverviewQuery());
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
