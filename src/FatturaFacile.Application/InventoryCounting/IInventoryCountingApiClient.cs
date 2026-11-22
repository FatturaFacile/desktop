// <copyright file="IInventoryCountingApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.InventoryCounting;

/// <summary>
/// Client of the web-api physical inventory surface (/api/v1/inventory): ABC analysis, cycle count plans,
/// count sessions and the variance reconciliation with its approval levels. Failures are returned, never thrown.
/// </summary>
public interface IInventoryCountingApiClient
{
    /// <summary>
    /// Gets a value indicating whether a session token is available.
    /// </summary>
    public bool IsAuthenticated { get; }

    /// <summary>
    /// Lists the ABC analysis runs, newest first.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The runs.</returns>
    public Task<InventoryCountingResult<IReadOnlyList<AbcRunDto>>> GetAbcRunsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Fetches one ABC analysis run with its ranked items.
    /// </summary>
    /// <param name="runId">The run id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The run.</returns>
    public Task<InventoryCountingResult<AbcRunDto>> GetAbcRunAsync(string runId, CancellationToken cancellationToken);

    /// <summary>
    /// Runs an ABC analysis.
    /// </summary>
    /// <param name="request">The parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The new run.</returns>
    public Task<InventoryCountingResult<AbcRunDto>> RunAbcAsync(AbcRunRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the manual ABC class overrides.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The overrides.</returns>
    public Task<InventoryCountingResult<IReadOnlyList<AbcOverrideDto>>> GetAbcOverridesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Forces the ABC class of an item, or clears the override when the class is null.
    /// </summary>
    /// <param name="stockItemId">The stock item id.</param>
    /// <param name="abcClass">The class (A, B or C), or null to clear.</param>
    /// <param name="reason">The mandatory justification.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<InventoryCountingResult<bool>> OverrideAbcClassAsync(string stockItemId, string? abcClass, string reason, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the cycle count plans.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The plans.</returns>
    public Task<InventoryCountingResult<IReadOnlyList<CycleCountPlanDto>>> GetPlansAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Generates the ABC based plans of a warehouse.
    /// </summary>
    /// <param name="warehouseId">The warehouse id.</param>
    /// <param name="warehouseZoneId">The zone id, or null.</param>
    /// <param name="freezesStock">Whether the counts freeze the stock, or null for the policy default.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated plans.</returns>
    public Task<InventoryCountingResult<IReadOnlyList<CycleCountPlanDto>>> GeneratePlansAsync(string warehouseId, string? warehouseZoneId, bool? freezesStock, CancellationToken cancellationToken);

    /// <summary>
    /// Opens a count session for a plan now.
    /// </summary>
    /// <param name="planId">The plan id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<InventoryCountingResult<bool>> RunPlanAsync(string planId, CancellationToken cancellationToken);

    /// <summary>
    /// Opens the count sessions of every plan that is due.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of opened sessions.</returns>
    public Task<InventoryCountingResult<int>> RunDuePlansAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a plan.
    /// </summary>
    /// <param name="planId">The plan id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<InventoryCountingResult<bool>> DeletePlanAsync(string planId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the count sessions.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The sessions.</returns>
    public Task<InventoryCountingResult<IReadOnlyList<CountSessionDto>>> GetSessionsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates a count session.
    /// </summary>
    /// <param name="request">The session parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The new session.</returns>
    public Task<InventoryCountingResult<CountSessionDto>> CreateSessionAsync(CreateCountSessionRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Freezes the stock of a session.
    /// </summary>
    /// <param name="sessionId">The session id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<InventoryCountingResult<bool>> FreezeAsync(string sessionId, CancellationToken cancellationToken);

    /// <summary>
    /// Lifts the stock freeze of a session.
    /// </summary>
    /// <param name="sessionId">The session id.</param>
    /// <param name="reason">The mandatory reason.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<InventoryCountingResult<bool>> UnfreezeAsync(string sessionId, string reason, CancellationToken cancellationToken);

    /// <summary>
    /// Assigns the session to a counter, or clears the assignment when null.
    /// </summary>
    /// <param name="sessionId">The session id.</param>
    /// <param name="assignedTo">The counter, or null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<InventoryCountingResult<bool>> AssignAsync(string sessionId, string? assignedTo, CancellationToken cancellationToken);

    /// <summary>
    /// Cancels a session.
    /// </summary>
    /// <param name="sessionId">The session id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<InventoryCountingResult<bool>> CancelAsync(string sessionId, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches the variance report (system, counted and final quantities, summary, approvals and adjustment).
    /// </summary>
    /// <param name="sessionId">The session id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The report.</returns>
    public Task<InventoryCountingResult<CountVarianceReportDto>> GetVarianceAsync(string sessionId, CancellationToken cancellationToken);

    /// <summary>
    /// Requests a recount of the given lines.
    /// </summary>
    /// <param name="sessionId">The session id.</param>
    /// <param name="lineIds">The line ids.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<InventoryCountingResult<bool>> RecountAsync(string sessionId, IReadOnlyList<string> lineIds, CancellationToken cancellationToken);

    /// <summary>
    /// Submits the differences for approval (reconcile).
    /// </summary>
    /// <param name="sessionId">The session id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<InventoryCountingResult<bool>> SubmitDifferencesAsync(string sessionId, CancellationToken cancellationToken);

    /// <summary>
    /// Approves one approval level.
    /// </summary>
    /// <param name="sessionId">The session id.</param>
    /// <param name="level">The level (1 or 2).</param>
    /// <param name="notes">Optional notes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<InventoryCountingResult<bool>> ApproveAsync(string sessionId, int level, string? notes, CancellationToken cancellationToken);

    /// <summary>
    /// Rejects one approval level.
    /// </summary>
    /// <param name="sessionId">The session id.</param>
    /// <param name="level">The level (1 or 2).</param>
    /// <param name="reason">The mandatory reason.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<InventoryCountingResult<bool>> RejectAsync(string sessionId, int level, string reason, CancellationToken cancellationToken);
}
