// <copyright file="SwitchTenantCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Tenants.Notifications;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Tenants.Commands;

/// <summary>
/// Handler for <see cref="SwitchTenantCommand"/>.
/// </summary>
public sealed class SwitchTenantCommandHandler : IRequestHandler<SwitchTenantCommand, bool>
{
    private readonly IRepository<Tenant> _tenantRepository;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IPublisher _publisher;

    /// <summary>
    /// Initializes a new instance of the <see cref="SwitchTenantCommandHandler"/> class.
    /// </summary>
    /// <param name="tenantRepository">The tenant repository.</param>
    /// <param name="currentTenantService">The current tenant service.</param>
    /// <param name="publisher">The MediatR notification publisher.</param>
    public SwitchTenantCommandHandler(
        IRepository<Tenant> tenantRepository,
        ICurrentTenantService currentTenantService,
        IPublisher publisher)
    {
        _tenantRepository = tenantRepository;
        _currentTenantService = currentTenantService;
        _publisher = publisher;
    }

    /// <inheritdoc/>
    public async Task<bool> Handle(SwitchTenantCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (tenant == null)
        {
            return false;
        }

        _currentTenantService.SetCurrentTenant(tenant.Id, tenant.Name, tenant.Country, tenant.DefaultCurrency);
        await _publisher.Publish(
            new TenantChangedNotification(tenant.Id, tenant.Name, tenant.Country, tenant.DefaultCurrency),
            cancellationToken).ConfigureAwait(false);

        return true;
    }
}
