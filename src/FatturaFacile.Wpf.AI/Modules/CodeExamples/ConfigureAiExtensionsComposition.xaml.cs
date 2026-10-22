// <copyright file="ConfigureAiExtensionsComposition.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;

using DevExpress.AIIntegration;
using DevExpress.AIIntegration.Extensions;

namespace FatturaFacile.Modules.CodeExamples;

public partial class ConfigureAiExtensionsComposition : System.Windows.Controls.UserControl
{
    private AIExtensionsContainerLocal localContainer = null;

    public ConfigureAiExtensionsComposition()
    {
        InitializeComponent();
    }

    private void OnLoaded(
        object sender,
        RoutedEventArgs e)
    {
        localContainer = new AIExtensionsContainerLocal(AIExtensionsContainerDesktop.Default);
        localContainer.Register<ProofreadRequest, ErnestHemingwayStyleExtension>();

        AIExtensionsContainerDesktop.QueryIAIExtensionsContainer += QueryAiExtensionsContainerHandler;
    }

    private void OnUnloaded(
        object sender,
        RoutedEventArgs e)
    {
        AIExtensionsContainerDesktop.QueryIAIExtensionsContainer -= QueryAiExtensionsContainerHandler;
        localContainer = null;
    }

    private void QueryAiExtensionsContainerHandler(
        object sender,
        AIExtensionsContainerQueryEventArgs e)
    {
        if (e.Source == textEdit)
        {
            e.Container = localContainer;
        }
    }
}
