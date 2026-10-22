// <copyright file="AIAgentWithToolApprovalModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Agents.ToolApprovals;
using DevExpress.AIIntegration.Blazor.Chat;
using DevExpress.AIIntegration.Blazor.Chat.WebView;

using Microsoft.Extensions.AI;

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

    private readonly Dictionary<string, ToolApprovalRequestBatch> requestToBatchMap =
        new Dictionary<string, ToolApprovalRequestBatch>();

    /// <summary>
    /// Initializes a new instance of the <see cref="AIAgentWithToolApprovalModule"/> class.
    /// </summary>
    public AIAgentWithToolApprovalModule()
    {
        InitializeComponent();
        InitializeAgentModule(aiChatControl);

        aiChatControl.ContentFormat = ResponseContentFormat.PlainText;
        aiChatControl.ResponseReceived += ChatControl_ResponseReceived;
        var sendResponsesAsync = SendResponsesAsync;
        aiChatControl.MessageContentTemplate = (message) => (builder) =>
        {
            builder.OpenComponent<AIAgentWithToolApprovalMessageContentTemplate>(0);
            builder.AddAttribute(1, "Message", message);
            builder.AddAttribute(2, "RequestToBatchMap", requestToBatchMap);
            builder.AddAttribute(3, "SendResponsesAsync", sendResponsesAsync);
            builder.CloseComponent();
        };
    }

    private void ChatControl_ResponseReceived(
        object sender,
        AIChatControlResponseReceivedEventArgs e)
    {
        foreach (var message in e.Messages)
        {
            var requests = message.Contents.OfType<ToolApprovalRequestContent>().ToList();
            if (requests.Count == 0)
            {
                continue;
            }

            var requestBatch = new ToolApprovalRequestBatch(requests);
            requests.ForEach(r => requestToBatchMap[r.RequestId] = requestBatch);

            message.Visible = true;
        }
    }

    private async Task SendResponsesAsync(
        List<AIContent> responses)
    {
        if (responses == null || responses.Count == 0)
        {
            return;
        }

        await ChatControl.SendMessageAsync(responses);
    }

    /// <inheritdoc />
    protected override string ChatClientServiceKey => ServiceKey;

    /// <inheritdoc />
    protected override PromptSuggestion[] PromptSuggestions => new[]
    {
        new PromptSuggestion(
            "Update inactive accounts", "Move inactive Texas accounts to the Tier 2 service level.",
            "Update all of our inactive accounts in Texas to Tier 2 status.", false),
        new PromptSuggestion(
            "Nudge late payers", "Send a friendly reminder to everyone with an unpaid invoice.",
            "Follow up on all invoices that are more than 30 days overdue.", false),
        new PromptSuggestion(
            "Archive past orders", "Move orders placed before 2020 into long-term storage.",
            "Tidy up the order history by archiving everything from before 2020.", false)
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
