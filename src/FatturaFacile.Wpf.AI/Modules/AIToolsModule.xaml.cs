// <copyright file="AIToolsModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;

using DevExpress.AIIntegration.Blazor.Chat;
using DevExpress.AIIntegration.Blazor.Chat.WebView;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;

using Microsoft.Extensions.AI;

namespace FatturaFacile;

/// <summary>
/// Interaction logic and view module for interacting with WPF UI controls (Data Grid, Layout Control, Themes) using natural language AI tool calls.
/// </summary>
public partial class AIToolsModule
{

    /// <summary>
    /// Identifier key used to register and resolve the associated chat client service in DI containers.
    /// </summary>
    public const string ToolsChatResponseProviderServiceKey = "ToolsChatResponseProvider";

    /// <summary>
    /// System prompt directing the AI model to interact with the UI only through registered tools.
    /// </summary>
    private const string SystemMessage =
        @"You are an assistant integrated into an application. Your role is to perform all user-requested UI operations exclusively through the provided tools.

Rules:
1. Always use tools to interact with the UI. Do not simulate actions or produce imagined results.
2. If a tool requires a tool-target identifier, obtain it only by calling the GetToolTargets tool. Never invent, guess, or construct identifiers; always use one returned by GetToolTargets.
3. Provide valid parameter values for every tool call, using context or additional tool calls when needed.
4. If the user requests an action that cannot be performed through the available tools, clearly state that it is not supported.";

    /// <summary>
    /// Collection of prompt suggestions for the Layout Control tab.
    /// </summary>
    private ObservableCollection<IPromptSuggestion> layoutPromptSuggestions =
        new ObservableCollection<IPromptSuggestion>
        {
            new PromptSuggestion(
                                 title: "Smart Paste",
                                 text: "Auto-populate the DevExpress Layout Control form",
                                 prompt:
                                 "Populate this form with sample data: \r\n" +
                                 "Payment: Amount - $123.00\r\n" +
                                 "Statement Date: 10/15/2024\r\n" +
                                 "Account Number: 133244556\r\n" +
                                 "Name: John Smith\r\n" +
                                 "Contact: (123) 456-7890\r\n" +
                                 "Email: john@myemail.com\r\n" +
                                 "Address:\r\n" +
                                 "- 123 Elm St Apt 4B\r\n" +
                                 "- New York, NY 10001",
                                 sendOnClick: true),
            new PromptSuggestion(
                                 title: "Application Theme",
                                 text: "Switch to dark mode",
                                 prompt: "Change the application theme to dark mode",
                                 sendOnClick: true),
            new PromptSuggestion(
                                 title: "Application Layout",
                                 text: "Switch to full-window mode",
                                 prompt: "Switch the application to full-window mode",
                                 sendOnClick: true),
        };

