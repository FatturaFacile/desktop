using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.Clients;

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