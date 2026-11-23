// <copyright file="IReceivablesService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Receivables;

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// Offline-first receivables (Mod 013): reads go to the server and fall back to the last cached answer; writes are saved in the outbox
/// first and replayed in order when the server is reachable.
/// </summary>
public interface IReceivablesService
{
    /// <summary>
    /// Loads the dashboard; offline, the last cached one.
    /// </summary>
    /// <param name="currency">The ISO currency, null for the company currency.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesLoad<ReceivablesDashboard>?> LoadDashboardAsync(string? currency, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the aging; offline, the last cached one.
    /// </summary>
    /// <param name="customerId">The customer, null for everybody.</param>
    /// <param name="currency">The ISO currency.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesLoad<AgingReport>?> LoadAgingAsync(Guid? customerId, string? currency, CancellationToken cancellationToken);

    /// <summary>
    /// Loads a page of receivable items; offline, the last cached page of the same filters.
    /// </summary>
    /// <param name="filter">The filters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesLoad<ReceivableItemPage>?> LoadItemsAsync(ReceivableItemFilter filter, CancellationToken cancellationToken);

    /// <summary>
    /// Loads an item with its allocations.
    /// </summary>
    /// <param name="id">The item.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesLoad<ReceivableItem>?> LoadItemAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the customer ledger with its running balance.
    /// </summary>
    /// <param name="customerId">The customer.</param>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <param name="currency">The ISO currency.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesLoad<CustomerLedger>?> LoadLedgerAsync(Guid customerId, DateOnly? from, DateOnly? to, string? currency, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the payments of a customer not yet fully allocated.
    /// </summary>
    /// <param name="customerId">The customer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesLoad<IReadOnlyList<UnreconciledPayment>>?> LoadUnreconciledPaymentsAsync(Guid customerId, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the credit position of a customer.
    /// </summary>
    /// <param name="customerId">The customer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesLoad<CustomerCreditStatus>?> LoadCreditStatusAsync(Guid customerId, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the composite credit risk of a customer.
    /// </summary>
    /// <param name="customerId">The customer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesLoad<CreditRiskScore>?> LoadRiskAsync(Guid customerId, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the credit limit history of a customer, newest first.
    /// </summary>
    /// <param name="customerId">The customer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesLoad<IReadOnlyList<CustomerCreditLimit>>?> LoadCreditLimitsAsync(Guid customerId, CancellationToken cancellationToken);

    /// <summary>
    /// Runs a credit check; offline, it is decided locally from the cached credit position with the same policy rules.
    /// </summary>
    /// <param name="customerId">The customer.</param>
    /// <param name="scope">The operation.</param>
    /// <param name="amount">What the operation adds to the exposure.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesLoad<CreditDecision>?> CheckCreditAsync(Guid customerId, CreditScope scope, decimal amount, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the days sales outstanding.
    /// </summary>
    /// <param name="customerId">The customer, null for the company.</param>
    /// <param name="periodDays">The days of the period.</param>
    /// <param name="currency">The ISO currency.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesLoad<DsoResult>?> LoadDsoAsync(Guid? customerId, int periodDays, string? currency, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the bad-debt provision per bucket.
    /// </summary>
    /// <param name="customerId">The customer, null for everybody.</param>
    /// <param name="currency">The ISO currency.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesLoad<ProvisionSummary>?> LoadProvisionAsync(Guid? customerId, string? currency, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the statement of a customer for a period.
    /// </summary>
    /// <param name="customerId">The customer.</param>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <param name="currency">The ISO currency.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesLoad<CustomerStatement>?> LoadStatementAsync(Guid customerId, DateOnly from, DateOnly to, string? currency, CancellationToken cancellationToken);

    /// <summary>
    /// Downloads the statement as a file; null when the server cannot be reached.
    /// </summary>
    /// <param name="customerId">The customer.</param>
    /// <param name="format">pdf or xlsx.</param>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <param name="currency">The ISO currency.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesFile?> ExportStatementAsync(Guid customerId, string format, DateOnly from, DateOnly to, string? currency, CancellationToken cancellationToken);

    /// <summary>
    /// Applies allocations; queued in the outbox when the server is not reachable.
    /// </summary>
    /// <param name="request">The allocations.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesActionResult> ApplyAllocationsAsync(ApplyAllocationsRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Reverses an allocation; queued when offline.
    /// </summary>
    /// <param name="request">The reversal.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesActionResult> ReverseAllocationAsync(ReverseAllocationRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Marks or clears the dispute of an item; queued when offline.
    /// </summary>
    /// <param name="request">The dispute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesActionResult> SetDisputeAsync(DisputeRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Creates or updates a credit limit; queued when offline.
    /// </summary>
    /// <param name="request">The limit.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesActionResult> SaveCreditLimitAsync(CreditLimitRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a credit limit; queued when offline.
    /// </summary>
    /// <param name="request">The deletion.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesActionResult> DeleteCreditLimitAsync(DeleteCreditLimitRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Grants a credit override; queued when offline.
    /// </summary>
    /// <param name="request">The override.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesActionResult> GrantOverrideAsync(GrantOverrideRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Revokes a credit override; queued when offline.
    /// </summary>
    /// <param name="request">The revocation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesActionResult> RevokeOverrideAsync(RevokeOverrideRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Changes the provision rates; queued when offline.
    /// </summary>
    /// <param name="request">The rates.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesActionResult> UpdateProvisionRatesAsync(ProvisionRatesRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Asks the server to email a statement; queued when offline.
    /// </summary>
    /// <param name="request">The statement email.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesActionResult> SendStatementAsync(SendStatementRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Sends the queued actions in order.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesFlushResult> FlushAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Lists the queued and the refused actions.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ReceivablesQueue> LoadQueueAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Acknowledges a refused action.
    /// </summary>
    /// <param name="entryId">The entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    public Task DismissConflictAsync(Guid entryId, CancellationToken cancellationToken);
}
