// <copyright file="CustomerTimelineEvent.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Application.CustomerLifecycle;

/// <summary>
/// One touchpoint of the customer timeline.
/// </summary>
/// <param name="Type">activity, opportunity, invoice, ticket, work_order, contract or lead.</param>
/// <param name="Id">The identifier of the source record.</param>
/// <param name="OccurredAt">When it happened.</param>
/// <param name="Title">The title.</param>
/// <param name="Status">The status of the source record.</param>
/// <param name="Amount">The amount, when the record has one.</param>
/// <param name="Reference">The document number, when the record has one.</param>
public sealed record CustomerTimelineEvent(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("occurred_at")] DateTimeOffset OccurredAt,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("amount")] string? Amount,
    [property: JsonPropertyName("reference")] string? Reference);
