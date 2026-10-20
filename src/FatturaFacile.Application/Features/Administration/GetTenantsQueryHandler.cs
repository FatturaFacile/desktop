using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.Administration;

public sealed class GetTenantsQueryHandler(ITenantRepository tenantRepository, IMapper mapper) : IRequestHandler<GetTenantsQuery, IReadOnlyList<TenantDto>>
{
    private readonly ITenantRepository _tenantRepository = tenantRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<IReadOnlyList<TenantDto>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await _tenantRepository.GetActiveTenantsAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<TenantDto>>(tenants);
    }
}