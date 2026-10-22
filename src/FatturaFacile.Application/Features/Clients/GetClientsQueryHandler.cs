// <copyright file="GetClientsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;

using MediatR;

namespace FatturaFacile.Application.Features.Clients;

/// <summary>
/// Handles <see cref="GetClientsQuery"/> by retrieving client domain entities and projecting them into data transfer objects.
/// </summary>
/// <param name="clientRepository">The repository used to access stored client profiles.</param>
/// <param name="mapper">The mapper used to project domain entities to <see cref="ClientDto"/>.</param>
public sealed class GetClientsQueryHandler(IClientRepository clientRepository, IMapper mapper)
    : IRequestHandler<GetClientsQuery, IReadOnlyList<ClientDto>>
{

    /// <summary>
    /// The repository used to access stored client profiles.
    /// </summary>
    private readonly IClientRepository clientRepository = clientRepository;

    /// <summary>
    /// The mapper used to project domain entities to <see cref="ClientDto"/>.
    /// </summary>
    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Executes the query, retrieving active or all clients according to the query specification.
    /// </summary>
    /// <param name="request">The query containing retrieval criteria.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the query execution returning a read-only list of <see cref="ClientDto"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
    public async Task<IReadOnlyList<ClientDto>> Handle(
        GetClientsQuery   request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(argument: request);

        var clients = request.ActiveOnly
                          ? await clientRepository.GetActiveClientsAsync(cancellationToken: cancellationToken)
                          : await clientRepository.GetAllAsync(cancellationToken: cancellationToken);

        return mapper.Map<IReadOnlyList<ClientDto>>(source: clients);
    }

}
