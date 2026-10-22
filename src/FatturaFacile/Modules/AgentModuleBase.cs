using System;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.AIIntegration.Blazor.Chat;
using DevExpress.AIIntegration.Blazor.Chat.WebView;
using DevExpress.AIIntegration.Wpf.Chat;
using DevExpress.Xpf.DemoBase;
using Markdig;
using Microsoft.AspNetCore.Components;

namespace AIDemo {
    public abstract class AgentModuleBase : DemoModule {
        AIChatControl chatControl;

        protected AIChatControl ChatControl => chatControl;

        protected abstract string ChatClientServiceKey { get; }
        protected abstract PromptSuggestion[] PromptSuggestions { get; }
        protected abstract string Icon { get; }
        protected abstract string Title { get; }
        protected abstract string Description { get; }

        protected void InitializeAgentModule(AIChatControl moduleChatControl) {
            if(moduleChatControl == null)
                throw new ArgumentNullException(nameof(moduleChatControl));
            chatControl = moduleChatControl;

            WarningText = Properties.Resource.EndpointWarning;
            chatControl.ChatResponseProviderServiceKey = ChatClientServiceKey;
            chatControl.ContentFormat = ResponseContentFormat.Markdown;
            chatControl.MarkdownConvert += MarkdownConvert;
            chatControl.EmptyStateTemplate = builder => {
                builder.OpenComponent<EmptyMessageAreaTemplate>(0);
                builder.AddAttribute(1, "Icon", Icon);
                builder.AddAttribute(2, "Title", Title);
                builder.AddAttribute(3, "Description", Description);
                builder.CloseComponent();
            };
            chatControl.PromptSuggestions = new ObservableCollection<IPromptSuggestion>(PromptSuggestions);
        }

        void MarkdownConvert(object sender, AIChatControlMarkdownConvertEventArgs e) {
            string html = Markdown.ToHtml(e.MarkdownText);
            e.HtmlText = (MarkupString)html;
        }
    }
}
