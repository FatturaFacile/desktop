using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Automotive;
using MediatR;

namespace FatturaFacile.ViewModels;

/// <summary>
/// Represents tire hotel view model.
/// </summary>
/// <param name="mediator">The mediator.</param>
public sealed partial class TireHotelViewModel(ISender mediator) : ViewModelBase
{
    private readonly ISender _mediator = mediator;

/// <summary>
/// Gets or sets the tire records.
/// </summary>
    [ObservableProperty]
    public partial ObservableCollection<TireHotelDto> TireRecords { get; set; } = [];

/// <summary>
/// Gets or sets the selected record.
/// </summary>
    [ObservableProperty]
    public partial TireHotelDto? SelectedRecord { get; set; }

/// <summary>
/// Initialize Async.
/// </summary>
/// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public override Task InitializeAsync() => LoadTireRecordsAsync();

/// <summary>
/// Load Tire Records Async.
/// </summary>
/// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    [RelayCommand]
    public async Task LoadTireRecordsAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var list = await _mediator.Send(new GetTireRecordsQuery());
            TireRecords = [with(list)];
            SelectedRecord = TireRecords.FirstOrDefault();
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
