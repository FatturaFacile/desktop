// <copyright file="ISupplierRebatesWriteService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// The server-validated actions of the supplier rebates. They need the server (versions, the approval workflow and the ledger
/// postings are decided there): offline they change nothing and are not queued, and the result says so. After an accepted action
/// the cache of the agreement is refreshed.
/// </summary>
public interface ISupplierRebatesWriteService
{
    /// <summary>
    /// Creates an agreement, or updates one.
    /// </summary>
    /// <param name="agreementId">The agreement to update, or null to create.</param>
    /// <param name="input">The agreement.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result; its data is the saved agreement.</returns>
    public Task<SupplierRebatesResult> SaveAgreementAsync(string? agreementId, RebateAgreementInput input, CancellationToken cancellationToken);

    /// <summary>
    /// Activates, closes or deletes an agreement.
    /// </summary>
    /// <param name="agreementId">The agreement identifier.</param>
    /// <param name="action">The status change.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<SupplierRebatesResult> ChangeAgreementAsync(string agreementId, RebateAgreementAction action, CancellationToken cancellationToken);

    /// <summary>
    /// Plans a contribution, or changes one.
    /// </summary>
    /// <param name="agreementId">The agreement identifier.</param>
    /// <param name="contributionId">The contribution to change, or null to add.</param>
    /// <param name="input">The contribution.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<SupplierRebatesResult> SaveContributionAsync(string agreementId, string? contributionId, RebateContributionInput input, CancellationToken cancellationToken);

    /// <summary>
    /// Removes a planned contribution.
    /// </summary>
    /// <param name="agreementId">The agreement identifier.</param>
    /// <param name="contributionId">The contribution identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<SupplierRebatesResult> DeleteContributionAsync(string agreementId, string contributionId, CancellationToken cancellationToken);

    /// <summary>
    /// Runs the periodic accrual of an agreement.
    /// </summary>
    /// <param name="agreementId">The agreement identifier.</param>
    /// <param name="periodEnd">The end of the period, or null for today.</param>
    /// <param name="periodStart">The start of the period, or null to let the server derive it.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result; its data is the run.</returns>
    public Task<SupplierRebatesResult> RunAccrualAsync(string agreementId, DateOnly? periodEnd, DateOnly? periodStart, CancellationToken cancellationToken);

    /// <summary>
    /// Reverses a posted accrual run.
    /// </summary>
    /// <param name="agreementId">The agreement identifier.</param>
    /// <param name="runId">The run identifier.</param>
    /// <param name="reason">The reason.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<SupplierRebatesResult> ReverseRunAsync(string agreementId, string runId, string reason, CancellationToken cancellationToken);

    /// <summary>
    /// Proposes the settlement of the open accruals of an agreement.
    /// </summary>
    /// <param name="agreementId">The agreement identifier.</param>
    /// <param name="input">The proposal.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result; its data is the proposal.</returns>
    public Task<SupplierRebatesResult> ProposeSettlementAsync(string agreementId, RebateProposalInput input, CancellationToken cancellationToken);

    /// <summary>
    /// Moves a settlement proposal through the workflow.
    /// </summary>
    /// <param name="agreementId">The agreement identifier.</param>
    /// <param name="proposalId">The proposal identifier.</param>
    /// <param name="action">The workflow step.</param>
    /// <param name="details">The optional data of the step.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result; its data is the proposal.</returns>
    public Task<SupplierRebatesResult> AdvanceSettlementAsync(string agreementId, string proposalId, SettlementAction action, RebateSettlementStep details, CancellationToken cancellationToken);
}
