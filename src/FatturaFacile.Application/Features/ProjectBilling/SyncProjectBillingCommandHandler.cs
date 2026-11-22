// <copyright file="SyncProjectBillingCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ProjectBilling;
using MediatR;

namespace FatturaFacile.Application.Features.ProjectBilling;

/// <summary>
/// Handles <see cref="SyncProjectBillingCommand"/>.
/// </summary>
/// <param name="sync">The sync service.</param>
public sealed class SyncProjectBillingCommandHandler(IProjectBillingSyncService sync) : IRequestHandler<SyncProjectBillingCommand, ProjectBillingSyncResult>
{
    /// <inheritdoc/>
    public Task<ProjectBillingSyncResult> Handle(SyncProjectBillingCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return sync.SyncAsync(request.ProjectId, cancellationToken);
    }
}
