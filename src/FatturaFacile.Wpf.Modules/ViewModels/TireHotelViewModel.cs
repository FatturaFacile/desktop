// <copyright file="TireHotelViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Automotive;

using MediatR;

namespace FatturaFacile.ViewModels;

/// <summary>
/// Manages automotive tire hotel operations, tracking seasonal wheel and tire storage, customer vehicle assignments, tread depths, and storage bay locations.
/// </summary>
/// <param name="mediator">The MediatR sender used to execute tire storage queries.</param>
public sealed partial class TireHotelViewModel(ISender mediator) : ViewModelBase
{

    /// <summary>
    /// The MediatR sender used to execute tire storage queries.
    /// </summary>
    private readonly ISender mediator = mediator;

    /// <summary>
    /// Gets or sets the collection of seasonal tire storage records retrieved for the active workshop tenant.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<TireHotelDto> TireRecords { get; set; } = [];

    /// <summary>
    /// Gets or sets the tire storage record currently selected for inspection or update, or <see langword="null"/> if none is selected.
    /// </summary>
    [ObservableProperty]
    public partial TireHotelDto? SelectedRecord { get; set; }

    /// <summary>
    /// Asynchronously initializes the ViewModel by querying tire hotel storage records.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous initialization operation.</returns>
    public override Task InitializeAsync() => LoadTireRecordsAsync();

    /// <summary>
    /// Dispatches a <see cref="GetTireRecordsQuery"/> via MediatR to fetch active tire hotel storage entries.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous query operation.</returns>
    [RelayCommand]
    public async Task LoadTireRecordsAsync()
    {
        IsBusy       = true;
        ErrorMessage = null;

        try
        {
            var list = await mediator.Send(request: new GetTireRecordsQuery());
            TireRecords    = [with(collection: list)];
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
