// <copyright file="WorkflowAsAgentModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.AIIntegration.Blazor.Chat.WebView;

namespace FatturaFacile;

/// <summary>
/// Interaction logic and view module hosting an agent backed by a multi-step composition workflow (e.g., poetic ideation followed by Shakespearean styling).
/// </summary>
public partial class WorkflowAsAgentModule : AgentModuleBase
{
    /// <summary>
    /// Identifier key used to register and resolve the associated chat client service in DI containers.
    /// </summary>
    public const string ServiceKey = nameof(WorkflowAsAgentModule);

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowAsAgentModule"/> class.
    /// </summary>
    public WorkflowAsAgentModule()
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
    protected override string Title => "Shakespearean Poet";

    /// <inheritdoc />
    protected override string Description =>
        "The AI Assistant uses a few keywords to craft a poem\n" +
        "then rewrites it in the grand style of <em>William Shakespeare</em>.\n" +
        "Choose a keyword combination below or enter <strong>2 or 3 custom keywords</strong>.";
}
