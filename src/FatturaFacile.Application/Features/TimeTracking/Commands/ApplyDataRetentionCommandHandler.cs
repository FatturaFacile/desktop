// <copyright file="ApplyDataRetentionCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.TimeTracking;
using FatturaFacile.Domain.Common;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Commands;

/// <summary>
/// Handles <see cref="ApplyDataRetentionCommand"/>.
/// </summary>
/// <param name="retentionService">The retention service.</param>
/// <param name="currentTenantService">The active company.</param>
public sealed class ApplyDataRetentionCommandHandler(IDataRetentionService retentionService, ICurrentTenantService currentTenantService)
    : IRequestHandler<ApplyDataRetentionCommand, DataRetentionResult>
{
    /// <inheritdoc />
    public Task<DataRetentionResult> Handle(ApplyDataRetentionCommand request, CancellationToken cancellationToken) =>
        retentionService.ApplyAsync(currentTenantService.TenantId, DateTime.Today, cancellationToken);
}
