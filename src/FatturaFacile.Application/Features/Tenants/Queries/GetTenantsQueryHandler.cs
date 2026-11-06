// <copyright file="GetTenantsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using AutoMapper.QueryableExtensions;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Tenants.Queries;

/// <summary>
/// Handler for <see cref="GetTenantsQuery"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetTenantsQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The tenant repository.</param>
/// <param name="mapper">The AutoMapper instance.</param>
/// <param name="currentUserService">The signed-in user (only accessible tenants are listed).</param>
public sealed class GetTenantsQueryHandler(IRepository<Tenant> repository, IMapper mapper, ICurrentUserService currentUserService)
    : IRequestHandler<GetTenantsQuery, IReadOnlyList<TenantDto>>
{

    /// <inheritdoc/>
    public Task<IReadOnlyList<TenantDto>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        var accessible = currentUserService.User?.TenantRoles.Keys.ToList() ?? [];

        IReadOnlyList<TenantDto> result = repository.Query()
            .Where(t => accessible.Contains(t.Id))
            .OrderBy(t => t.Name)
            .ProjectTo<TenantDto>(mapper.ConfigurationProvider)
            .ToList();

        return Task.FromResult(result);
    }
}
