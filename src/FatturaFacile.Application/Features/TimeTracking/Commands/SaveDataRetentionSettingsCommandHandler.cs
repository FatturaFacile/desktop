// <copyright file="SaveDataRetentionSettingsCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Commands;

/// <summary>
/// Handles <see cref="SaveDataRetentionSettingsCommand"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="currentTenantService">The active company.</param>
public sealed class SaveDataRetentionSettingsCommandHandler(IUnitOfWork unitOfWork, ICurrentTenantService currentTenantService)
    : IRequestHandler<SaveDataRetentionSettingsCommand, bool>
{
    /// <inheritdoc />
    public Task<bool> Handle(SaveDataRetentionSettingsCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.Years, cancellationToken);
    }

    private async Task<bool> HandleInternalAsync(int years, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<Tenant>();
        var tenant = await repository.GetByIdAsync(currentTenantService.TenantId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Azienda non trovata.");

        tenant.TimeRecordRetentionYears = years;
        repository.Update(tenant);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
