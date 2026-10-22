// <copyright file="ToolCallingAgentModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.AIIntegration.Blazor.Chat.WebView;
using FatturaFacile.Wpf.AI.Modules;

namespace FatturaFacile;

/// <summary>
/// Interaction logic and view module for an AI agent equipped with function/tool calling capabilities.
/// </summary>
public partial class ToolCallingAgentModule : AgentModuleBase
{

    /// <summary>
    /// Identifier key used to register and resolve the associated chat client service in DI containers.
    /// </summary>
    public const string ServiceKey = nameof(ToolCallingAgentModule);

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolCallingAgentModule"/> class.
    /// </summary>
    public ToolCallingAgentModule()
    {
        InitializeComponent();
        InitializeAgentModule(moduleChatControl: aiChatControl);
    }

    /// <inheritdoc />
    protected override string ChatClientServiceKey => ServiceKey;

    /// <inheritdoc />
    protected override PromptSuggestion[] PromptSuggestions => new[]
                                                               {
                                                                   new PromptSuggestion(
                                                                                        title: "Current UTC Time",
                                                                                        text: "System Information",
                                                                                        prompt: "What is the current UTC time?",
                                                                                        sendOnClick: false),
                                                                   new PromptSuggestion(
                                                                                        title: "Add Numbers",
                                                                                        text: "Math Operation",
                                                                                        prompt: "What is the sum of 42 and 58?",
                                                                                        sendOnClick: false),
                                                                   new PromptSuggestion(
                                                                                        title: "Reverse Text",
                                                                                        text: "String Function",
                                                                                        prompt: "Reverse the text 'Hello, World!'.",
                                                                                        sendOnClick: false),
                                                               };

    /// <inheritdoc />
    protected override string Icon => "&#x1F527;";

    /// <inheritdoc />
    protected override string Title => "AI Agent with Tool Calling";

    /// <inheritdoc />
    protected override string Description =>
        "This simple <strong>AI Agent</strong> is equipped with tools to retrieve the current time,\n" +
        "add numbers, and reverse text.\n"                                                             +
        "Choose a suggestion below or ask the agent to use one of its tools.";

}
