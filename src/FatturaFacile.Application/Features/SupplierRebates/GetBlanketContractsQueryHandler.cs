// <copyright file="GetBlanketContractsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Handles <see cref="GetBlanketContractsQuery"/>.
/// </summary>
/// <param name="cache">The local cache.</param>
public sealed class GetBlanketContractsQueryHandler(ISupplierRebatesCache cache) : IRequestHandler<GetBlanketContractsQuery, RebateCached<IReadOnlyList<BlanketContract>>>
{
    /// <inheritdoc/>
    public Task<RebateCached<IReadOnlyList<BlanketContract>>> Handle(GetBlanketContractsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return LoadAsync(cancellationToken);
    }

    private async Task<RebateCached<IReadOnlyList<BlanketContract>>> LoadAsync(CancellationToken cancellationToken)
    {
        var stored = await cache.FindAsync(RebateCacheKinds.BlanketContracts, RebateCacheKinds.AllKey, cancellationToken).ConfigureAwait(false);

        return stored is null
            ? new RebateCached<IReadOnlyList<BlanketContract>>([], SyncedAtUtc: null)
            : new RebateCached<IReadOnlyList<BlanketContract>>(SupplierRebateParser.ParseBlanketContracts(stored.Json), stored.SyncedAtUtc);
    }
}
