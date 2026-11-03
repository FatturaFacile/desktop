// <copyright file="CreateClientCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Crm;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;
using MediatR;

namespace FatturaFacile.Application.Features.Clients;

/// <summary>
/// Handles the <see cref="CreateClientCommand"/> request to register a new client profile under Italian or Swiss jurisdiction.
/// </summary>
/// <param name="clientRepository">The repository used to persist client domain entities.</param>
/// <param name="tenantProvider">The tenant context provider.</param>
/// <param name="mapper">The AutoMapper instance.</param>
public sealed class CreateClientCommandHandler(
    IClientRepository clientRepository,
    ITenantProvider   tenantProvider,
    IMapper           mapper) : IRequestHandler<CreateClientCommand, ClientDto>
{

    /// <summary>
    /// The repository used to persist client domain entities.
    /// </summary>
    private readonly IClientRepository clientRepository = clientRepository;

    /// <summary>
    /// The tenant provider supplying the active tenant isolation boundary.
    /// </summary>
    private readonly ITenantProvider tenantProvider = tenantProvider;

    /// <summary>
    /// The mapper used to transform persisted entities into <see cref="ClientDto"/>.
    /// </summary>
    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Processes client creation, instantiating an Italian or Swiss client domain entity with value object validation, and saves the entity.
    /// </summary>
    /// <param name="request">The command describing client profile data and jurisdiction parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the creation operation returning the newly persisted <see cref="ClientDto"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
    public async Task<ClientDto> Handle(
        CreateClientCommand request,
        CancellationToken   cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(argument: request);

        var address = new Address(
                                  street: request.Street,
                                  city: request.City,
                                  postalCode: request.PostalCode,
                                  provinceOrCanton: request.ProvinceOrCanton,
                                  countryCode: request.CountryCode);

        Client client;

        if (request.Jurisdiction == Jurisdiction.Italy)
        {
            var cf = new FiscalCode(value: request.FiscalCode!);

            var vat = new VatNumber(
                                    value: request.VatNumber!,
                                    countryCode: "IT");

            var data = new ItalianClientData(
                TenantId: tenantProvider.CurrentTenantId,
                BusinessName: request.BusinessName,
                FiscalCode: cf,
                VatNumber: vat,
                SdiCode: request.SdiCode ?? "0000000",
                PecEmail: request.PecEmail ?? string.Empty,
                Address: address,
                Email: request.Email,
                Phone: request.PhoneNumber,
                PaymentTermDays: request.PaymentTermDays);

            client = Client.CreateItalian(data: data);
        }
        else
        {
            var qrIban = !string.IsNullOrWhiteSpace(value: request.QrIban) ? new QrIban(value: request.QrIban) : null;

            var data = new SwissClientData(
                TenantId: tenantProvider.CurrentTenantId,
                BusinessName: request.BusinessName,
                SwissUid: request.SwissUid!,
                CantonalCode: request.CantonalCode ?? "TI",
                QrIban: qrIban,
                Address: address,
                Email: request.Email,
                Phone: request.PhoneNumber,
                PaymentTermDays: request.PaymentTermDays);

            client = Client.CreateSwiss(data: data);
        }

        await clientRepository.AddAsync(
                                        entity: client,
                                        cancellationToken: cancellationToken);

        return mapper.Map<ClientDto>(source: client);
    }

}
