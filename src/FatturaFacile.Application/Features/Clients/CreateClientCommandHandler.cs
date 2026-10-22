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
    ITenantProvider tenantProvider,
    IMapper mapper) : IRequestHandler<CreateClientCommand, ClientDto>
{
    private readonly IClientRepository clientRepository = clientRepository;

    private readonly ITenantProvider tenantProvider = tenantProvider;

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
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var address = new Address(
            request.Street,
            request.City,
            request.PostalCode,
            request.ProvinceOrCanton,
            request.CountryCode);
        Client client;

        if (request.Jurisdiction == Jurisdiction.Italy)
        {
            var cf = new FiscalCode(request.FiscalCode!);
            var vat = new VatNumber(
                request.VatNumber!,
                "IT");
            client = Client.CreateItalian(
                tenantProvider.CurrentTenantId,
                request.BusinessName,
                cf,
                vat,
                request.SdiCode ?? "0000000",
                request.PecEmail ?? string.Empty,
                address,
                request.Email,
                request.PhoneNumber,
                request.PaymentTermDays);
        }
        else
        {
            var qrIban = !string.IsNullOrWhiteSpace(request.QrIban) ? new QrIban(request.QrIban) : null;
            client = Client.CreateSwiss(
                tenantProvider.CurrentTenantId,
                request.BusinessName,
                request.SwissUid!,
                request.CantonalCode ?? "TI",
                qrIban,
                address,
                request.Email,
                request.PhoneNumber,
                request.PaymentTermDays);
        }

        await clientRepository.AddAsync(
            client,
            cancellationToken);
        return mapper.Map<ClientDto>(client);
    }
}
