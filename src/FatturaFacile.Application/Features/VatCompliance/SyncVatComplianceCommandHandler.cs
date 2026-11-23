// <copyright file="SyncVatComplianceCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.VatCompliance;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Handles <see cref="SyncVatComplianceCommand"/>.
/// </summary>
/// <param name="sync">The sync service.</param>
public sealed class SyncVatComplianceCommandHandler(IVatComplianceSyncService sync) : IRequestHandler<SyncVatComplianceCommand, VatComplianceSyncResult>
{
    /// <inheritdoc/>
    public Task<VatComplianceSyncResult> Handle(SyncVatComplianceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return sync.SyncAsync(request.Year, cancellationToken);
    }
}
