// <copyright file="CreateClientCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Crm;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Repositories;
using FatturaFacile.Domain.ValueObjects;

using MediatR;

namespace FatturaFacile.Application.Features.Clients;

/// <summary>
/// Handles <see cref="CreateClientCommand"/> by constructing valid jurisdiction-specific client aggregate entities and persisting them.
/// </summary>
/// <param name="clientRepository">The repository used to persist client domain entities.</param>
/// <param name="tenantProvider">The tenant provider supplying the active tenant isolation boundary.</param>
/// <param name="mapper">The mapper used to transform persisted entities into <see cref="ClientDto"/>.</param>
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

            client = Client.CreateItalian(
                                          tenantId: tenantProvider.CurrentTenantId,
                                          businessName: request.BusinessName,
                                          fiscalCode: cf,
                                          vatNumber: vat,
                                          sdiCode: request.SdiCode   ?? "0000000",
                                          pecEmail: request.PecEmail ?? string.Empty,
                                          address: address,
                                          email: request.Email,
                                          phone: request.PhoneNumber,
                                          paymentTermDays: request.PaymentTermDays);
        }
        else
        {
            var qrIban = !string.IsNullOrWhiteSpace(value: request.QrIban) ? new QrIban(value: request.QrIban) : null;

            client = Client.CreateSwiss(
                                        tenantId: tenantProvider.CurrentTenantId,
                                        businessName: request.BusinessName,
                                        swissUid: request.SwissUid!,
                                        cantonalCode: request.CantonalCode ?? "TI",
                                        qrIban: qrIban,
                                        address: address,
                                        email: request.Email,
                                        phone: request.PhoneNumber,
                                        paymentTermDays: request.PaymentTermDays);
        }

        await clientRepository.AddAsync(
                                        entity: client,
                                        cancellationToken: cancellationToken);

        return mapper.Map<ClientDto>(source: client);
    }

}
