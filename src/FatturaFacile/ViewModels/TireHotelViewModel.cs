using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Automotive;
using MediatR;

namespace FatturaFacile.ViewModels;

public sealed partial class TireHotelViewModel(ISender mediator) : ViewModelBase
{
    private readonly ISender _mediator = mediator;

    [ObservableProperty]
    public partial ObservableCollection<TireHotelDto> TireRecords { get; set; } = [];

    [ObservableProperty]
    public partial TireHotelDto? SelectedRecord { get; set; }

    public override Task InitializeAsync() => LoadTireRecordsAsync();

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
