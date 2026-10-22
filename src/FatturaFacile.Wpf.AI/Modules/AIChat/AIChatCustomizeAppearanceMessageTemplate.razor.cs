// <copyright file="AIChatCustomizeAppearanceMessageTemplate.razor.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.AIIntegration.Blazor.Chat;
using Microsoft.AspNetCore.Components;

namespace FatturaFacile.Modules.AIChat;

public partial class AIChatCustomizeAppearanceMessageTemplate : ComponentBase
{
    [Parameter] public BlazorChatMessage Context { get; set; }

    private string GetMessageClasses(
        BlazorChatMessage message)
    {
        if (message.Role == ChatMessageRole.Assistant)
        {
            return "demo-chat-message demo-assistant-message";
        }

        if (message.Role == ChatMessageRole.User)
        {
            return "demo-chat-message demo-user-message";
        }

        if (message.Role == ChatMessageRole.Error)
        {
            return "demo-chat-message demo-error-message";
        }

        return "demo-chat-message";
    }
}
