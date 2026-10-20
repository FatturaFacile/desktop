using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Administration;

public record GetTenantsQuery() : IRequest<IReadOnlyList<TenantDto>>;