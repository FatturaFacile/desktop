using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Administration;

/// <summary>
/// Represents get tenants query.
/// </summary>
public record GetTenantsQuery() : IRequest<IReadOnlyList<TenantDto>>;
