// <copyright file="GetRebateAgreementDetailQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Handles <see cref="GetRebateAgreementDetailQuery"/>.
/// </summary>
/// <param name="cache">The local cache.</param>
public sealed class GetRebateAgreementDetailQueryHandler(ISupplierRebatesCache cache) : IRequestHandler<GetRebateAgreementDetailQuery, RebateAgreementDetail?>
{
    /// <inheritdoc/>
    public Task<RebateAgreementDetail?> Handle(GetRebateAgreementDetailQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return LoadAsync(request.AgreementId, cancellationToken);
    }

    private async Task<RebateAgreementDetail?> LoadAsync(string agreementId, CancellationToken cancellationToken)
    {
        var agreement = await cache.FindAsync(RebateCacheKinds.Agreement, agreementId, cancellationToken).ConfigureAwait(false);
        if (agreement is null)
        {
            return null;
        }

        var progress = await cache.FindAsync(RebateCacheKinds.Progress, agreementId, cancellationToken).ConfigureAwait(false);
        var runs = await cache.FindAsync(RebateCacheKinds.Runs, agreementId, cancellationToken).ConfigureAwait(false);
        var proposals = await cache.FindAsync(RebateCacheKinds.Proposals, agreementId, cancellationToken).ConfigureAwait(false);

        return new RebateAgreementDetail(
            SupplierRebateParser.ParseAgreement(agreement.Json),
            progress is null ? null : SupplierRebateParser.ParseProgress(progress.Json),
            SupplierRebateParser.ParseRuns(runs?.Json),
            SupplierRebateParser.ParseProposals(proposals?.Json),
            agreement.SyncedAtUtc);
    }
}
