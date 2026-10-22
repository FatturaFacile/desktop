// <copyright file="ModifyDefaultExtension.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;
using DevExpress.AIIntegration;
using DevExpress.AIIntegration.Extensions;

namespace FatturaFacile.Modules.CodeExamples;

/// <summary>
/// Interaction logic and demonstration view illustrating how to override default built-in AI extensions with custom implementations.
/// </summary>
public partial class ModifyDefaultExtension
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModifyDefaultExtension"/> class.
    /// </summary>
    public ModifyDefaultExtension()
    {
        InitializeComponent();
    }

    private void OnLoaded(
        object sender,
        RoutedEventArgs e) => AIExtensionsContainerDesktop.Default
        .Register<ProofreadRequest, WilliamShakespeareStyleExtension>();

    private void OnUnloaded(
        object sender,
        RoutedEventArgs e) => AIExtensionsContainerDesktop.Default.Register<ProofreadRequest, ProofreadExtension>();
}
