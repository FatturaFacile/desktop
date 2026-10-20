using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Crm;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Repositories;
using FatturaFacile.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace FatturaFacile.Application.Features.Clients;

public record CreateClientCommand(
    string BusinessName,
    Jurisdiction Jurisdiction,
    string? FiscalCode,
    string? VatNumber,
    string? SdiCode,
    string? PecEmail,
    string? SwissUid,
    string? QrIban,
    string? CantonalCode,
    string Street,
    string City,
    string PostalCode,
    string ProvinceOrCanton,
    string CountryCode,
    string Email,
    string? PhoneNumber,
    int PaymentTermDays = 30) : IRequest<ClientDto>;

public sealed class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        _ = RuleFor(x => x.BusinessName).NotEmpty().MaximumLength(200);
        _ = RuleFor(x => x.Email).NotEmpty().EmailAddress();
        _ = RuleFor(x => x.Street).NotEmpty();
        _ = RuleFor(x => x.City).NotEmpty();
        _ = RuleFor(x => x.PostalCode).NotEmpty();

        _ = When(x => x.Jurisdiction == Jurisdiction.Italy, () =>
        {
            _ = RuleFor(x => x.FiscalCode).NotEmpty();
            _ = RuleFor(x => x.VatNumber).NotEmpty();
        });

        _ = When(x => x.Jurisdiction == Jurisdiction.Switzerland, () => _ = RuleFor(x => x.SwissUid).NotEmpty());
    }
}

public sealed class CreateClientCommandHandler(IClientRepository clientRepository, ITenantProvider tenantProvider, IMapper mapper) : IRequestHandler<CreateClientCommand, ClientDto>
{
    private readonly IClientRepository _clientRepository = clientRepository;
    private readonly ITenantProvider _tenantProvider = tenantProvider;
    private readonly IMapper _mapper = mapper;

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

public record GetClientsQuery(bool ActiveOnly = true) : IRequest<IReadOnlyList<ClientDto>>;

public sealed class GetClientsQueryHandler(IClientRepository clientRepository, IMapper mapper) : IRequestHandler<GetClientsQuery, IReadOnlyList<ClientDto>>
{
    private readonly IClientRepository _clientRepository = clientRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<IReadOnlyList<ClientDto>> Handle(GetClientsQuery request, CancellationToken cancellationToken)
    {
        var clients = request.ActiveOnly
            ? await _clientRepository.GetActiveClientsAsync(cancellationToken)
            : await _clientRepository.GetAllAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<ClientDto>>(clients);
    }
}
