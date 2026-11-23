// <copyright file="SyncSupplierRebatesCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Handles <see cref="SyncSupplierRebatesCommand"/>.
/// </summary>
/// <param name="sync">The sync service.</param>
public sealed class SyncSupplierRebatesCommandHandler(ISupplierRebatesSyncService sync) : IRequestHandler<SyncSupplierRebatesCommand, SupplierRebatesResult>
{
    /// <inheritdoc/>
    public Task<SupplierRebatesResult> Handle(SyncSupplierRebatesCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.Scope switch
        {
            RebateSyncScope.Agreements => sync.SyncAgreementsAsync(cancellationToken),
            RebateSyncScope.Agreement => sync.SyncAgreementAsync(Required(request), cancellationToken),
            RebateSyncScope.BlanketContracts => sync.SyncBlanketContractsAsync(cancellationToken),
            RebateSyncScope.BlanketBalance => sync.SyncBlanketBalanceAsync(Required(request), cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(request), request.Scope, "Unknown supplier rebate sync scope."),
        };
    }

    private static string Required(SyncSupplierRebatesCommand request) =>
        string.IsNullOrWhiteSpace(request.Id) ? throw new ArgumentException("The sync scope needs an identifier.", nameof(request)) : request.Id;
}
