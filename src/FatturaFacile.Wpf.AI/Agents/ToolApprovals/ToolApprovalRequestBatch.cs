// <copyright file="ToolApprovalRequestBatch.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.AI;

namespace FatturaFacile.Agents.ToolApprovals;

enum ToolApprovalRequestState
{
    Pending,
    Approved,
    Rejected
}

/// <summary>
/// Manages a batch of AI tool approval requests, tracking per-request approval states and response generation.
/// </summary>
public class ToolApprovalRequestBatch
{
    private readonly List<ToolApprovalRequestContent> requests;

    private readonly Dictionary<string, ToolApprovalRequestState> states =
        new Dictionary<string, ToolApprovalRequestState>();

    private ToolApprovalRequestState GetState(
        string requestId)
    {
        if (states.TryGetValue(requestId, out var state))
        {
            return state;
        }

        throw new InvalidOperationException($"Request with id {requestId} not found.");
    }

    private void SetState(
        string requestId,
        ToolApprovalRequestState state)
    {
        if (!states.ContainsKey(requestId))
        {
            throw new InvalidOperationException($"Request with id {requestId} not found.");
        }

        states[requestId] = state;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolApprovalRequestBatch"/> class with the specified approval requests.
    /// </summary>
    /// <param name="requests">The list of tool approval requests.</param>
    public ToolApprovalRequestBatch(
        List<ToolApprovalRequestContent> requests)
    {
        this.requests = new List<ToolApprovalRequestContent>(requests);
        this.requests.ForEach(r => states[r.RequestId] = ToolApprovalRequestState.Pending);
    }

    /// <summary>
    /// Determines whether the specified request has been approved or rejected.
    /// </summary>
    /// <param name="requestId">The request identifier.</param>
    /// <returns><c>true</c> if resolved; otherwise <c>false</c>.</returns>
    public bool IsRequestResolved(
        string requestId) =>
        GetState(requestId) != ToolApprovalRequestState.Pending;

    /// <summary>
    /// Determines whether the specified request has been rejected.
    /// </summary>
    /// <param name="requestId">The request identifier.</param>
    /// <returns><c>true</c> if rejected; otherwise <c>false</c>.</returns>
    public bool IsRequestRejected(
        string requestId) =>
        GetState(requestId) == ToolApprovalRequestState.Rejected;

    /// <summary>
    /// Marks the specified request as approved.
    /// </summary>
    /// <param name="requestId">The request identifier.</param>
    public void ApproveRequest(
        string requestId) =>
        SetState(requestId, ToolApprovalRequestState.Approved);

    /// <summary>
    /// Marks the specified request as rejected.
    /// </summary>
    /// <param name="requestId">The request identifier.</param>
    public void RejectRequest(
        string requestId) =>
        SetState(requestId, ToolApprovalRequestState.Rejected);

    /// <summary>
    /// Generates AI content response messages for all resolved requests in the batch.
    /// </summary>
    /// <returns>A list of generated <see cref="AIContent"/> items.</returns>
    public List<AIContent> GenerateResponses()
    {
        return requests
            .FindAll(r => states[r.RequestId] != ToolApprovalRequestState.Pending)
            .Select(r => states[r.RequestId] == ToolApprovalRequestState.Approved
                ? r.CreateResponse(true)
                : r.CreateResponse(
                    false,
                    "The user rejected this operation. Confirm cancellation and reassure them no changes were made.")
            )
            .ToList<AIContent>();
    }

    /// <summary>
    /// Returns the number of requests currently pending user decision.
    /// </summary>
    /// <returns>The count of pending requests.</returns>
    public int CountPendingRequests() =>
        requests.Count(r => states[r.RequestId] == ToolApprovalRequestState.Pending);
}
