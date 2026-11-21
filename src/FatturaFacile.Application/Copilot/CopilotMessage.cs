// <copyright file="CopilotMessage.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Copilot;

/// <summary>
/// A message of a Copilot conversation.
/// </summary>
/// <param name="Id">The server identifier.</param>
/// <param name="ConversationId">The server identifier of the conversation.</param>
/// <param name="Role">"user", "assistant", "tool_call" or "tool_result".</param>
/// <param name="ContentMarkdown">The Markdown content.</param>
/// <param name="Citations">The citations.</param>
/// <param name="Visualization">The structured visualization, if any.</param>
/// <param name="ProposedAction">The proposed action, if any.</param>
/// <param name="ActionStatus">"pending", "confirmed", "rejected", "executed" or "failed", if an action was proposed.</param>
/// <param name="CreatedAt">The creation time.</param>
public sealed record CopilotMessage(
    string Id,
    string ConversationId,
    string Role,
    string ContentMarkdown,
    IReadOnlyList<CopilotCitation> Citations,
    CopilotVisualization? Visualization,
    CopilotProposedAction? ProposedAction,
    string? ActionStatus,
    DateTimeOffset? CreatedAt);
