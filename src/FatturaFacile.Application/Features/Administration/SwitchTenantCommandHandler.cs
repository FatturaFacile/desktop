using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.Administration;

public sealed class SwitchTenantCommandHandler(ITenantRepository tenantRepository, ITenantProvider tenantProvider, IMapper mapper) : IRequestHandler<SwitchTenantCommand, TenantDto>
{
    private readonly ITenantRepository _tenantRepository = tenantRepository;
    private readonly ITenantProvider _tenantProvider = tenantProvider;
    private readonly IMapper _mapper = mapper;

    public async Task<TenantDto> Handle(SwitchTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TargetTenantId, cancellationToken)
                     ?? throw new InvalidOperationException("Tenant not found.");

        _tenantProvider.SetTenant(tenant.Id, tenant.Code);
        return _mapper.Map<TenantDto>(tenant);
    }
}