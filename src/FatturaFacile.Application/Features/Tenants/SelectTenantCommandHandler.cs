// <copyright file="SelectTenantCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Common;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Ports;
using MediatR;

namespace FatturaFacile.Application.Features.Tenants;

/// <summary>
/// Handles tenant selection commands by verifying tenant existence and updating the ambient application tenant state.
/// </summary>
public class SelectTenantCommandHandler : 
    IRequestHandler<SelectTenantCommand, TenantDto>,
    ICommandHandler<SelectTenantCommand, TenantDto>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectTenantCommandHandler"/> class.
    /// </summary>
    /// <param name="tenantRepository">The tenant repository dependency.</param>
    /// <param name="currentTenantService">The ambient tenant context service.</param>
    /// <param name="mapper">The AutoMapper instance for DTO mapping.</param>
    public SelectTenantCommandHandler(
        ITenantRepository tenantRepository,
        ICurrentTenantService currentTenantService,
        IMapper mapper)
    {
        _tenantRepository = tenantRepository;
        _currentTenantService = currentTenantService;
        _mapper = mapper;
    }

    /// <summary>
    /// Executes the tenant selection workflow via MediatR pipeline.
    /// </summary>
    /// <param name="command">The tenant selection parameters.</param>
    /// <param name="cancellationToken">Cancellation token to observe during processing.</param>
    /// <returns>The newly activated <see cref="TenantDto"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the target tenant identifier is not found.</exception>
    public async Task<TenantDto> Handle(SelectTenantCommand command, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetTenantByIdAsync(command.TenantId, cancellationToken);
        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant with ID '{command.TenantId}' was not found.");
        }

        var dto = _mapper.Map<TenantDto>(tenant);
        _currentTenantService.TenantId = tenant.Id;
        _currentTenantService.CurrentTenant = dto;

        return dto;
    }

    /// <summary>
    /// Executes the tenant selection workflow via application dispatcher.
    /// </summary>
    /// <param name="command">The tenant selection parameters.</param>
    /// <param name="cancellationToken">Cancellation token to observe during processing.</param>
    /// <returns>The newly activated <see cref="TenantDto"/>.</returns>
    public Task<TenantDto> HandleAsync(SelectTenantCommand command, CancellationToken cancellationToken = default)
    {
        return Handle(command, cancellationToken);
    }
}
