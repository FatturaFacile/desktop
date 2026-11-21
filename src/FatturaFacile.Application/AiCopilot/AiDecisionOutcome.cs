// <copyright file="AiDecisionOutcome.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AiCopilot;

/// <summary>
/// Result of sending one decision to the web-api.
/// </summary>
/// <param name="Status">The classified status.</param>
/// <param name="Detail">The problem detail reported by the server, or a diagnostic message.</param>
public sealed record AiDecisionOutcome(AiApiStatus Status, string? Detail = null);
