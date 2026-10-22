// <copyright file="WorkflowResponseStreamingModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.AIIntegration.Blazor.Chat.WebView;

namespace FatturaFacile;

/// <summary>
/// Interaction logic and view module demonstrating real-time response streaming and intermediate step telemetry from a multi-stage AI workflow.
/// </summary>
public partial class WorkflowResponseStreamingModule : AgentModuleBase
{
    /// <summary>
    /// Identifier key used to register and resolve the associated chat client service in DI containers.
    /// </summary>
    public const string ServiceKey = nameof(WorkflowResponseStreamingModule);

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowResponseStreamingModule"/> class.
    /// </summary>
    public WorkflowResponseStreamingModule()
    {
        InitializeComponent();
        InitializeAgentModule(aiChatControl);
    }

    /// <inheritdoc />
    protected override string ChatClientServiceKey => ServiceKey;

    /// <inheritdoc />
    protected override PromptSuggestion[] PromptSuggestions => new[]
    {
        new PromptSuggestion("Moon, whisper, ocean", "Tranquil & Mystical", "moon, whisper, ocean", false),
        new PromptSuggestion("Fire, dream, shadow", "Heroic & Hopeful", "fire, dream, shadow", false),
        new PromptSuggestion("Rose, thunder, silence", "Romantic & Contemplative", "rose, thunder, silence", false)
    };

    /// <inheritdoc />
    protected override string Icon => "&#x1F3AD;";

    /// <inheritdoc />
    protected override string Title => "Workflow Response Streaming";

    /// <inheritdoc />
    protected override string Description =>
        "This demo implements the same Shakespearean Poet pipeline\n" +
        "and uses a custom workflow provider implementation (WorkflowResponseProvider).\n" +
        "This provider streams events in real time. Enter <strong>2 or 3 keywords</strong> to get started.";
}
