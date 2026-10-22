using System;
using System.Collections.Generic;
using DevExpress.AIIntegration.Blazor.Chat;
using Microsoft.AspNetCore.Components;

namespace AIDemo {
    public partial class AIChatFileAttachmentsMessageTemplate : ComponentBase {
        [Parameter] public BlazorChatMessage Context { get; set; }
        [Parameter] public List<string> ImageExtensions { get; set; }
        [Parameter] public string FileIconBase64 { get; set; }

        string GetImageSrc(AIChatUploadFileInfo file) {
            var base64 = Convert.ToBase64String(file.Data.ToArray());
#pragma warning disable DX0025 
            return string.Format("data:{0};base64,{1}", file.Type, base64);
#pragma warning restore DX0025
        }

        string GetMessageClasses(BlazorChatMessage message) {
            if(message.Role == ChatMessageRole.User)
                return "dxbl-chatui-message dxbl-chatui-message-user";
            if(message.Role == ChatMessageRole.System)
                return "dxbl-chatui-message dxbl-chatui-message-system";
            if(message.Role == ChatMessageRole.Assistant)
                return "dxbl-chatui-message dxbl-chatui-message-assistant";
            if(message.Role == ChatMessageRole.Tool)
                return "dxbl-chatui-message dxbl-chatui-message-assistant";
            if(message.Role == ChatMessageRole.Error)
                return "dxbl-chatui-message dxbl-chatui-message-error";

            return "dxbl-chatui-message";
        }
    }
}
