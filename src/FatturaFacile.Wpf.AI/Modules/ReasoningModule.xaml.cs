// <copyright file="ReasoningModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.AIIntegration.Blazor.Chat;
using DevExpress.AIIntegration.Blazor.Chat.WebView;
using FatturaFacile.Wpf.AI.Modules;
using Microsoft.Extensions.AI;

namespace FatturaFacile;

/// <summary>
/// Interaction logic and view module demonstrating chain-of-thought and structured reasoning capabilities with advanced LLMs.
/// </summary>
public partial class ReasoningModule : AgentModuleBase
{

    /// <summary>
    /// Identifier key used to register and resolve the associated chat client service in DI containers.
    /// </summary>
    public const string ServiceKey = nameof(ReasoningModule);

    /// <summary>
    /// System instruction directing the model to reason step-by-step without specialized math markup.
    /// </summary>
    private const string SystemPrompt =
        "Always reason through every question before giving your final answer, regardless of how simple it seems. " +
        "Do not use LaTeX, KaTeX, MathJax, or any other mathematical markup. "                                      +
        "Express mathematical content in plain text or Markdown (e.g., use C(3,2) instead of \\binom{3}{2}).";

    /// <summary>
    /// Initializes a new instance of the <see cref="ReasoningModule"/> class.
    /// </summary>
    public ReasoningModule()
    {
        InitializeComponent();
        InitializeAgentModule(moduleChatControl: aiChatControl);
        aiChatControl.ContentFormat =  ResponseContentFormat.PlainText;
        aiChatControl.Initialized   += ChatControl_Initialized;

        aiChatControl.MessageContentTemplate = (message) => (builder) =>
                                                            {
                                                                builder.OpenComponent<ReasoningMessage>(sequence: 0);
                                                                builder.AddAttribute(sequence: 1, name: "Message", value: message);
                                                                builder.CloseComponent();
                                                            };
    }

    /// <summary>
    /// Handles the chat control initialized event, setting up initial system message.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">Event arguments for chat control initialization.</param>
    private void ChatControl_Initialized(
        object                            sender,
        AIChatControlInitializedEventArgs e) =>
        ChatControl.LoadMessages(
                                 messages: new[]
                                           {
                                               new BlazorChatMessage(role: ChatRole.System, text: SystemPrompt)
                                           });

    /// <inheritdoc />
    protected override string ChatClientServiceKey => ServiceKey;

    /// <inheritdoc />
    protected override PromptSuggestion[] PromptSuggestions => new[]
    {
        new PromptSuggestion(
            title: "Water Jug Problem",
            text: "Classic Logic Puzzle",
            prompt: "You have a 3-gallon jug and a 5-gallon jug. How can you measure exactly 4 gallons of water? " +
                    "Explain each step.",
            sendOnClick: false),
        new PromptSuggestion(
            title: "Probability Puzzle",
            text: "Math Reasoning",
            prompt: "A bag contains 3 red balls and 5 blue balls. You draw 2 balls without replacement. " +
                    "What is the probability that both balls are the same color? Show your work step by step.",
            sendOnClick: false),
        new PromptSuggestion(
            title: "Knights and Knaves",
            text: "Logical Deduction",
            prompt: "On an island, every person is either a knight (always tells the truth) or a knave (always lies). " +
                    "You meet two people, A and B. A says 'At least one of us is a knave.' What are A and B?",
            sendOnClick: false)
    };

    /// <inheritdoc />
    protected override string Icon => "&#x1F9E0;";

    /// <inheritdoc />
    protected override string Title => "Responses API & Reasoning Models";

    /// <inheritdoc />
    protected override string Description =>
        "Test the model's reasoning capabilities with logic puzzles, math problems,\n" +
        "coding challenges, and other multi-step tasks.\n"                             +
        "Choose a suggestion below or enter your own prompt.";

}
