// <copyright file="AgentModuleBase.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;

using DevExpress.AIIntegration.Blazor.Chat;
using DevExpress.AIIntegration.Blazor.Chat.WebView;
using DevExpress.AIIntegration.Wpf.Chat;
using FatturaFacile.Modules.AgentsAPI;
using Markdig;
using Microsoft.AspNetCore.Components;
using DevExpress.Xpf.DemoBase;

namespace FatturaFacile;

/// <summary>
/// Abstract base class for demonstration modules hosting an interactive AI chat control.
/// </summary>
public abstract class AgentModuleBase : DemoModule
{
    private AIChatControl chatControl;

    protected AIChatControl ChatControl => chatControl;

    protected abstract string ChatClientServiceKey { get; }
    protected abstract PromptSuggestion[] PromptSuggestions { get; }
    protected abstract string Icon { get; }
    protected abstract string Title { get; }
    protected abstract string Description { get; }

    protected void InitializeAgentModule(
        AIChatControl moduleChatControl)
    {
        if (moduleChatControl == null)
        {
            throw new ArgumentNullException(nameof(moduleChatControl));
        }

        chatControl = moduleChatControl;

        WarningText = Properties.Resource.EndpointWarning;
        chatControl.ChatResponseProviderServiceKey = ChatClientServiceKey;
        chatControl.ContentFormat = ResponseContentFormat.Markdown;
        chatControl.MarkdownConvert += MarkdownConvert;
        chatControl.EmptyStateTemplate = builder =>
        {
            builder.OpenComponent<EmptyMessageAreaTemplate>(0);
            builder.AddAttribute(1, "Icon", Icon);
            builder.AddAttribute(2, "Title", Title);
            builder.AddAttribute(3, "Description", Description);
            builder.CloseComponent();
        };
        chatControl.PromptSuggestions = new ObservableCollection<IPromptSuggestion>(PromptSuggestions);
    }

    private void MarkdownConvert(
        object sender,
        AIChatControlMarkdownConvertEventArgs e)
    {
        var html = Markdown.ToHtml(e.MarkdownText);
        e.HtmlText = (MarkupString)html;
    }
}
