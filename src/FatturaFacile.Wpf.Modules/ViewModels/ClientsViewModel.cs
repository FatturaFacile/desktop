// <copyright file="ClientsViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Clients;
using FatturaFacile.Domain.Enums;

using MediatR;

namespace FatturaFacile.ViewModels;

/// <summary>
/// Manages client-related presentation logic, querying customer registries, and onboarding clients across Italian and Swiss fiscal jurisdictions.
/// </summary>
/// <param name="mediator">The MediatR sender used to dispatch queries and commands for client operations.</param>
public sealed partial class ClientsViewModel(ISender mediator) : ViewModelBase
{

    /// <summary>
    /// The MediatR sender used to dispatch queries and commands for client operations.
    /// </summary>
    private readonly ISender mediator = mediator;

    /// <summary>
    /// Gets or sets the collection of retrieved client records currently displayed in the view.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<ClientDto> Clients { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of supported fiscal jurisdictions available for client onboarding and filtering.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<Jurisdiction> AvailableJurisdictions { get; set; } =
        [
            Jurisdiction.Italy, Jurisdiction.Switzerland
        ];

    /// <summary>
    /// Gets or sets the client currently selected in the master grid or details panel, or <see langword="null"/> if none is selected.
    /// </summary>
    [ObservableProperty]
    public partial ClientDto? SelectedClient { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the modal dialog for creating a new client is currently displayed.
    /// </summary>
    [ObservableProperty]
    public partial bool IsCreateModalOpen { get; set; }

    /// <summary>
    /// Gets or sets the company or legal business name for the client being created.
    /// </summary>
    [ObservableProperty]
    public partial string NewBusinessName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target fiscal jurisdiction governing the validation rules for the new client.
    /// </summary>
    [ObservableProperty]
    public partial Jurisdiction NewJurisdiction { get; set; } = Jurisdiction.Italy;

    /// <summary>
    /// Gets or sets the Italian VAT registration number (Partita IVA) for the new client.
    /// </summary>
    [ObservableProperty]
    public partial string NewVatNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Italian tax/fiscal code (Codice Fiscale) for the new client.
    /// </summary>
    [ObservableProperty]
    public partial string NewFiscalCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Italian SDI recipient code (Codice Destinatario) for electronic invoice routing, defaulting to standard 7 zeros.
    /// </summary>
    [ObservableProperty]
    public partial string NewSdiCode { get; set; } = "0000000";

    /// <summary>
    /// Gets or sets the certified email address (Posta Elettronica Certificata) used for Italian electronic document delivery.
    /// </summary>
    [ObservableProperty]
    public partial string NewPecEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Swiss Business Identification Number (UID/IDE, e.g. CHE-123.456.789) for the new client.
    /// </summary>
    [ObservableProperty]
    public partial string NewSwissUid { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Swiss QR-IBAN formatted account number for Swiss QR-bill payments.
    /// </summary>
    [ObservableProperty]
    public partial string NewQrIban { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the street address and house number for the new client.
    /// </summary>
    [ObservableProperty]
    public partial string NewStreet { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the city or municipality for the new client's address.
    /// </summary>
    [ObservableProperty]
    public partial string NewCity { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the postal or ZIP code for the new client's address.
    /// </summary>
    [ObservableProperty]
    public partial string NewPostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the two-letter province code (Italy) or canton abbreviation (Switzerland) for the new client.
    /// </summary>
    [ObservableProperty]
    public partial string NewProvinceOrCanton { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary administrative contact email address for the new client.
    /// </summary>
    [ObservableProperty]
    public partial string NewEmail { get; set; } = string.Empty;

    /// <summary>
    /// Asynchronously initializes the ViewModel by loading the initial client registry from the backend.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous initialization operation.</returns>
    public override Task InitializeAsync() => LoadClientsAsync();

    /// <summary>
    /// Dispatches a query via MediatR to fetch all registered clients and updates the observable collection.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous retrieval operation.</returns>
    [RelayCommand]
    public async Task LoadClientsAsync()
    {
        IsBusy       = true;
        ErrorMessage = null;

        try
        {
            var list = await mediator.Send(request: new GetClientsQuery());
            Clients        = [with(collection: list)];
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
    /// Displays the modal dialog window to collect data for a new client.
    /// </summary>
    [RelayCommand]
    public void OpenCreateModal() => IsCreateModalOpen = true;

    /// <summary>
    /// Closes the client creation modal dialog without saving changes.
    /// </summary>
    [RelayCommand]
    public void CloseCreateModal() => IsCreateModalOpen = false;

    /// <summary>
    /// Asynchronously constructs and dispatches a <see cref="CreateClientCommand"/> to persist the new client registry entry.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous persistence operation.</returns>
    [RelayCommand]
    public async Task SaveNewClientAsync()
    {
        IsBusy       = true;
        ErrorMessage = null;

        try
        {
            var countryCode = NewJurisdiction == Jurisdiction.Switzerland ? "CH" : "IT";

            var command = new CreateClientCommand(
                                                  BusinessName: NewBusinessName,
                                                  Jurisdiction: NewJurisdiction,
                                                  FiscalCode: NewJurisdiction == Jurisdiction.Italy ? NewFiscalCode : null,
                                                  VatNumber: NewJurisdiction  == Jurisdiction.Italy ? NewVatNumber : null,
                                                  SdiCode: NewJurisdiction    == Jurisdiction.Italy ? NewSdiCode : null,
                                                  PecEmail: NewJurisdiction   == Jurisdiction.Italy ? NewPecEmail : null,
                                                  SwissUid: NewJurisdiction   == Jurisdiction.Switzerland ? NewSwissUid : null,
                                                  QrIban: NewJurisdiction     == Jurisdiction.Switzerland ? NewQrIban : null,
                                                  CantonalCode: NewJurisdiction == Jurisdiction.Switzerland
                                                                    ? NewProvinceOrCanton
                                                                    : null,
                                                  Street: NewStreet,
                                                  City: NewCity,
                                                  PostalCode: NewPostalCode,
                                                  ProvinceOrCanton: NewProvinceOrCanton,
                                                  CountryCode: countryCode,
                                                  Email: NewEmail,
                                                  PhoneNumber: null);

            var created = await mediator.Send(request: command);
            Clients.Add(item: created);
            SelectedClient    = created;
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
