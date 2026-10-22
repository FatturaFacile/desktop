using System.Collections.ObjectModel;
using System.Windows;
using DevExpress.AIIntegration.Blazor.Chat;
using DevExpress.AIIntegration.Blazor.Chat.WebView;

namespace AIDemo {
    public partial class AIChatPromptSuggestionsModule {
        public AIChatPromptSuggestionsModule() {
            InitializeComponent();

            WarningText = Properties.Resource.EndpointWarning;
            UpdatePromptSuggestions();
        }

        void OnSendOnClickChanged(object sender, RoutedEventArgs e) {
            UpdatePromptSuggestions();
        }

        void UpdatePromptSuggestions() {
            aiChatControl.PromptSuggestions = CreatePromptSuggestions(sendOnClickCheckEdit.IsChecked == true);
        }

        static ObservableCollection<IPromptSuggestion> CreatePromptSuggestions(bool sendOnClick) {
            return new ObservableCollection<IPromptSuggestion> {
                new PromptSuggestion("Tell me a joke", "Take a break and enjoy a quick laugh", "Tell me a joke", sendOnClick),
                new PromptSuggestion("Summarize text", "Extract a quick summary(main ideas)", "Summarize text", sendOnClick),
                new PromptSuggestion("Write an email", "Make your text look and sound professional", "Write an email", sendOnClick),
                new PromptSuggestion("Brainstorm ideas", "Get creative input for your tasks", "Brainstorm ideas", sendOnClick),
                new PromptSuggestion("Fix my writing", "Avoid spelling, grammar, and style errors", "Fix my writing", sendOnClick)
            };
        }
    }
}
