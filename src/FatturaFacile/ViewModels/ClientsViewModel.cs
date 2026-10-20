using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Clients;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.ViewModels;

public sealed partial class ClientsViewModel(ISender mediator) : ViewModelBase
{
    private readonly ISender _mediator = mediator;

    [ObservableProperty]
    public partial ObservableCollection<ClientDto> Clients { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<Jurisdiction> AvailableJurisdictions { get; set; } = [Jurisdiction.Italy, Jurisdiction.Switzerland];

    [ObservableProperty]
    public partial ClientDto? SelectedClient { get; set; }

    [ObservableProperty]
    public partial bool IsCreateModalOpen { get; set; }

    /// <summary>
    /// Create Form Fields
    /// </summary>
    [ObservableProperty]
    public partial string NewBusinessName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial Jurisdiction NewJurisdiction { get; set; } = Jurisdiction.Italy;

    [ObservableProperty]
    public partial string NewVatNumber { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewFiscalCode { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewSdiCode { get; set; } = "0000000";

    [ObservableProperty]
    public partial string NewPecEmail { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewSwissUid { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewQrIban { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewStreet { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewCity { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewPostalCode { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewProvinceOrCanton { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewEmail { get; set; } = string.Empty;

    public override Task InitializeAsync() => LoadClientsAsync();

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

    [RelayCommand]
    public void OpenCreateModal() => IsCreateModalOpen = true;

    [RelayCommand]
    public void CloseCreateModal() => IsCreateModalOpen = false;

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
