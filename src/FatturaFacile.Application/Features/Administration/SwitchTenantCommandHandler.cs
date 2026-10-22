// <copyright file="SwitchTenantCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.Administration;

/// <summary>
/// Handles <see cref="SwitchTenantCommand"/> by validating tenant existence and updating the ambient tenant provider.
/// </summary>
/// <param name="tenantRepository">The repository used to query tenant information.</param>
/// <param name="tenantProvider">The tenant provider updated with the new active tenant context.</param>
/// <param name="mapper">The mapper used to transform the target tenant entity into a DTO.</param>
public sealed class SwitchTenantCommandHandler(
    ITenantRepository tenantRepository,
    ITenantProvider tenantProvider,
    IMapper mapper) : IRequestHandler<SwitchTenantCommand, TenantDto>
{
    private readonly ITenantRepository tenantRepository = tenantRepository;

    private readonly ITenantProvider tenantProvider = tenantProvider;

    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Executes the tenant switch, validating that the target tenant exists and reconfiguring ambient tenant state.
    /// </summary>
    /// <param name="request">The switch tenant command containing the destination tenant identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the switch operation returning the target <see cref="TenantDto"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the requested tenant does not exist in the repository.</exception>
    public async Task<TenantDto> Handle(
        SwitchTenantCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tenant = await tenantRepository.GetByIdAsync(
                         request.TargetTenantId,
                         cancellationToken)
                     ?? throw new InvalidOperationException("Tenant not found.");

        tenantProvider.SetTenant(
            tenant.Id,
            tenant.Code);
        return mapper.Map<TenantDto>(tenant);
    }
}
