// <copyright file="GetRebateAgreementsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Handles <see cref="GetRebateAgreementsQuery"/>.
/// </summary>
/// <param name="cache">The local cache.</param>
public sealed class GetRebateAgreementsQueryHandler(ISupplierRebatesCache cache) : IRequestHandler<GetRebateAgreementsQuery, RebateCached<IReadOnlyList<RebateAgreement>>>
{
    /// <inheritdoc/>
    public Task<RebateCached<IReadOnlyList<RebateAgreement>>> Handle(GetRebateAgreementsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return LoadAsync(cancellationToken);
    }

    private async Task<RebateCached<IReadOnlyList<RebateAgreement>>> LoadAsync(CancellationToken cancellationToken)
    {
        var stored = await cache.FindAsync(RebateCacheKinds.Agreements, RebateCacheKinds.AllKey, cancellationToken).ConfigureAwait(false);

        return stored is null
            ? new RebateCached<IReadOnlyList<RebateAgreement>>([], SyncedAtUtc: null)
            : new RebateCached<IReadOnlyList<RebateAgreement>>(SupplierRebateParser.ParseAgreements(stored.Json), stored.SyncedAtUtc);
    }
}
