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
/// Represents switch tenant command handler.
/// </summary>
/// <param name="tenantRepository">The tenant repository.</param>
/// <param name="tenantProvider">The tenant provider.</param>
/// <param name="mapper">The mapper.</param>
public sealed class SwitchTenantCommandHandler(
    ITenantRepository tenantRepository,
    ITenantProvider tenantProvider,
    IMapper mapper) : IRequestHandler<SwitchTenantCommand, TenantDto>
{
    private readonly ITenantRepository tenantRepository = tenantRepository;

    private readonly ITenantProvider tenantProvider = tenantProvider;

    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Handle.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when target tenant does not exist.</exception>
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
