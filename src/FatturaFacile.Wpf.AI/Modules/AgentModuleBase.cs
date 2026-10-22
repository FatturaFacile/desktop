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

    /// <summary>
    /// Gets the underlying <see cref="AIChatControl"/> instance associated with this demo module.
    /// </summary>
    protected AIChatControl ChatControl => chatControl;

    /// <summary>
    /// Gets the service key used to resolve the chat response provider or agent backend from the DI container.
    /// </summary>
    protected abstract string ChatClientServiceKey { get; }

    /// <summary>
    /// Gets the predefined prompt suggestion chips displayed in the chat interface.
    /// </summary>
    protected abstract PromptSuggestion[] PromptSuggestions { get; }

    /// <summary>
    /// Gets the glyph or icon identifier displayed in the empty-state conversation greeting.
    /// </summary>
    protected abstract string Icon { get; }

    /// <summary>
    /// Gets the primary title displayed in the empty-state greeting area.
    /// </summary>
    protected abstract string Title { get; }

    /// <summary>
    /// Gets the descriptive instructions displayed in the empty-state greeting area.
    /// </summary>
    protected abstract string Description { get; }

    /// <summary>
    /// Initializes common chat control behavior, markdown converters, empty state templates, and prompt suggestions.
    /// </summary>
    /// <param name="moduleChatControl">The chat control instance declared in the module view.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="moduleChatControl"/> is <see langword="null"/>.</exception>
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