    /// <summary>
    /// Collection of prompt suggestions for the Data Grid tab.
    /// </summary>
    private ObservableCollection<IPromptSuggestion> gridPromptSuggestions = new ObservableCollection<IPromptSuggestion>
                                                                            {
                                                                                new PromptSuggestion(
                                                                                                     title:
                                                                                                     "Sort Grid Column",
                                                                                                     text:
                                                                                                     "Sort the grid against a specific column in ascending order.",
                                                                                                     prompt:
                                                                                                     "Sort the grid by Country in ascending order.",
                                                                                                     sendOnClick: true),
                                                                                new PromptSuggestion(
                                                                                                     title:
                                                                                                     "Group Grid Data",
                                                                                                     text:
                                                                                                     "Group grid data against a specific column.",
                                                                                                     prompt:
                                                                                                     "Group grid data by Country.",
                                                                                                     sendOnClick: true),
                                                                                new PromptSuggestion(
                                                                                                     title:
                                                                                                     "Clear Grouping",
                                                                                                     text:
                                                                                                     "Remove all data groupings from the grid.",
                                                                                                     prompt:
                                                                                                     "Clear all data groupings in the grid.",
                                                                                                     sendOnClick: true),
                                                                                new PromptSuggestion(
                                                                                                     title:
                                                                                                     "Filter Grid Data",
                                                                                                     text:
                                                                                                     "Filter the grid by order total amount.",
                                                                                                     prompt:
                                                                                                     "Filter the grid and display orders where total amount is greater than 500.",
                                                                                                     sendOnClick: true),
                                                                                new PromptSuggestion(
                                                                                                     title:
                                                                                                     "Clear Filters",
                                                                                                     text:
                                                                                                     "Remove all data filters from the grid.",
                                                                                                     prompt:
                                                                                                     "Clear all data filters from the grid.",
                                                                                                     sendOnClick: true),
                                                                                new PromptSuggestion(
                                                                                                     title:
                                                                                                     "Application Theme",
                                                                                                     text:
                                                                                                     "Switch to dark mode",
                                                                                                     prompt:
                                                                                                     "Change the application theme to dark mode",
                                                                                                     sendOnClick: true),
                                                                                new PromptSuggestion(
                                                                                                     title:
                                                                                                     "Application Layout",
                                                                                                     text:
                                                                                                     "Switch to full-screen mode.",
                                                                                                     prompt:
                                                                                                     "Switch the application to full-window mode.",
                                                                                                     sendOnClick: true),
                                                                            };

    /// <summary>
    /// Initializes a new instance of the <see cref="AIToolsModule"/> class.
    /// </summary>
    public AIToolsModule()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Sets prompt suggestions corresponding to the active tab control content type.
    /// </summary>
    /// <param name="tabItem">The active tab item.</param>
    private void SetSuggestions(DXTabItem tabItem) =>
        AiChatControl.PromptSuggestions = tabItem.Content is GridControl
                                              ? gridPromptSuggestions
                                              : layoutPromptSuggestions;

    /// <summary>
    /// Handles tab selection changes, updating available prompt suggestions for the active view.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">Selection change event arguments.</param>
    private void TabControl_OnSelectionChanged(
        object                              sender,
        TabControlSelectionChangedEventArgs e)
    {
        if (e.NewSelectedItem == null || AiChatControl == null)
        {
            return;
        }

        var tabItem = e.NewSelectedItem as DXTabItem;

        if (tabItem?.Content == null)
        {
            return;
        }

        SetSuggestions(tabItem: tabItem);
    }

    /// <summary>
    /// Filters internal tool routing parameters from response messages before presentation.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">Response received event arguments.</param>
    private void OnResponseReceived(
        object                                 sender,
        AIChatControlResponseReceivedEventArgs e)
    {
        if (e.Message.FunctionCalls.Count > 0)
        {
            if (e.Message.FunctionCalls.RemoveAll(match: fcall => fcall.Request.Name == "GetToolTargets") > 0)
            {
                e.Message.FunctionCalls.ForEach(action: fcall =>
                                                        {
                                                            var argumentsToRemove = fcall.Request.Arguments.Keys
                                                                                         .Where(predicate: k => k.EndsWith(value: "_identifier"))
                                                                                         .ToList();

                                                            foreach (var argument in argumentsToRemove)
                                                            {
                                                                _ = fcall.Request.Arguments.Remove(key: argument);
                                                            }
                                                        });
            }
        }
    }

    /// <summary>
    /// Initializes AI chat messages and suggestions upon chat control ready state.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">Initialization event arguments.</param>
    private void AiChatControl_Initialized(
        object                            sender,
        AIChatControlInitializedEventArgs e)
    {
        SetSuggestions(tabItem: tabControl.SelectedItem as DXTabItem);

        AiChatControl.LoadMessages(
                                   messages: new[]
                                             {
                                                 new BlazorChatMessage(role: ChatRole.System, text: SystemMessage)
                                             });
    }

}
