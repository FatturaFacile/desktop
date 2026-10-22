using DevExpress.AIIntegration;
using DevExpress.AIIntegration.Blazor.Chat;
using DevExpress.AIIntegration.Blazor.Chat.WebView;
using Markdig;
using Microsoft.AspNetCore.Components;

namespace AIDemo {
    public partial class AIChatCustomizeAppearanceModule {
        readonly RenderFragment<BlazorChatMessage> messageTemplate = message => builder => {
            builder.OpenComponent<AIChatCustomizeAppearanceMessageTemplate>(0);
            builder.AddAttribute(1, nameof(AIChatCustomizeAppearanceMessageTemplate.Context), message);
            builder.CloseComponent();
        };

        public AIChatCustomizeAppearanceModule() {
            InitializeComponent();

            WarningText = Properties.Resource.EndpointWarning;
            UpdateContentFormat();
            UpdateMessageTemplate();
            aiChatControl.MarkdownConvert += MarkdownConvert;
        }

        void OnCustomMessageAppearanceChanged(object sender, System.Windows.RoutedEventArgs e) {
            UpdateMessageTemplate();
        }

        void OnRichFormattedResponsesChanged(object sender, System.Windows.RoutedEventArgs e) {
            UpdateContentFormat();
        }

        void UpdateContentFormat() {
            aiChatControl.ContentFormat = richFormattedResponsesCheckEdit.IsChecked == true ? ResponseContentFormat.Markdown : ResponseContentFormat.PlainText;
        }

        void UpdateMessageTemplate() {
            aiChatControl.MessageTemplate = customMessageAppearanceCheckEdit.IsChecked == true ? messageTemplate : null;
        }

        void MarkdownConvert(object sender, AIChatControlMarkdownConvertEventArgs e) {
            string html = Markdown.ToHtml(e.MarkdownText);
            e.HtmlText = (MarkupString)html;
        }
    }
}
