using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Clients;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.ViewModels;

/// <summary>
    /// Represents clients view model.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    public sealed partial class ClientsViewModel(ISender mediator) : ViewModelBase
    {
    private readonly ISender _mediator = mediator;

/// <summary>
    /// Gets or sets the clients.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<ClientDto> Clients { get; set; } = [];

/// <summary>
    /// Gets or sets the available jurisdictions.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<Jurisdiction> AvailableJurisdictions { get; set; } = [Jurisdiction.Italy, Jurisdiction.Switzerland];

/// <summary>
    /// Gets or sets the selected client.
    /// </summary>
    [ObservableProperty]
    public partial ClientDto? SelectedClient { get; set; }

/// <summary>
    /// Gets or sets a value indicating whether create modal open.
    /// </summary>
    [ObservableProperty]
    public partial bool IsCreateModalOpen { get; set; }

/// <summary>
    /// Gets or sets the create Form Fields.
    /// </summary>
[ObservableProperty]
    public partial string NewBusinessName { get; set; } = string.Empty;

/// <summary>
/// Gets or sets the new jurisdiction.
/// </summary>
    [ObservableProperty]
    public partial Jurisdiction NewJurisdiction { get; set; } = Jurisdiction.Italy;

/// <summary>
/// Gets or sets the new vat number.
/// </summary>
    [ObservableProperty]
    public partial string NewVatNumber { get; set; } = string.Empty;

/// <summary>
/// Gets or sets the new fiscal code.
/// </summary>
    [ObservableProperty]
    public partial string NewFiscalCode { get; set; } = string.Empty;

/// <summary>
/// Gets or sets the new sdi code.
/// </summary>
    [ObservableProperty]
    public partial string NewSdiCode { get; set; } = "0000000";

/// <summary>
/// Gets or sets the new pec email.
/// </summary>
    [ObservableProperty]
    public partial string NewPecEmail { get; set; } = string.Empty;

/// <summary>
/// Gets or sets the new swiss uid.
/// </summary>
    [ObservableProperty]
    public partial string NewSwissUid { get; set; } = string.Empty;

/// <summary>
/// Gets or sets the new qr iban.
/// </summary>
    [ObservableProperty]
    public partial string NewQrIban { get; set; } = string.Empty;

/// <summary>
/// Gets or sets the new street.
/// </summary>
    [ObservableProperty]
    public partial string NewStreet { get; set; } = string.Empty;

/// <summary>
/// Gets or sets the new city.
/// </summary>
    [ObservableProperty]
    public partial string NewCity { get; set; } = string.Empty;

/// <summary>
/// Gets or sets the new postal code.
/// </summary>
    [ObservableProperty]
    public partial string NewPostalCode { get; set; } = string.Empty;

/// <summary>
/// Gets or sets the new province or canton.
/// </summary>
    [ObservableProperty]
    public partial string NewProvinceOrCanton { get; set; } = string.Empty;

/// <summary>
/// Gets or sets the new email.
/// </summary>
    [ObservableProperty]
    public partial string NewEmail { get; set; } = string.Empty;

/// <summary>
/// Initialize Async.
/// </summary>
/// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public override Task InitializeAsync() => LoadClientsAsync();

/// <summary>
/// Load Clients Async.
/// </summary>
/// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    [RelayCommand]
    public async Task LoadClientsAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var list = await _mediator.Send(new GetClientsQuery());
            Clients = [with(list)];
            SelectedClient = Clients.FirstOrDefault();
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

/// <summary>
/// Open Create Modal.
/// </summary>
    [RelayCommand]
    public void OpenCreateModal() => IsCreateModalOpen = true;

/// <summary>
/// Close Create Modal.
/// </summary>
    [RelayCommand]
    public void CloseCreateModal() => IsCreateModalOpen = false;

/// <summary>
/// Save New Client Async.
/// </summary>
/// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    [RelayCommand]
    public async Task SaveNewClientAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var countryCode = NewJurisdiction == Jurisdiction.Switzerland ? "CH" : "IT";

            var command = new CreateClientCommand(
                NewBusinessName,
                NewJurisdiction,
                NewJurisdiction == Jurisdiction.Italy ? NewFiscalCode : null,
                NewJurisdiction == Jurisdiction.Italy ? NewVatNumber : null,
                NewJurisdiction == Jurisdiction.Italy ? NewSdiCode : null,
                NewJurisdiction == Jurisdiction.Italy ? NewPecEmail : null,
                NewJurisdiction == Jurisdiction.Switzerland ? NewSwissUid : null,
                NewJurisdiction == Jurisdiction.Switzerland ? NewQrIban : null,
                NewJurisdiction == Jurisdiction.Switzerland ? NewProvinceOrCanton : null,
                NewStreet,
                NewCity,
                NewPostalCode,
                NewProvinceOrCanton,
                countryCode,
                NewEmail,
PhoneNumber: null);

            var created = await _mediator.Send(command);
            Clients.Add(created);
            SelectedClient = created;
            IsCreateModalOpen = false;
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
