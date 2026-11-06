// <copyright file="GetCurrentTenantQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Tenants.Queries;

/// <summary>
/// Handler for <see cref="GetCurrentTenantQuery"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetCurrentTenantQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The tenant repository.</param>
/// <param name="currentTenantService">The active tenant.</param>
/// <param name="mapper">The mapper.</param>
public sealed class GetCurrentTenantQueryHandler(IRepository<Tenant> repository, ICurrentTenantService currentTenantService, IMapper mapper)
    : IRequestHandler<GetCurrentTenantQuery, TenantDto?>
{
    /// <inheritdoc/>
    public async Task<TenantDto?> Handle(GetCurrentTenantQuery request, CancellationToken cancellationToken)
    {
        var tenant = await repository.GetByIdAsync(currentTenantService.TenantId, cancellationToken).ConfigureAwait(false);
        return tenant is null ? null : mapper.Map<TenantDto>(tenant);
    }
}
