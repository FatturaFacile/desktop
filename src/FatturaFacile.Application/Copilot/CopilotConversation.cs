// <copyright file="CopilotConversation.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Copilot;

/// <summary>
/// A Copilot conversation.
/// </summary>
/// <param name="Id">The server identifier.</param>
/// <param name="Title">The title.</param>
/// <param name="Locale">The locale.</param>
/// <param name="Context">The context the conversation was started from.</param>
/// <param name="CreatedAt">The creation time.</param>
/// <param name="UpdatedAt">The last update time.</param>
/// <param name="Messages">The messages (empty when only the list was fetched).</param>
public sealed record CopilotConversation(
    string Id,
    string Title,
    string Locale,
    CopilotContext Context,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyList<CopilotMessage> Messages);
