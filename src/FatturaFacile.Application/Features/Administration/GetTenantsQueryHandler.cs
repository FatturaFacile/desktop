// <copyright file="GetTenantsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.Administration;

/// <summary>
/// Represents get tenants query handler.
/// </summary>
/// <param name="tenantRepository">The tenant repository.</param>
/// <param name="mapper">The mapper.</param>
public sealed class GetTenantsQueryHandler(ITenantRepository tenantRepository, IMapper mapper)
    : IRequestHandler<GetTenantsQuery, IReadOnlyList<TenantDto>>
{
    private readonly ITenantRepository tenantRepository = tenantRepository;

    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Handle.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public async Task<IReadOnlyList<TenantDto>> Handle(
        GetTenantsQuery request,
        CancellationToken cancellationToken)
    {
        var tenants = await tenantRepository.GetActiveTenantsAsync(cancellationToken);
        return mapper.Map<IReadOnlyList<TenantDto>>(tenants);
    }
}
