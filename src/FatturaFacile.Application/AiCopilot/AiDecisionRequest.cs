// <copyright file="AiDecisionRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AiCopilot;

/// <summary>
/// A human decision on a suggestion.
/// </summary>
/// <param name="Approve">True to approve, false to reject.</param>
/// <param name="Text">The approval feedback (optional) or the rejection reason (required, at least 3 characters).</param>
/// <param name="EditedPayloadJson">The edited payload of an approval as a JSON object, or null.</param>
public sealed record AiDecisionRequest(bool Approve, string? Text, string? EditedPayloadJson = null);
