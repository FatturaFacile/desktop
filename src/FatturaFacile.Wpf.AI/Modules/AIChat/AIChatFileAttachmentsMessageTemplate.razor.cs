// <copyright file="AIChatFileAttachmentsMessageTemplate.razor.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.AIIntegration.Blazor.Chat;

using Microsoft.AspNetCore.Components;

namespace FatturaFacile.Modules.AIChat;

/// <summary>
/// Blazor chat message template rendering file attachment previews (such as inline images and document download tiles).
/// </summary>
public partial class AIChatFileAttachmentsMessageTemplate : ComponentBase
{

    /// <summary>
    /// Gets or sets the chat message containing text and attached binary artifacts.
    /// </summary>
    [Parameter]
    public BlazorChatMessage Context { get; set; } = null!;

    /// <summary>
    /// Gets or sets the list of file extensions recognized as renderable image media.
    /// </summary>
    [Parameter]
    public List<string> ImageExtensions { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the Base64-encoded SVG or PNG icon graphic representing generic non-image documents.
    /// </summary>
    [Parameter]
    public string FileIconBase64 { get; set; } = string.Empty;

    /// <summary>
    /// Generates a data URI scheme string embedding Base64 image payload for HTML img tags.
    /// </summary>
    /// <param name="file">The uploaded file info containing data bytes and MIME type.</param>
    /// <returns>A data URI string.</returns>
    private string GetImageSrc(AIChatUploadFileInfo file)
    {
        var base64 = Convert.ToBase64String(inArray: file.Data.ToArray());
        #pragma warning disable DX0025
        return string.Format(format: "data:{0};base64,{1}", arg0: file.Type, arg1: base64);
        #pragma warning restore DX0025
    }

    /// <summary>
    /// Computes CSS class names corresponding to the author role of the specified message.
    /// </summary>
    /// <param name="message">The chat message model.</param>
    /// <returns>A string of whitespace-separated CSS class names.</returns>
    private string GetMessageClasses(BlazorChatMessage message)
    {
        if (message.Role == ChatMessageRole.User)
        {
            return "dxbl-chatui-message dxbl-chatui-message-user";
        }

        if (message.Role == ChatMessageRole.System)
        {
            return "dxbl-chatui-message dxbl-chatui-message-system";
        }

        if (message.Role == ChatMessageRole.Assistant)
        {
            return "dxbl-chatui-message dxbl-chatui-message-assistant";
        }

        if (message.Role == ChatMessageRole.Tool)
        {
            return "dxbl-chatui-message dxbl-chatui-message-assistant";
        }

        return message.Role == ChatMessageRole.Error ? "dxbl-chatui-message dxbl-chatui-message-error" : "dxbl-chatui-message";
    }

}
