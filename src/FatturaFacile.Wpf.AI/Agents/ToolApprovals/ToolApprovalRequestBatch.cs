// <copyright file="ToolApprovalRequestBatch.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using Microsoft.Extensions.AI;

namespace FatturaFacile.Wpf.AI.Agents.ToolApprovals;

/// <summary>
/// Defines the lifecycle approval states of an AI tool invocation request.
/// </summary>
internal enum ToolApprovalRequestState
{

    /// <summary>
    /// The request is awaiting user decision.
    /// </summary>
    Pending,

    /// <summary>
    /// The user approved the tool invocation.
    /// </summary>
    Approved,

    /// <summary>
    /// The user rejected the tool invocation.
    /// </summary>
    Rejected

}

/// <summary>
/// Manages a batch of AI tool approval requests, tracking per-request approval states and response generation.
/// </summary>
public class ToolApprovalRequestBatch
{

    /// <summary>
    /// The collection of pending tool approval requests.
    /// </summary>
    private readonly List<ToolApprovalRequestContent> requests;

    /// <summary>
    /// Maps each request identifier to its current approval state.
    /// </summary>
    private readonly Dictionary<string, ToolApprovalRequestState> states =
        new Dictionary<string, ToolApprovalRequestState>();

    /// <summary>
    /// Gets the current approval state for the specified request.
    /// </summary>
    /// <param name="requestId">The request identifier.</param>
    /// <returns>The current <see cref="ToolApprovalRequestState"/>.</returns>
    private ToolApprovalRequestState GetState(string requestId)
    {
        return states.TryGetValue(key: requestId, value: out var state)
            ? state
            : throw new InvalidOperationException(message: $"Request with id {requestId} not found.");
    }

    /// <summary>
    /// Updates the approval state for the specified request identifier.
    /// </summary>
    /// <param name="requestId">The request identifier.</param>
    /// <param name="state">The target state to set.</param>
    private void SetState(
        string                   requestId,
        ToolApprovalRequestState state)
    {
        if (!states.ContainsKey(key: requestId))
        {
            throw new InvalidOperationException(message: $"Request with id {requestId} not found.");
        }

        states[key: requestId] = state;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolApprovalRequestBatch"/> class with the specified approval requests.
    /// </summary>
    /// <param name="requests">The list of tool approval requests.</param>
    public ToolApprovalRequestBatch(List<ToolApprovalRequestContent> requests)
    {
        this.requests = new List<ToolApprovalRequestContent>(collection: requests);
        this.requests.ForEach(action: r => states[key: r.RequestId] = ToolApprovalRequestState.Pending);
    }

    /// <summary>
    /// Determines whether the specified request has been approved or rejected.
    /// </summary>
    /// <param name="requestId">The request identifier.</param>
    /// <returns><c>true</c> if resolved; otherwise <c>false</c>.</returns>
    public bool IsRequestResolved(string requestId) =>
        GetState(requestId: requestId) != ToolApprovalRequestState.Pending;

    /// <summary>
    /// Determines whether the specified request has been rejected.
    /// </summary>
    /// <param name="requestId">The request identifier.</param>
    /// <returns><c>true</c> if rejected; otherwise <c>false</c>.</returns>
    public bool IsRequestRejected(string requestId) =>
        GetState(requestId: requestId) == ToolApprovalRequestState.Rejected;

    /// <summary>
    /// Marks the specified request as approved.
    /// </summary>
    /// <param name="requestId">The request identifier.</param>
    public void ApproveRequest(string requestId) =>
        SetState(requestId: requestId, state: ToolApprovalRequestState.Approved);

    /// <summary>
    /// Marks the specified request as rejected.
    /// </summary>
    /// <param name="requestId">The request identifier.</param>
    public void RejectRequest(string requestId) =>
        SetState(requestId: requestId, state: ToolApprovalRequestState.Rejected);

    /// <summary>
    /// Generates AI content response messages for all resolved requests in the batch.
    /// </summary>
    /// <returns>A list of generated <see cref="AIContent"/> items.</returns>
    public List<AIContent> GenerateResponses()
    {
        return requests
              .FindAll(match: r => states[key: r.RequestId] != ToolApprovalRequestState.Pending)
              .Select(selector: r => states[key: r.RequestId] == ToolApprovalRequestState.Approved
                                         ? r.CreateResponse(approved: true)
                                         : r.CreateResponse(
                                                            approved: false,
                                                            reason: "The user rejected this operation. Confirm cancellation and reassure them no changes were made."))
              .ToList<AIContent>();
    }

    /// <summary>
    /// Returns the number of requests currently pending user decision.
    /// </summary>
    /// <returns>The count of pending requests.</returns>
    public int CountPendingRequests() =>
        requests.Count(predicate: r => states[key: r.RequestId] == ToolApprovalRequestState.Pending);

}
