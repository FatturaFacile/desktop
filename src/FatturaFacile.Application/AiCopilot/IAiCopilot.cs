// <copyright file="IAiCopilot.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AiCopilot;

/// <summary>
/// Entry point of the AI copilot for the presentation layer (cache, decisions and offline queue).
/// </summary>
public interface IAiCopilot
{
    /// <summary>
    /// Occurs when the cache changed outside of a call of the presentation layer (background refresh).
    /// </summary>
    public event EventHandler? Changed;

    /// <summary>
    /// Gets a value indicating whether the web-api integration is configured.
    /// </summary>
    public bool IsConfigured { get; }

    /// <summary>
    /// Reads the local cache without touching the network.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The cached state.</returns>
    public Task<AiCopilotSnapshot> LoadAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Pushes queued decisions and refreshes the cache from the server.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<AiRefreshResult> RefreshAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Approves or rejects a suggestion online, or queues the decision when the server is unreachable.
    /// </summary>
    /// <param name="id">The suggestion identifier.</param>
    /// <param name="request">The decision.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<AiDecisionResult> DecideAsync(Guid id, AiDecisionRequest request, CancellationToken cancellationToken);
}
