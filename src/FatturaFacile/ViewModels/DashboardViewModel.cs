using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Dashboard;
using MediatR;

namespace FatturaFacile.ViewModels;

/// <summary>
/// Represents dashboard view model.
/// </summary>
/// <param name="mediator">The mediator.</param>
public sealed partial class DashboardViewModel(ISender mediator) : ViewModelBase
{
    private readonly ISender _mediator = mediator;

/// <summary>
/// Gets or sets the kpi summary.
/// </summary>
    [ObservableProperty]
    public partial DashboardKpiDto? KpiSummary { get; set; }

/// <summary>
/// Initialize Async.
/// </summary>
/// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public override Task InitializeAsync() => LoadKpiAsync();

/// <summary>
/// Load Kpi Async.
/// </summary>
/// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
