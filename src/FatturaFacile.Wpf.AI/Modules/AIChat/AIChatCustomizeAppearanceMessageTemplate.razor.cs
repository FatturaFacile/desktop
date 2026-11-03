// <copyright file="AIChatCustomizeAppearanceMessageTemplate.razor.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.AIIntegration.Blazor.Chat;

using Microsoft.AspNetCore.Components;

namespace FatturaFacile.Modules.AIChat;

/// <summary>
/// Custom Blazor message bubble template applying custom CSS styling based on chat participant roles.
/// </summary>
public partial class AIChatCustomizeAppearanceMessageTemplate : ComponentBase
{

    /// <summary>
    /// Gets or sets the chat message instance being formatted and styled.
    /// </summary>
    [Parameter]
    public BlazorChatMessage Context { get; set; } = null!;

    /// <summary>
    /// Computes CSS class names corresponding to the author role of the specified message.
    /// </summary>
    /// <param name="message">The chat message model.</param>
    /// <returns>A string of whitespace-separated CSS class names.</returns>
    private string GetMessageClasses(BlazorChatMessage message)
    {
        if (message.Role == ChatMessageRole.Assistant)
        {
            return "demo-chat-message demo-assistant-message";
        }

        if (message.Role == ChatMessageRole.User)
        {
            return "demo-chat-message demo-user-message";
        }

        return message.Role == ChatMessageRole.Error ? "demo-chat-message demo-error-message" : "demo-chat-message";
    }

}
