using DevExpress.AIIntegration.Blazor.Chat.WebView;

namespace AIDemo {
    public partial class WorkflowAsAgentModule : AgentModuleBase {
        public const string ServiceKey = nameof(WorkflowAsAgentModule);

        public WorkflowAsAgentModule() {
            InitializeComponent();
            InitializeAgentModule(aiChatControl);
        }

        protected override string ChatClientServiceKey => ServiceKey;
        protected override PromptSuggestion[] PromptSuggestions => new[] {
             new PromptSuggestion("Moon, whisper, ocean", "Tranquil & Mystical", "moon, whisper, ocean", false),
            new PromptSuggestion("Fire, dream, shadow", "Heroic & Hopeful", "fire, dream, shadow", false),
            new PromptSuggestion("Rose, thunder, silence", "Romantic & Contemplative", "rose, thunder, silence", false)

        };
        protected override string Icon => "&#x1F3AD;";
        protected override string Title => "Shakespearean Poet";
        protected override string Description =>
            "The AI Assistant uses a few keywords to craft a poem\n" +
            "then rewrites it in the grand style of <em>William Shakespeare</em>.\n" +
            "Choose a keyword combination below or enter <strong>2 or 3 custom keywords</strong>.";
    }
}
