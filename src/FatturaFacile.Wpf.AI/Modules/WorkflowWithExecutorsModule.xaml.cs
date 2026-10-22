// <copyright file="WorkflowWithExecutorsModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.AIIntegration.Blazor.Chat.WebView;

namespace FatturaFacile;

/// <summary>
/// Interaction logic and view module illustrating multi-step deterministic executor workflows (e.g., uppercase and reverse string transforms) with real-time UI execution step reporting.
/// </summary>
public partial class WorkflowWithExecutorsModule : AgentModuleBase
{
    /// <summary>
    /// Identifier key used to register and resolve the associated chat client service in DI containers.
    /// </summary>
    public const string ServiceKey = nameof(WorkflowWithExecutorsModule);

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowWithExecutorsModule"/> class.
    /// </summary>
    public WorkflowWithExecutorsModule()
    {
        InitializeComponent();
        InitializeAgentModule(aiChatControl);
    }

    /// <inheritdoc />
    protected override string ChatClientServiceKey => ServiceKey;

    /// <inheritdoc />
    protected override PromptSuggestion[] PromptSuggestions => new[]
    {
        new PromptSuggestion("Hello, World!", "Classic Greeting", "Hello, World!", false),
        new PromptSuggestion("DevExpress WPF", "Product Name", "DevExpress WPF", false),
        new PromptSuggestion("Workflow streaming rocks!", "Enthusiastic Message", "Workflow streaming rocks!", false)
    };

    /// <inheritdoc />
    protected override string Icon => "&#x1F504;";

    /// <inheritdoc />
    protected override string Title => "Workflow with Execution Steps";

    /// <inheritdoc />
    protected override string Description =>
        "Enter any text and start a two-step transformation workflow:\n" +
        "<strong>Uppercase</strong> &rarr; <strong>Reverse</strong>.\n" +
        "Our custom response provider (<em>WorkflowResponseProvider</em>) streams operation progress to the chat control in real time.";
}
