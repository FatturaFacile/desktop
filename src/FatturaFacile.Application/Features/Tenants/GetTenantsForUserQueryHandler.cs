// <copyright file="GetTenantsForUserQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Common;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Ports;
using MediatR;

namespace FatturaFacile.Application.Features.Tenants;

/// <summary>
/// Handles queries to enumerate tenant workspaces authorized for a given Windows account.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetTenantsForUserQueryHandler"/> class.
/// </remarks>
/// <param name="tenantRepository">The tenant repository dependency.</param>
/// <param name="mapper">The AutoMapper instance for tenant DTO mapping.</param>
public class GetTenantsForUserQueryHandler(ITenantRepository tenantRepository, IMapper mapper) :
    IRequestHandler<GetTenantsForUserQuery, IReadOnlyList<TenantDto>>,
    IQueryHandler<GetTenantsForUserQuery, IReadOnlyList<TenantDto>>
{
    private readonly ITenantRepository _tenantRepository = tenantRepository;
    private readonly IMapper _mapper = mapper;


    /// <summary>
    /// Executes the tenant lookup via MediatR pipeline.
    /// </summary>
    /// <param name="query">The query containing the Windows username.</param>
    /// <param name="cancellationToken">Cancellation token to observe during processing.</param>
    /// <returns>A collection of authorized <see cref="TenantDto"/> instances.</returns>
    public async Task<IReadOnlyList<TenantDto>> Handle(GetTenantsForUserQuery query, CancellationToken cancellationToken)
    {
        var tenants = await _tenantRepository.GetTenantsForWindowsUserAsync(query.WindowsUsername, cancellationToken);
        return _mapper.Map<IReadOnlyList<TenantDto>>(tenants);
    }

    /// <summary>
    /// Executes the tenant lookup via application dispatcher.
    /// </summary>
    /// <param name="query">The query containing the Windows username.</param>
    /// <param name="cancellationToken">Cancellation token to observe during processing.</param>
    /// <returns>A collection of authorized <see cref="TenantDto"/> instances.</returns>
    public Task<IReadOnlyList<TenantDto>> HandleAsync(GetTenantsForUserQuery query, CancellationToken cancellationToken = default) => Handle(query, cancellationToken);
}
