// <copyright file="AIChatPromptSuggestionsModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Windows;

using DevExpress.AIIntegration.Blazor.Chat;
using DevExpress.AIIntegration.Blazor.Chat.WebView;
using FatturaFacile.Wpf.AI;

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

        WarningText = Resource.EndpointWarning;
        UpdatePromptSuggestions();
    }

    /// <summary>
    /// Handles changes to the send-on-click option and refreshes prompt suggestion objects.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The routed event data.</param>
    private void OnSendOnClickChanged(
        object          sender,
        RoutedEventArgs e) => UpdatePromptSuggestions();

    /// <summary>
    /// Refreshes the prompt suggestion collection on the chat control.
    /// </summary>
    private void UpdatePromptSuggestions() => aiChatControl.PromptSuggestions =
                                                  CreatePromptSuggestions(sendOnClick: sendOnClickCheckEdit.IsChecked == true);

    /// <summary>
    /// Creates a collection of predefined prompt suggestions configured with the specified send-on-click behavior.
    /// </summary>
    /// <param name="sendOnClick">True to submit immediately upon clicking; false to copy to input.</param>
    /// <returns>An observable collection of prompt suggestions.</returns>
    private static ObservableCollection<IPromptSuggestion> CreatePromptSuggestions(bool sendOnClick)
    {
        return new ObservableCollection<IPromptSuggestion>
               {
                   new PromptSuggestion(
                                        title: "Tell me a joke",
                                        text: "Take a break and enjoy a quick laugh",
                                        prompt: "Tell me a joke",
                                        sendOnClick: sendOnClick),
                   new PromptSuggestion(
                                        title: "Summarize text",
                                        text: "Extract a quick summary(main ideas)",
                                        prompt: "Summarize text",
                                        sendOnClick: sendOnClick),
                   new PromptSuggestion(
                                        title: "Write an email",
                                        text: "Make your text look and sound professional",
                                        prompt: "Write an email",
                                        sendOnClick: sendOnClick),
                   new PromptSuggestion(
                                        title: "Brainstorm ideas",
                                        text: "Get creative input for your tasks",
                                        prompt: "Brainstorm ideas",
                                        sendOnClick: sendOnClick),
                   new PromptSuggestion(
                                        title: "Fix my writing",
                                        text: "Avoid spelling, grammar, and style errors",
                                        prompt: "Fix my writing",
                                        sendOnClick: sendOnClick),
               };
    }

}
