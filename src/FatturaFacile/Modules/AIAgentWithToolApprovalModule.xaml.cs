using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIDemo.Agents.ToolApprovals;
using DevExpress.AIIntegration.Blazor.Chat;
using DevExpress.AIIntegration.Blazor.Chat.WebView;
using Microsoft.Extensions.AI;

namespace AIDemo {
    public partial class AIAgentWithToolApprovalModule : AgentModuleBase {
        public const string ServiceKey = nameof(AIAgentWithToolApprovalModule);

        readonly Dictionary<string, ToolApprovalRequestBatch> requestToBatchMap = new Dictionary<string, ToolApprovalRequestBatch>();

        public AIAgentWithToolApprovalModule() {
            InitializeComponent();
            InitializeAgentModule(aiChatControl);

            aiChatControl.ContentFormat = ResponseContentFormat.PlainText;
            aiChatControl.ResponseReceived += ChatControl_ResponseReceived;
            Func<List<AIContent>, Task> sendResponsesAsync = SendResponsesAsync;
            aiChatControl.MessageContentTemplate = (message) => (builder) => {
                builder.OpenComponent<AIAgentWithToolApprovalMessageContentTemplate>(0);
                builder.AddAttribute(1, "Message", message);
                builder.AddAttribute(2, "RequestToBatchMap", requestToBatchMap);
                builder.AddAttribute(3, "SendResponsesAsync", sendResponsesAsync);
                builder.CloseComponent();
            };
        }

        void ChatControl_ResponseReceived(object sender, AIChatControlResponseReceivedEventArgs e) {
            foreach(var message in e.Messages) {
                var requests = message.Contents.OfType<ToolApprovalRequestContent>().ToList();
                if(requests.Count == 0)
                    continue;

                var requestBatch = new ToolApprovalRequestBatch(requests);
                requests.ForEach(r => requestToBatchMap[r.RequestId] = requestBatch);

                message.Visible = true;
            }
        }

        async Task SendResponsesAsync(List<AIContent> responses) {
            if(responses == null || responses.Count == 0)
                return;
            await ChatControl.SendMessageAsync(responses);
        }

        protected override string ChatClientServiceKey => ServiceKey;
        protected override PromptSuggestion[] PromptSuggestions => new[] {
            new PromptSuggestion("Update inactive accounts", "Move inactive Texas accounts to the Tier 2 service level.", "Update all of our inactive accounts in Texas to Tier 2 status.", false),
            new PromptSuggestion("Nudge late payers", "Send a friendly reminder to everyone with an unpaid invoice.", "Follow up on all invoices that are more than 30 days overdue.", false),
            new PromptSuggestion("Archive past orders", "Move orders placed before 2020 into long-term storage.", "Tidy up the order history by archiving everything from before 2020.", false)
        };
        protected override string Icon => "&#x1F6E1;&#xFE0F;";
        protected override string Title => "AI Chat with Tool Calling Approval";
        protected override string Description =>
            "For your security, sensitive AI actions require your final review and approval before execution.\n" +
            "Choose a suggestion below or enter your own prompt.";
    }
}
