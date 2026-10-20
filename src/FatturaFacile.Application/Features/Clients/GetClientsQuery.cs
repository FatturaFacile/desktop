using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Clients;

public record GetClientsQuery(bool ActiveOnly = true) : IRequest<IReadOnlyList<ClientDto>>;