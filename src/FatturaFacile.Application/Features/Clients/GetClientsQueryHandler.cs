// <copyright file="GetClientsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.Clients;

/// <summary>
/// Represents get clients query handler.
/// </summary>
/// <param name="clientRepository">The client repository.</param>
/// <param name="mapper">The mapper.</param>
public sealed class GetClientsQueryHandler(IClientRepository clientRepository, IMapper mapper)
    : IRequestHandler<GetClientsQuery, IReadOnlyList<ClientDto>>
{
    private readonly IClientRepository clientRepository = clientRepository;

    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Handle.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public async Task<IReadOnlyList<ClientDto>> Handle(
        GetClientsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var clients = request.ActiveOnly
            ? await clientRepository.GetActiveClientsAsync(cancellationToken)
            : await clientRepository.GetAllAsync(cancellationToken);

        return mapper.Map<IReadOnlyList<ClientDto>>(clients);
    }
}
