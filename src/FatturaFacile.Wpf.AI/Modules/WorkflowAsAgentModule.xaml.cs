// <copyright file="WorkflowAsAgentModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.AIIntegration.Blazor.Chat.WebView;
using FatturaFacile.Wpf.AI.Modules;

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
        InitializeAgentModule(moduleChatControl: aiChatControl);
    }

    /// <inheritdoc />
    protected override string ChatClientServiceKey => ServiceKey;

    /// <inheritdoc />
    protected override PromptSuggestion[] PromptSuggestions => new[]
                                                               {
                                                                   new PromptSuggestion(
                                                                                        title: "Moon, whisper, ocean",
                                                                                        text: "Tranquil & Mystical",
                                                                                        prompt: "moon, whisper, ocean",
                                                                                        sendOnClick: false),
                                                                   new PromptSuggestion(
                                                                                        title: "Fire, dream, shadow",
                                                                                        text: "Heroic & Hopeful",
                                                                                        prompt: "fire, dream, shadow",
                                                                                        sendOnClick: false),
                                                                   new PromptSuggestion(
                                                                                        title: "Rose, thunder, silence",
                                                                                        text: "Romantic & Contemplative",
                                                                                        prompt: "rose, thunder, silence",
                                                                                        sendOnClick: false)
                                                               };

    /// <inheritdoc />
    protected override string Icon => "&#x1F3AD;";

    /// <inheritdoc />
    protected override string Title => "Shakespearean Poet";

    /// <inheritdoc />
    protected override string Description =>
        "The AI Assistant uses a few keywords to craft a poem\n"                 +
        "then rewrites it in the grand style of <em>William Shakespeare</em>.\n" +
        "Choose a keyword combination below or enter <strong>2 or 3 custom keywords</strong>.";

}
