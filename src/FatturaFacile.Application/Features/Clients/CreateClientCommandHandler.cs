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
/// Represents create client command handler.
/// </summary>
/// <param name="clientRepository">The client repository.</param>
/// <param name="tenantProvider">The tenant provider.</param>
/// <param name="mapper">The mapper.</param>
public sealed class CreateClientCommandHandler(IClientRepository clientRepository, ITenantProvider tenantProvider, IMapper mapper) : IRequestHandler<CreateClientCommand, ClientDto>
{
    private readonly IClientRepository _clientRepository = clientRepository;

    private readonly ITenantProvider _tenantProvider = tenantProvider;

    private readonly IMapper _mapper = mapper;

/// <summary>
    /// Handle.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public async Task<ClientDto> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        var address = new Address(request.Street, request.City, request.PostalCode, request.ProvinceOrCanton, request.CountryCode);
        Client client;

        if (request.Jurisdiction == Jurisdiction.Italy)
        {
            var cf = new FiscalCode(request.FiscalCode!);
            var vat = new VatNumber(request.VatNumber!, "IT");
            client = Client.CreateItalian(
                _tenantProvider.CurrentTenantId,
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
                _tenantProvider.CurrentTenantId,
                request.BusinessName,
                request.SwissUid!,
                request.CantonalCode ?? "TI",
                qrIban,
                address,
                request.Email,
                request.PhoneNumber,
                request.PaymentTermDays);
        }

        await _clientRepository.AddAsync(client, cancellationToken);
        return _mapper.Map<ClientDto>(client);
    }
}
