using DevExpress.AIIntegration.Blazor.Chat;
using Microsoft.AspNetCore.Components;

namespace AIDemo {
    public partial class AIChatCustomizeAppearanceMessageTemplate : ComponentBase {
        [Parameter] public BlazorChatMessage Context { get; set; }

        string GetMessageClasses(BlazorChatMessage message) {
            if(message.Role == ChatMessageRole.Assistant)
                return "demo-chat-message demo-assistant-message";
            if(message.Role == ChatMessageRole.User)
                return "demo-chat-message demo-user-message";
            if(message.Role == ChatMessageRole.Error)
                return "demo-chat-message demo-error-message";

            return "demo-chat-message";
        }
    }
}
