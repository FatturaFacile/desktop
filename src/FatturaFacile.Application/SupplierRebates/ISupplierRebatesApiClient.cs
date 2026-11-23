// <copyright file="ISupplierRebatesApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// Access to the supplier rebate (`/api/v1/rebates`) and blanket agreement (`/api/v1/procurement/contracts`) endpoints of the
/// web-api. Every method returns the raw `data` payload of the answer (`{}` when the answer has no body). A network failure surfaces
/// as an <see cref="HttpRequestException"/> without status code; an HTTP error as an <see cref="HttpRequestException"/> whose
/// status code is set and whose message is the message of the server.
/// </summary>
public interface ISupplierRebatesApiClient
{
    /// <summary>
    /// Gets a value indicating whether a user is signed in to the web-api.
    /// </summary>
    public bool IsAuthenticated { get; }

    /// <summary>
    /// Reads the purchase rebate agreements.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` array.</returns>
    public Task<string> GetAgreementsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Reads one agreement with its tiers and contributions.
    /// </summary>
    /// <param name="agreementId">The agreement identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object.</returns>
    public Task<string> GetAgreementAsync(string agreementId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the live progress of an agreement.
    /// </summary>
    /// <param name="agreementId">The agreement identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object.</returns>
    public Task<string> GetProgressAsync(string agreementId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the accrual runs of an agreement.
    /// </summary>
    /// <param name="agreementId">The agreement identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` array.</returns>
    public Task<string> GetRunsAsync(string agreementId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the settlement proposals of an agreement.
    /// </summary>
    /// <param name="agreementId">The agreement identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` array.</returns>
    public Task<string> GetProposalsAsync(string agreementId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the blanket agreements of the picker.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` array.</returns>
    public Task<string> GetBlanketContractsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Reads the balance of a blanket agreement.
    /// </summary>
    /// <param name="contractId">The blanket agreement identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object.</returns>
    public Task<string> GetBlanketBalanceAsync(string contractId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the supplier invoices a settlement can be set off against.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The answer as sent by the server: the invoice list.</returns>
    public Task<string> GetPassiveInvoicesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates a draft agreement.
    /// </summary>
    /// <param name="jsonBody">The request body.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object of the new agreement.</returns>
    public Task<string> CreateAgreementAsync(string jsonBody, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an agreement.
    /// </summary>
    /// <param name="agreementId">The agreement identifier.</param>
    /// <param name="jsonBody">The request body.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object of the agreement.</returns>
    public Task<string> UpdateAgreementAsync(string agreementId, string jsonBody, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a draft agreement.
    /// </summary>
    /// <param name="agreementId">The agreement identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty object.</returns>
    public Task<string> DeleteAgreementAsync(string agreementId, CancellationToken cancellationToken);

    /// <summary>
    /// Activates or closes an agreement.
    /// </summary>
    /// <param name="agreementId">The agreement identifier.</param>
    /// <param name="activate">True to activate a draft, false to close an active agreement.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object of the agreement.</returns>
    public Task<string> ChangeAgreementStatusAsync(string agreementId, bool activate, CancellationToken cancellationToken);

    /// <summary>
    /// Plans a contribution on an agreement.
    /// </summary>
    /// <param name="agreementId">The agreement identifier.</param>
    /// <param name="jsonBody">The request body.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object of the contribution.</returns>
    public Task<string> AddContributionAsync(string agreementId, string jsonBody, CancellationToken cancellationToken);

    /// <summary>
    /// Changes a planned contribution.
    /// </summary>
    /// <param name="contributionId">The contribution identifier.</param>
    /// <param name="jsonBody">The request body.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object of the contribution.</returns>
    public Task<string> UpdateContributionAsync(string contributionId, string jsonBody, CancellationToken cancellationToken);

    /// <summary>
    /// Removes a planned contribution.
    /// </summary>
    /// <param name="contributionId">The contribution identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty object.</returns>
    public Task<string> DeleteContributionAsync(string contributionId, CancellationToken cancellationToken);

    /// <summary>
    /// Runs the periodic accrual of an agreement (idempotent per period end).
    /// </summary>
    /// <param name="agreementId">The agreement identifier.</param>
    /// <param name="jsonBody">The request body.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object of the run.</returns>
    public Task<string> RunAccrualAsync(string agreementId, string jsonBody, CancellationToken cancellationToken);

    /// <summary>
    /// Reverses a posted accrual run.
    /// </summary>
    /// <param name="runId">The run identifier.</param>
    /// <param name="jsonBody">The request body with the reason.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object of the run.</returns>
    public Task<string> ReverseRunAsync(string runId, string jsonBody, CancellationToken cancellationToken);

    /// <summary>
    /// Proposes the settlement of the open accruals of an agreement.
    /// </summary>
    /// <param name="agreementId">The agreement identifier.</param>
    /// <param name="jsonBody">The request body.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object of the proposal.</returns>
    public Task<string> ProposeSettlementAsync(string agreementId, string jsonBody, CancellationToken cancellationToken);

    /// <summary>
    /// Moves a settlement proposal through the workflow.
    /// </summary>
    /// <param name="proposalId">The proposal identifier.</param>
    /// <param name="action">The workflow step.</param>
    /// <param name="jsonBody">The request body.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object of the proposal.</returns>
    public Task<string> AdvanceSettlementAsync(string proposalId, SettlementAction action, string jsonBody, CancellationToken cancellationToken);
}
