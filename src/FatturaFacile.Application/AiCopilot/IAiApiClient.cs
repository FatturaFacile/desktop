// <copyright file="IAiApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AiCopilot;

/// <summary>
/// Typed client of the web-api AI surface (/api/v1/ai).
/// </summary>
public interface IAiApiClient
{
    /// <summary>
    /// Gets a value indicating whether a web-api session is available.
    /// </summary>
    public bool IsAuthenticated { get; }

    /// <summary>
    /// Fetches the open suggestion queue. Failures surface as <see cref="System.Net.Http.HttpRequestException"/>
    /// whose status code is set for HTTP errors (403 when the user lacks the ai_review module).
    /// </summary>
    /// <param name="perPage">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The page.</returns>
    public Task<AiSuggestionPage> GetSuggestionsAsync(int perPage, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches the predictive insights; failures surface as for <see cref="GetSuggestionsAsync"/>.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The insights.</returns>
    public Task<AiInsightsDto> GetInsightsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Approves a suggestion, optionally with an edited payload.
    /// </summary>
    /// <param name="id">The suggestion identifier.</param>
    /// <param name="editedPayloadJson">The edited payload as a JSON object, or null.</param>
    /// <param name="feedback">The optional feedback.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The classified outcome; never throws for HTTP or network failures.</returns>
    public Task<AiDecisionOutcome> ApproveAsync(Guid id, string? editedPayloadJson, string? feedback, CancellationToken cancellationToken);

    /// <summary>
    /// Rejects a suggestion.
    /// </summary>
    /// <param name="id">The suggestion identifier.</param>
    /// <param name="reason">The rejection reason (at least 3 characters).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The classified outcome; never throws for HTTP or network failures.</returns>
    public Task<AiDecisionOutcome> RejectAsync(Guid id, string reason, CancellationToken cancellationToken);
}
