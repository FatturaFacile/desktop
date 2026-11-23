// <copyright file="GetBlanketBalanceQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Handles <see cref="GetBlanketBalanceQuery"/>.
/// </summary>
/// <param name="cache">The local cache.</param>
public sealed class GetBlanketBalanceQueryHandler(ISupplierRebatesCache cache) : IRequestHandler<GetBlanketBalanceQuery, RebateCached<BlanketBalance>?>
{
    /// <inheritdoc/>
    public Task<RebateCached<BlanketBalance>?> Handle(GetBlanketBalanceQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return LoadAsync(request.ContractId, cancellationToken);
    }

    private async Task<RebateCached<BlanketBalance>?> LoadAsync(string contractId, CancellationToken cancellationToken)
    {
        var stored = await cache.FindAsync(RebateCacheKinds.BlanketBalance, contractId, cancellationToken).ConfigureAwait(false);

        return stored is null ? null : new RebateCached<BlanketBalance>(SupplierRebateParser.ParseBlanketBalance(stored.Json), stored.SyncedAtUtc);
    }
}
