// <copyright file="SupplierRebatesWriteService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// Default <see cref="ISupplierRebatesWriteService"/>: sends the action to the server and, once it is accepted, refreshes the cache of
/// the agreement and of the agreement list (best effort: a failed refresh never turns an accepted action into a failure).
/// </summary>
/// <param name="api">The web-api client.</param>
/// <param name="sync">The sync service that refreshes the cache.</param>
/// <param name="time">The clock.</param>
public sealed class SupplierRebatesWriteService(ISupplierRebatesApiClient api, ISupplierRebatesSyncService sync, TimeProvider time) : ISupplierRebatesWriteService
{
    /// <inheritdoc/>
    public Task<SupplierRebatesResult> SaveAgreementAsync(string? agreementId, RebateAgreementInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var body = SupplierRebatesRequestWriter.Agreement(input);

        return RunAsync(
            agreementId,
            () => agreementId is null ? api.CreateAgreementAsync(body, cancellationToken) : api.UpdateAgreementAsync(agreementId, body, cancellationToken),
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task<SupplierRebatesResult> ChangeAgreementAsync(string agreementId, RebateAgreementAction action, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agreementId);

        var isDelete = action == RebateAgreementAction.Delete;
        var activate = action == RebateAgreementAction.Activate;

        // A deleted agreement has nothing to refresh but the list.
        return RunAsync(
            isDelete ? null : agreementId,
            () => isDelete ? api.DeleteAgreementAsync(agreementId, cancellationToken) : api.ChangeAgreementStatusAsync(agreementId, activate, cancellationToken),
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task<SupplierRebatesResult> SaveContributionAsync(string agreementId, string? contributionId, RebateContributionInput input, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agreementId);
        ArgumentNullException.ThrowIfNull(input);

        var body = SupplierRebatesRequestWriter.Contribution(input);

        return RunAsync(
            agreementId,
            () => contributionId is null ? api.AddContributionAsync(agreementId, body, cancellationToken) : api.UpdateContributionAsync(contributionId, body, cancellationToken),
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task<SupplierRebatesResult> DeleteContributionAsync(string agreementId, string contributionId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agreementId);
        ArgumentException.ThrowIfNullOrWhiteSpace(contributionId);

        return RunAsync(agreementId, () => api.DeleteContributionAsync(contributionId, cancellationToken), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<SupplierRebatesResult> RunAccrualAsync(string agreementId, DateOnly? periodEnd, DateOnly? periodStart, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agreementId);

        var body = SupplierRebatesRequestWriter.Accrual(periodEnd, periodStart);

        return RunAsync(agreementId, () => api.RunAccrualAsync(agreementId, body, cancellationToken), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<SupplierRebatesResult> ReverseRunAsync(string agreementId, string runId, string reason, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agreementId);
        ArgumentException.ThrowIfNullOrWhiteSpace(runId);

        var body = SupplierRebatesRequestWriter.Reversal(reason);

        return RunAsync(agreementId, () => api.ReverseRunAsync(runId, body, cancellationToken), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<SupplierRebatesResult> ProposeSettlementAsync(string agreementId, RebateProposalInput input, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agreementId);
        ArgumentNullException.ThrowIfNull(input);

        var body = SupplierRebatesRequestWriter.Proposal(input);

        return RunAsync(agreementId, () => api.ProposeSettlementAsync(agreementId, body, cancellationToken), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<SupplierRebatesResult> AdvanceSettlementAsync(string agreementId, string proposalId, SettlementAction action, RebateSettlementStep details, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agreementId);
        ArgumentException.ThrowIfNullOrWhiteSpace(proposalId);
        ArgumentNullException.ThrowIfNull(details);

        var body = SupplierRebatesRequestWriter.SettlementStep(details);

        return RunAsync(agreementId, () => api.AdvanceSettlementAsync(proposalId, action, body, cancellationToken), cancellationToken);
    }

    private static string? IdOf(string? data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return null;
        }

        try
        {
            return SupplierRebateParser.ParseAgreement(data).Id is { Length: > 0 } id ? id : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private async Task<SupplierRebatesResult> RunAsync(string? refreshAgreementId, Func<Task<string>> send, CancellationToken cancellationToken)
    {
        if (!api.IsAuthenticated)
        {
            return new SupplierRebatesResult(SupplierRebatesStatus.Unauthorized);
        }

        var result = await SupplierRebatesCall.RunAsync(
            async () =>
            {
                var data = await send().ConfigureAwait(false);

                return new SupplierRebatesResult(SupplierRebatesStatus.Synced, time.GetUtcNow().UtcDateTime, Data: data);
            },
            cancellationToken).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            await RefreshAsync(refreshAgreementId, result.Data, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    private async Task RefreshAsync(string? agreementId, string? data, CancellationToken cancellationToken)
    {
        _ = await sync.SyncAgreementsAsync(cancellationToken).ConfigureAwait(false);

        // A new agreement has no identifier yet: it comes back in the answer.
        var id = agreementId ?? IdOf(data);
        if (id is not null)
        {
            _ = await sync.SyncAgreementAsync(id, cancellationToken).ConfigureAwait(false);
        }
    }
}
