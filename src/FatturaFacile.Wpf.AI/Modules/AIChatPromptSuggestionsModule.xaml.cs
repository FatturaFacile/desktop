// <copyright file="AIChatPromptSuggestionsModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Windows;
using DevExpress.AIIntegration.Blazor.Chat;
using DevExpress.AIIntegration.Blazor.Chat.WebView;

namespace FatturaFacile;

/// <summary>
/// Interaction logic and view module demonstrating prompt recommendation chips and click-to-send behaviors in the AI Chat interface.
/// </summary>
public partial class AIChatPromptSuggestionsModule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AIChatPromptSuggestionsModule"/> class.
    /// </summary>
    public AIChatPromptSuggestionsModule()
    {
        InitializeComponent();

        WarningText = Properties.Resource.EndpointWarning;
        UpdatePromptSuggestions();
    }

    private void OnSendOnClickChanged(
        object sender,
        RoutedEventArgs e) => UpdatePromptSuggestions();

    private void UpdatePromptSuggestions() => aiChatControl.PromptSuggestions =
        CreatePromptSuggestions(sendOnClickCheckEdit.IsChecked == true);

    private static ObservableCollection<IPromptSuggestion> CreatePromptSuggestions(
        bool sendOnClick)
    {
        return new ObservableCollection<IPromptSuggestion>
        {
            new PromptSuggestion(
                "Tell me a joke", "Take a break and enjoy a quick laugh", "Tell me a joke", sendOnClick),
            new PromptSuggestion(
                "Summarize text", "Extract a quick summary(main ideas)", "Summarize text", sendOnClick),
            new PromptSuggestion(
                "Write an email", "Make your text look and sound professional", "Write an email", sendOnClick),
            new PromptSuggestion(
                "Brainstorm ideas", "Get creative input for your tasks", "Brainstorm ideas", sendOnClick),
            new PromptSuggestion(
                "Fix my writing", "Avoid spelling, grammar, and style errors", "Fix my writing", sendOnClick)
        };
    }
}
