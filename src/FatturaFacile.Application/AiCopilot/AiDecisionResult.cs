// <copyright file="AiDecisionResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AiCopilot;

/// <summary>
/// Result of a user decision.
/// </summary>
/// <param name="Status">The outcome.</param>
/// <param name="Detail">The problem detail of the server, when any.</param>
/// <param name="Snapshot">The cache after the decision.</param>
public sealed record AiDecisionResult(AiDecisionStatus Status, string? Detail, AiCopilotSnapshot Snapshot);
