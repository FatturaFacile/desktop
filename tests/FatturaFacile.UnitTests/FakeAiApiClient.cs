// <copyright file="FakeAiApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net;
using System.Net.Http;
using FatturaFacile.Application.AiCopilot;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Scripted web-api AI client for the offline queue and cache tests.
/// </summary>
internal sealed class FakeAiApiClient : IAiApiClient
{
    /// <summary>
    /// Gets or sets a value indicating whether a session is available.
    /// </summary>
    public bool IsAuthenticated { get; set; } = true;

    /// <summary>
    /// Gets or sets the suggestion queue returned by the server.
    /// </summary>
    public List<AiSuggestionDto> Queue { get; set; } = [];

    /// <summary>
    /// Gets or sets the insights returned by the server.
    /// </summary>
    public AiInsightsDto Insights { get; set; } = new(CashFlow: null, PaymentRiskCount: 0, Recommendations: [], Anomalies: []);

    /// <summary>
    /// Gets or sets the HTTP status that makes the read calls fail, or null for success; zero simulates a network failure.
    /// </summary>
    public int? ReadFailure { get; set; }

    /// <summary>
    /// Gets the outcome of the decision calls per suggestion; success when missing.
    /// </summary>
    public Dictionary<Guid, AiDecisionOutcome> Outcomes { get; } = [];

    /// <summary>
    /// Gets the decisions sent to the server, in order (action, id, feedback or reason, edited payload).
    /// </summary>
    public List<(string Action, Guid Id, string? Text, string? EditedPayload)> Sent { get; } = [];

    /// <inheritdoc/>
    public Task<AiSuggestionPage> GetSuggestionsAsync(int perPage, CancellationToken cancellationToken)
    {
        ThrowIfReadFails();
        return Task.FromResult(new AiSuggestionPage([.. Queue], Queue.Count));
    }

    /// <inheritdoc/>
    public Task<AiInsightsDto> GetInsightsAsync(CancellationToken cancellationToken)
    {
        ThrowIfReadFails();
        return Task.FromResult(Insights);
    }

    /// <inheritdoc/>
    public Task<AiDecisionOutcome> ApproveAsync(Guid id, string? editedPayloadJson, string? feedback, CancellationToken cancellationToken)
    {
        Sent.Add(("approve", id, feedback, editedPayloadJson));
        return Task.FromResult(OutcomeOf(id));
    }

    /// <inheritdoc/>
    public Task<AiDecisionOutcome> RejectAsync(Guid id, string reason, CancellationToken cancellationToken)
    {
        Sent.Add(("reject", id, reason, null));
        return Task.FromResult(OutcomeOf(id));
    }

    private AiDecisionOutcome OutcomeOf(Guid id) => Outcomes.TryGetValue(id, out var outcome) ? outcome : new AiDecisionOutcome(AiApiStatus.Success);

    private void ThrowIfReadFails()
    {
        if (ReadFailure is not { } status)
        {
            return;
        }

        throw status == 0
            ? new HttpRequestException("no route")
            : new HttpRequestException("HTTP " + status.ToString(CultureInfo.InvariantCulture), inner: null, (HttpStatusCode)status);
    }
}
