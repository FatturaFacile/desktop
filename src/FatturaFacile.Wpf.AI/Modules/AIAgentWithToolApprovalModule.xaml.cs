// <copyright file="AIAgentWithToolApprovalModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.AIIntegration.Blazor.Chat;
using DevExpress.AIIntegration.Blazor.Chat.WebView;

using Microsoft.Extensions.AI;
using FatturaFacile.Wpf.AI.Agents.ToolApprovals;
using FatturaFacile.Wpf.AI.Modules;

namespace FatturaFacile;

/// <summary>
/// Interaction logic and view module for human-in-the-loop AI agent workflows requiring explicit approval before executing sensitive tool calls.
/// </summary>
public partial class AIAgentWithToolApprovalModule : AgentModuleBase
{

    /// <summary>
    /// Identifier key used to register and resolve the associated chat client service in DI containers.
    /// </summary>
    public const string ServiceKey = nameof(AIAgentWithToolApprovalModule);

    /// <summary>
    /// Maps individual tool approval request identifiers to their parent batch instance.
    /// </summary>
    private readonly Dictionary<string, ToolApprovalRequestBatch> requestToBatchMap =
        new Dictionary<string, ToolApprovalRequestBatch>();

    /// <summary>
    /// Initializes a new instance of the <see cref="AIAgentWithToolApprovalModule"/> class.
    /// </summary>
    public AIAgentWithToolApprovalModule()
    {
        InitializeComponent();
        InitializeAgentModule(moduleChatControl: aiChatControl);

        aiChatControl.ContentFormat    =  ResponseContentFormat.PlainText;
        aiChatControl.ResponseReceived += ChatControl_ResponseReceived;
        var sendResponsesAsync = SendResponsesAsync;

        aiChatControl.MessageContentTemplate = (message) => (builder) =>
                                                            {
                                                                builder
                                                                   .OpenComponent<
                                                                        AIAgentWithToolApprovalMessageContentTemplate>(
                                                                                                                       sequence: 0);

                                                                builder.AddAttribute(sequence: 1, name: "Message", value: message);

                                                                builder.AddAttribute(
                                                                                     sequence: 2,
                                                                                     name: "RequestToBatchMap",
                                                                                     value: requestToBatchMap);

                                                                builder.AddAttribute(
                                                                                     sequence: 3,
                                                                                     name: "SendResponsesAsync",
                                                                                     value: sendResponsesAsync);

                                                                builder.CloseComponent();
                                                            };
    }

    /// <summary>
    /// Handles received responses and registers any contained tool approval requests.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The response received event arguments.</param>
    private void ChatControl_ResponseReceived(
        object                                 sender,
        AIChatControlResponseReceivedEventArgs e)
    {
        foreach (var message in e.Messages)
        {
            var requests = message.Contents.OfType<ToolApprovalRequestContent>().ToList();

            if (requests.Count == 0)
            {
                continue;
            }

            var requestBatch = new ToolApprovalRequestBatch(requests: requests);
            requests.ForEach(action: r => requestToBatchMap[key: r.RequestId] = requestBatch);

            message.Visible = true;
        }
    }

    /// <summary>
    /// Sends user approval decisions as AI content payloads back to the chat control.
    /// </summary>
    /// <param name="responses">The collection of AI response contents.</param>
    /// <returns>A task representing the asynchronous send operation.</returns>
    private async Task SendResponsesAsync(List<AIContent> responses)
    {
        if (responses == null || responses.Count == 0)
        {
            return;
        }

        await ChatControl.SendMessageAsync(contents: responses);
    }

    /// <inheritdoc />
    protected override string ChatClientServiceKey => ServiceKey;

    /// <inheritdoc />
    protected override PromptSuggestion[] PromptSuggestions => new[]
                                                               {
                                                                   new PromptSuggestion(
                                                                                        title: "Update inactive accounts",
                                                                                        text: "Move inactive Texas accounts to the Tier 2 service level.",
                                                                                        prompt: "Update all of our inactive accounts in Texas to Tier 2 status.",
                                                                                        sendOnClick: false),
                                                                   new PromptSuggestion(
                                                                                        title: "Nudge late payers",
                                                                                        text: "Send a friendly reminder to everyone with an unpaid invoice.",
                                                                                        prompt: "Follow up on all invoices that are more than 30 days overdue.",
                                                                                        sendOnClick: false),
                                                                   new PromptSuggestion(
                                                                                        title: "Archive past orders",
                                                                                        text: "Move orders placed before 2020 into long-term storage.",
                                                                                        prompt: "Tidy up the order history by archiving everything from before 2020.",
                                                                                        sendOnClick: false)
                                                               };

    /// <inheritdoc />
    protected override string Icon => "&#x1F6E1;&#xFE0F;";

    /// <inheritdoc />
    protected override string Title => "AI Chat with Tool Calling Approval";

    /// <inheritdoc />
    protected override string Description =>
        "For your security, sensitive AI actions require your final review and approval before execution.\n" +
        "Choose a suggestion below or enter your own prompt.";

}
