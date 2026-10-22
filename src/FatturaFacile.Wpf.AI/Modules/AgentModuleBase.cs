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

namespace FatturaFacile.Wpf.AI.Modules;

/// <summary>
/// Abstract base class for demonstration modules hosting an interactive AI chat control.
/// </summary>
public abstract class AgentModuleBase : DemoModule
{

    /// <summary>
    /// The hosted AI chat control instance.
    /// </summary>
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
    protected void InitializeAgentModule(AIChatControl moduleChatControl)
    {
        if (moduleChatControl == null)
        {
            throw new ArgumentNullException(paramName: nameof(moduleChatControl));
        }

        chatControl = moduleChatControl;

        WarningText                                = Resource.EndpointWarning;
        chatControl.ChatResponseProviderServiceKey =  ChatClientServiceKey;
        chatControl.ContentFormat                  =  ResponseContentFormat.Markdown;
        chatControl.MarkdownConvert                += MarkdownConvert;

        chatControl.EmptyStateTemplate = builder =>
                                         {
                                             builder.OpenComponent<EmptyMessageAreaTemplate>(sequence: 0);
                                             builder.AddAttribute(sequence: 1, name: "Icon",  value: Icon);
                                             builder.AddAttribute(sequence: 2, name: "Title", value: Title);
                                             builder.AddAttribute(sequence: 3, name: "Description", value: Description);
                                             builder.CloseComponent();
                                         };

        chatControl.PromptSuggestions = new ObservableCollection<IPromptSuggestion>(collection: PromptSuggestions);
    }

    /// <summary>
    /// Converts markdown text to HTML markup for rich rendering in the chat control.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The markdown conversion event arguments.</param>
    private void MarkdownConvert(
        object                                sender,
        AIChatControlMarkdownConvertEventArgs e)
    {
        var html = Markdown.ToHtml(markdown: e.MarkdownText);
        e.HtmlText = (MarkupString)html;
    }

}
