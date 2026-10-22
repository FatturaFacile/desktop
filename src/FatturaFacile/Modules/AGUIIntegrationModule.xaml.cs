using DevExpress.AIIntegration.Blazor.Chat;
using DevExpress.AIIntegration.Blazor.Chat.WebView;
using Microsoft.AspNetCore.Components;
using System;

namespace AIDemo {
    public partial class AGUIIntegrationModule: AgentModuleBase {
        public const string ServiceKey = nameof(AGUIIntegrationModule);

        public AGUIIntegrationModule() {
            InitializeComponent();
            InitializeAgentModule(aiChatControl);
            ChatControl.ContentFormat = ResponseContentFormat.PlainText;
            ChatControl.UseStreaming = false;
            ChatControl.MessageContentTemplate = (message) => (builder) => {
                builder.OpenComponent<WeatherMessageContentTemplate>(0);
                builder.AddAttribute(1, "Message", message);
                builder.CloseComponent();
            };
        }

        protected override string ChatClientServiceKey => ServiceKey;
        protected override PromptSuggestion[] PromptSuggestions => new[] {
             new PromptSuggestion("Weather in London", "Check the weather in the UK capital", "What's the weather in London?", false),
             new PromptSuggestion("Weather in Tokyo", "Check the weather in Japan's capital", "How's the weather looking in Tokyo?", false),
             new PromptSuggestion("Weather in New York", "Check the weather in New York (the Big Apple)", "Tell me about the weather in New York.", false)
        };
        protected override string Icon => "&#x1F324;&#xFE0F;";

        protected override string Title => "AG-UI Weather Agent";
        protected override string Description =>
            "Ask about the weather in any city. Pick a suggestion below or enter your own prompt.\n" +
            "The chat control queries a self-hosted <strong>Weather Agent</strong>.\n" +
            "<strong>AG-UI protocol</strong> is used for communication.";
    }
}
