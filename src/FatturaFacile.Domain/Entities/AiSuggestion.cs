// <copyright file="AiSuggestion.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Local cache row of a web-api AI suggestion (Human-in-the-Loop queue) with an optional decision taken offline.
/// </summary>
public class AiSuggestion : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the suggestion identifier assigned by the web-api.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the owning local tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the suggestion kind (for example "sales_quote" or "anomaly").
    /// </summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the workflow that produced the suggestion.
    /// </summary>
    public string Workflow { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the source record.
    /// </summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reference of the source record.
    /// </summary>
    public string SourceReference { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source text the proposal was derived from.
    /// </summary>
    public string? SourceText { get; set; }

    /// <summary>
    /// Gets or sets the proposed payload as a JSON object.
    /// </summary>
    public string ProposedPayloadJson { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the per-field confidences as a JSON object.
    /// </summary>
    public string FieldConfidencesJson { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the overall confidence between 0 and 1.
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Gets or sets the monetary amount involved, when any.
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// Gets or sets the reasoning of the model.
    /// </summary>
    public string? Reasoning { get; set; }

    /// <summary>
    /// Gets or sets the server status (see <see cref="AiSuggestionStatus"/>).
    /// </summary>
    public string Status { get; set; } = AiSuggestionStatus.Pending;

    /// <summary>
    /// Gets or sets the priority (low, normal, high).
    /// </summary>
    public string Priority { get; set; } = "normal";

    /// <summary>
    /// Gets or sets the AI provider that produced the suggestion.
    /// </summary>
    public string? ProviderUsed { get; set; }

    /// <summary>
    /// Gets or sets the model that produced the suggestion.
    /// </summary>
    public string? ModelUsed { get; set; }

    /// <summary>
    /// Gets or sets the instant after which the suggestion expires (UTC).
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the creation instant reported by the server (UTC).
    /// </summary>
    public DateTime? ServerCreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the decision queued while offline.
    /// </summary>
    public AiQueuedDecision QueuedDecision { get; set; }

    /// <summary>
    /// Gets or sets the approval feedback or the rejection reason of the queued decision.
    /// </summary>
    public string? QueuedFeedback { get; set; }

    /// <summary>
    /// Gets or sets the edited payload (JSON object) sent with a queued approval.
    /// </summary>
    public string? QueuedEditedPayloadJson { get; set; }

    /// <summary>
    /// Gets or sets the instant (UTC) the decision was queued.
    /// </summary>
    public DateTime? QueuedAt { get; set; }

    /// <summary>
    /// Gets or sets the number of failed delivery attempts of the queued decision.
    /// </summary>
    public int QueuedAttempts { get; set; }

    /// <summary>
    /// Gets or sets the last delivery error or server explanation.
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Gets or sets the instant (UTC) the row was last refreshed from the server.
    /// </summary>
    public DateTime? CachedAt { get; set; }

    /// <summary>
    /// Gets or sets the creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
