// <copyright file="SupplierRebatesSyncService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// Default <see cref="ISupplierRebatesSyncService"/>: pulls the payloads of a record and saves them together, so a reader sees the
/// old set or the new one, never a mix. A payload that does not parse never replaces a good cache.
/// </summary>
/// <param name="api">The web-api client.</param>
/// <param name="cache">The local cache.</param>
/// <param name="time">The clock.</param>
public sealed class SupplierRebatesSyncService(ISupplierRebatesApiClient api, ISupplierRebatesCache cache, TimeProvider time) : ISupplierRebatesSyncService
{
    /// <inheritdoc/>
    public Task<SupplierRebatesResult> SyncAgreementsAsync(CancellationToken cancellationToken) =>
        RunAsync(() => PullAgreementsAsync(cancellationToken), cancellationToken);

    /// <inheritdoc/>
    public Task<SupplierRebatesResult> SyncAgreementAsync(string agreementId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agreementId);

        return RunAsync(() => PullAgreementAsync(agreementId, cancellationToken), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<SupplierRebatesResult> SyncBlanketContractsAsync(CancellationToken cancellationToken) =>
        RunAsync(() => PullBlanketContractsAsync(cancellationToken), cancellationToken);

    /// <inheritdoc/>
    public Task<SupplierRebatesResult> SyncBlanketBalanceAsync(string contractId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contractId);

        return RunAsync(() => PullBlanketBalanceAsync(contractId, cancellationToken), cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<RebatePayablesResult> LoadPayablesAsync(string supplierId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(supplierId);

        if (!api.IsAuthenticated)
        {
            return new RebatePayablesResult(SupplierRebatesStatus.Unauthorized, []);
        }

        List<RebatePayableInvoice> invoices = [];
        var result = await SupplierRebatesCall.RunAsync(
            async () =>
            {
                var json = await api.GetPassiveInvoicesAsync(cancellationToken).ConfigureAwait(false);
                invoices = [.. SupplierRebateParser.ParsePayables(json, supplierId)];

                return new SupplierRebatesResult(SupplierRebatesStatus.Synced, time.GetUtcNow().UtcDateTime);
            },
            cancellationToken).ConfigureAwait(false);

        return new RebatePayablesResult(result.Status, result.IsSuccess ? invoices : []);
    }

    private Task<SupplierRebatesResult> RunAsync(Func<Task<SupplierRebatesResult>> pull, CancellationToken cancellationToken) =>
        api.IsAuthenticated
            ? SupplierRebatesCall.RunAsync(pull, cancellationToken)
            : Task.FromResult(new SupplierRebatesResult(SupplierRebatesStatus.Unauthorized));

    private async Task<SupplierRebatesResult> PullAgreementsAsync(CancellationToken cancellationToken)
    {
        var json = await api.GetAgreementsAsync(cancellationToken).ConfigureAwait(false);

        // A payload that does not parse must never replace a good cache.
        _ = SupplierRebateParser.ParseAgreements(json);

        return await SaveAsync([new RebateCacheEntry(RebateCacheKinds.Agreements, RebateCacheKinds.AllKey, json)], cancellationToken).ConfigureAwait(false);
    }

    private async Task<SupplierRebatesResult> PullAgreementAsync(string agreementId, CancellationToken cancellationToken)
    {
        var agreement = await api.GetAgreementAsync(agreementId, cancellationToken).ConfigureAwait(false);
        var progress = await api.GetProgressAsync(agreementId, cancellationToken).ConfigureAwait(false);
        var runs = await api.GetRunsAsync(agreementId, cancellationToken).ConfigureAwait(false);
        var proposals = await api.GetProposalsAsync(agreementId, cancellationToken).ConfigureAwait(false);

        _ = SupplierRebateParser.ParseAgreement(agreement);
        _ = SupplierRebateParser.ParseProgress(progress);
        _ = SupplierRebateParser.ParseRuns(runs);
        _ = SupplierRebateParser.ParseProposals(proposals);

        return await SaveAsync(
            [
                new RebateCacheEntry(RebateCacheKinds.Agreement, agreementId, agreement),
                new RebateCacheEntry(RebateCacheKinds.Progress, agreementId, progress),
                new RebateCacheEntry(RebateCacheKinds.Runs, agreementId, runs),
                new RebateCacheEntry(RebateCacheKinds.Proposals, agreementId, proposals),
            ],
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<SupplierRebatesResult> PullBlanketContractsAsync(CancellationToken cancellationToken)
    {
        var json = await api.GetBlanketContractsAsync(cancellationToken).ConfigureAwait(false);
        _ = SupplierRebateParser.ParseBlanketContracts(json);

        return await SaveAsync([new RebateCacheEntry(RebateCacheKinds.BlanketContracts, RebateCacheKinds.AllKey, json)], cancellationToken).ConfigureAwait(false);
    }

    private async Task<SupplierRebatesResult> PullBlanketBalanceAsync(string contractId, CancellationToken cancellationToken)
    {
        var json = await api.GetBlanketBalanceAsync(contractId, cancellationToken).ConfigureAwait(false);
        _ = SupplierRebateParser.ParseBlanketBalance(json);

        return await SaveAsync([new RebateCacheEntry(RebateCacheKinds.BlanketBalance, contractId, json)], cancellationToken).ConfigureAwait(false);
    }

    private async Task<SupplierRebatesResult> SaveAsync(IReadOnlyList<RebateCacheEntry> entries, CancellationToken cancellationToken)
    {
        var now = time.GetUtcNow().UtcDateTime;
        await cache.SaveAsync(entries, now, cancellationToken).ConfigureAwait(false);

        return new SupplierRebatesResult(SupplierRebatesStatus.Synced, now);
    }
}
