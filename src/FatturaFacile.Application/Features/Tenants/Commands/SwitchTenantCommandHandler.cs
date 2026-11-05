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
/// <remarks>
/// Initializes a new instance of the
/// <see cref="SwitchTenantCommandHandler"/> class.
/// </remarks>
/// <param name="tenantRepository">The tenant repository.</param>
/// <param name="currentTenantService">The current tenant service.</param>
/// <param name="publisher">The MediatR notification publisher.</param>
public sealed class SwitchTenantCommandHandler(
    IRepository<Tenant> tenantRepository,
    ICurrentTenantService currentTenantService,
    IPublisher publisher)
    : IRequestHandler<SwitchTenantCommand, bool>
{
    /// <inheritdoc/>
    public Task<bool> Handle(
        SwitchTenantCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(
            request,
            cancellationToken);
    }

    private async Task<bool> HandleInternalAsync(
        SwitchTenantCommand request,
        CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository
            .GetByIdAsync(
                request.TenantId,
                cancellationToken)
            .ConfigureAwait(false);

        if (tenant is null)
        {
            return false;
        }

        currentTenantService.SetCurrentTenant(
            tenant.Id,
            tenant.Name,
            tenant.Country,
            tenant.DefaultCurrency);

        await publisher
            .Publish(
                new TenantChangedNotification(
                    tenant.Id,
                    tenant.Name,
                    tenant.Country,
                    tenant.DefaultCurrency),
                cancellationToken)
            .ConfigureAwait(false);

        return true;
    }
}
