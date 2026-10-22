using System;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.AIIntegration.Blazor.Chat;
using DevExpress.AIIntegration.Blazor.Chat.WebView;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using Microsoft.Extensions.AI;

namespace AIDemo {
    public partial class AIToolsModule {
        public const string ToolsChatResponseProviderServiceKey = "ToolsChatResponseProvider";
        const string SystemMessage =
@"You are an assistant integrated into an application. Your role is to perform all user-requested UI operations exclusively through the provided tools.

Rules:
1. Always use tools to interact with the UI. Do not simulate actions or produce imagined results.
2. If a tool requires a tool-target identifier, obtain it only by calling the GetToolTargets tool. Never invent, guess, or construct identifiers; always use one returned by GetToolTargets.
3. Provide valid parameter values for every tool call, using context or additional tool calls when needed.
4. If the user requests an action that cannot be performed through the available tools, clearly state that it is not supported.";

        ObservableCollection<IPromptSuggestion> layoutPromptSuggestions = new ObservableCollection<IPromptSuggestion> {
            new PromptSuggestion(
                title: "Smart Paste",
                text: "Auto-populate the DevExpress Layout Control form",
                prompt: "Populate this form with sample data: \r\nPayment: Amount - $123.00\r\nStatement Date: 10/15/2024\r\nAccount Number: 133244556\r\nName: John Smith\r\nContact: (123) 456-7890\r\nEmail: john@myemail.com\r\nAddress:\r\n- 123 Elm St Apt 4B\r\n- New York, NY 10001",
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

        ObservableCollection<IPromptSuggestion> gridPromptSuggestions = new ObservableCollection<IPromptSuggestion>  {
            new PromptSuggestion(
                title: "Sort Grid Column",
                text: "Sort the grid against a specific column in ascending order.",
                prompt: "Sort the grid by Country in ascending order.",
                sendOnClick: true),
            new PromptSuggestion(
                title: "Group Grid Data",
                text: "Group grid data against a specific column.",
                prompt: "Group grid data by Country.",
                sendOnClick: true),
            new PromptSuggestion(
                title: "Clear Grouping",
                text: "Remove all data groupings from the grid.",
                prompt: "Clear all data groupings in the grid.",
                sendOnClick: true),
            new PromptSuggestion(
                title: "Filter Grid Data",
                text: "Filter the grid by order total amount.",
                prompt: "Filter the grid and display orders where total amount is greater than 500.",
                sendOnClick: true),
            new PromptSuggestion(
                title: "Clear Filters",
                text: "Remove all data filters from the grid.",
                prompt: "Clear all data filters from the grid.",
                sendOnClick: true),
            new PromptSuggestion(
                title: "Application Theme",
                text: "Switch to dark mode",
                prompt: "Change the application theme to dark mode",
                sendOnClick: true),
            new PromptSuggestion(
                title: "Application Layout",
                text: "Switch to full-screen mode.",
                prompt: "Switch the application to full-window mode.",
                sendOnClick: true),
        };

        public AIToolsModule() {
            InitializeComponent();
        }

        void SetSuggestions(DXTabItem tabItem) {
            AiChatControl.PromptSuggestions = tabItem.Content is GridControl ? gridPromptSuggestions : layoutPromptSuggestions;
        }

        void TabControl_OnSelectionChanged(object sender, TabControlSelectionChangedEventArgs e) {
            if (e.NewSelectedItem == null || AiChatControl == null)
                return;

            var tabItem = e.NewSelectedItem as DXTabItem;

            if(tabItem.Content == null)
                return;

            SetSuggestions(tabItem);
        }

        
        
        void OnResponseReceived(object sender, AIChatControlResponseReceivedEventArgs e) {
            if(e.Message.FunctionCalls.Count > 0) {
                
                if(e.Message.FunctionCalls.RemoveAll(fcall => fcall.Request.Name == "GetToolTargets") > 0) {
                    
                    
                    e.Message.FunctionCalls.ForEach(fcall => {
                        var argumentsToRemove = fcall.Request.Arguments.Keys
                            .Where(k => k.EndsWith("_identifier")).ToList();
                        foreach(var argument in argumentsToRemove)
                            fcall.Request.Arguments.Remove(argument);
                    });
                }
            }
        }

        void AiChatControl_Initialized(object sender, AIChatControlInitializedEventArgs e) {
            SetSuggestions(tabControl.SelectedItem as DXTabItem);
            AiChatControl.LoadMessages(new[] { new BlazorChatMessage(ChatRole.System, SystemMessage) });
        }
    }
}
