// <copyright file="CopilotStreamEvent.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Copilot;

/// <summary>
/// One server-sent event of an assistant turn.
/// </summary>
/// <param name="Kind">The event kind.</param>
/// <param name="Text">The token text, tool name or error message.</param>
/// <param name="Visualization">The visualization of a visualization event.</param>
/// <param name="ProposedAction">The action of an action.proposed event.</param>
/// <param name="MessageId">The message identifier of a completion event.</param>
/// <param name="ConversationId">The conversation identifier of a completion event.</param>
public sealed record CopilotStreamEvent(
    CopilotStreamEventKind Kind,
    string? Text = null,
    CopilotVisualization? Visualization = null,
    CopilotProposedAction? ProposedAction = null,
    string? MessageId = null,
    string? ConversationId = null);
