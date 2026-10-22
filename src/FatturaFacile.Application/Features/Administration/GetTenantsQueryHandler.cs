// <copyright file="GetTenantsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.Administration;

/// <summary>
/// Handles the execution of <see cref="GetTenantsQuery"/> by querying active tenant repositories.
/// </summary>
/// <param name="tenantRepository">The repository used to access tenant persistence records.</param>
/// <param name="mapper">The mapper instance used to convert domain entities to DTOs.</param>
public sealed class GetTenantsQueryHandler(ITenantRepository tenantRepository, IMapper mapper)
    : IRequestHandler<GetTenantsQuery, IReadOnlyList<TenantDto>>
{
    private readonly ITenantRepository tenantRepository = tenantRepository;

    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Processes the query and retrieves all active tenants mapped to transfer objects.
    /// </summary>
    /// <param name="request">The query request instance.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the query execution returning a read-only list of <see cref="TenantDto"/>.</returns>
    public async Task<IReadOnlyList<TenantDto>> Handle(
        GetTenantsQuery request,
        CancellationToken cancellationToken)
    {
        var tenants = await tenantRepository.GetActiveTenantsAsync(cancellationToken);
        return mapper.Map<IReadOnlyList<TenantDto>>(tenants);
    }
}
