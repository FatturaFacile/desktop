// <copyright file="DashboardViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Dashboard;
using MediatR;

namespace FatturaFacile.ViewModels;

/// <summary>
/// Manages high-level executive dashboard metrics, aggregating cross-module Key Performance Indicators (KPIs) such as revenues, overdue invoices, and active time tracking.
/// </summary>
/// <param name="mediator">The MediatR sender used to dispatch dashboard aggregation queries.</param>
public sealed partial class DashboardViewModel(ISender mediator) : ViewModelBase
{
    private readonly ISender mediator = mediator;

    /// <summary>
    /// Gets or sets the aggregated executive KPI metrics summary, or <see langword="null"/> if not yet loaded.
    /// </summary>
    [ObservableProperty]
    public partial DashboardKpiDto? KpiSummary { get; set; }

    /// <summary>
    /// Asynchronously initializes the ViewModel by retrieving the current business KPI summary.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous initialization operation.</returns>
    public override Task InitializeAsync() => LoadKpiAsync();

    /// <summary>
    /// Dispatches a <see cref="GetDashboardOverviewQuery"/> via MediatR to fetch aggregated business KPIs.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous query operation.</returns>
    [RelayCommand]
    public async Task LoadKpiAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            KpiSummary = await mediator.Send(new GetDashboardOverviewQuery());
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




//[GenerateViewModel]
//public partial class DashboardViewModel
//{
//    [GenerateProperty]
//    Type dashboard;
//    public DashboardViewModel()
//    {
//        dashboard = typeof(SampleDashboard);
//    }
//}
