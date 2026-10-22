// <copyright file="ConfigureAiExtensionsComposition.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;

using DevExpress.AIIntegration;
using DevExpress.AIIntegration.Extensions;
using FatturaFacile.Wpf.AI.Modules.CodeExamples;

namespace FatturaFacile.Modules.CodeExamples;

/// <summary>
/// Interaction logic and demonstration view for scoping and cascading local AI extension containers over global defaults.
/// </summary>
public partial class ConfigureAiExtensionsComposition : System.Windows.Controls.UserControl
{

    /// <summary>
    /// The scoped local AI extensions container overriding global extensions for specific UI controls.
    /// </summary>
    private AIExtensionsContainerLocal localContainer = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureAiExtensionsComposition"/> class.
    /// </summary>
    public ConfigureAiExtensionsComposition()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles the Loaded event to configure local extensions and hook container query handlers.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The routed event arguments.</param>
    private void OnLoaded(
        object          sender,
        RoutedEventArgs e)
    {
        localContainer = new AIExtensionsContainerLocal(parentContainer: AIExtensionsContainerDesktop.Default);
        _              = localContainer.Register<ProofreadRequest, ErnestHemingwayStyleExtension>();

        AIExtensionsContainerDesktop.QueryIAIExtensionsContainer += QueryAiExtensionsContainerHandler;
    }

    /// <summary>
    /// Handles the Unloaded event to unsubscribe from container queries and clean up local resources.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The routed event arguments.</param>
    private void OnUnloaded(
        object          sender,
        RoutedEventArgs e)
    {
        AIExtensionsContainerDesktop.QueryIAIExtensionsContainer -= QueryAiExtensionsContainerHandler;
        localContainer                                           =  null;
    }

    /// <summary>
    /// Resolves the local container for the specific text edit control during AI integration container queries.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The container query arguments.</param>
    private void QueryAiExtensionsContainerHandler(
        object                              sender,
        AIExtensionsContainerQueryEventArgs e)
    {
        if (e.Source == textEdit)
        {
            e.Container = localContainer;
        }
    }

}
