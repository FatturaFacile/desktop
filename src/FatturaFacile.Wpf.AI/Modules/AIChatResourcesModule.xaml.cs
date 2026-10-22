// <copyright file="AIChatResourcesModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Modules.AIChat;

namespace FatturaFacile;

/// <summary>
/// Interaction logic and view module illustrating preloaded domain resources and reference file attachments in the AI Chat control.
/// </summary>
public partial class AIChatResourcesModule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AIChatResourcesModule"/> class.
    /// </summary>
    public AIChatResourcesModule()
    {
        InitializeComponent();

        WarningText = Properties.Resource.EndpointWarning;
        aiChatControl.ChatResources = AIChatResourcesDataProvider.GetData();
    }
}
