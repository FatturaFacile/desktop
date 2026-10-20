using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Clients;

/// <summary>
/// Represents get clients query.
/// </summary>
/// <param name="ActiveOnly">The active only.</param>
public record GetClientsQuery(bool ActiveOnly = true) : IRequest<IReadOnlyList<ClientDto>>;
