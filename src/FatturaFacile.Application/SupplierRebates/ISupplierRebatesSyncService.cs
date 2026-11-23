// <copyright file="ISupplierRebatesSyncService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// Pulls the supplier rebate and blanket agreement payloads from the web-api into the local cache.
/// </summary>
public interface ISupplierRebatesSyncService
{
    /// <summary>
    /// Pulls the list of the rebate agreements; a failure leaves the cache untouched.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>How the attempt ended.</returns>
    public Task<SupplierRebatesResult> SyncAgreementsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Pulls an agreement with its progress, accrual runs and settlement proposals; they are saved together.
    /// </summary>
    /// <param name="agreementId">The agreement identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>How the attempt ended.</returns>
    public Task<SupplierRebatesResult> SyncAgreementAsync(string agreementId, CancellationToken cancellationToken);

    /// <summary>
    /// Pulls the list of the blanket agreements.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>How the attempt ended.</returns>
    public Task<SupplierRebatesResult> SyncBlanketContractsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Pulls the balance of a blanket agreement.
    /// </summary>
    /// <param name="contractId">The blanket agreement identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>How the attempt ended.</returns>
    public Task<SupplierRebatesResult> SyncBlanketBalanceAsync(string contractId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the open supplier invoices of a supplier for the set-off, live from the server.
    /// </summary>
    /// <param name="supplierId">The supplier identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The status and the open payables.</returns>
    public Task<RebatePayablesResult> LoadPayablesAsync(string supplierId, CancellationToken cancellationToken);
}
