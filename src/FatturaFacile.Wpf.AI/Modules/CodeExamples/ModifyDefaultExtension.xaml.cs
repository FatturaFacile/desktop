// <copyright file="ModifyDefaultExtension.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;

using DevExpress.AIIntegration;
using DevExpress.AIIntegration.Extensions;
using FatturaFacile.Wpf.AI.Modules.CodeExamples;

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

    /// <summary>
    /// Handles the Loaded event to register the Shakespearean proofreading extension.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event data.</param>
    private void OnLoaded(
        object          sender,
        RoutedEventArgs e) => AIExtensionsContainerDesktop.Default
                                                          .Register<ProofreadRequest,
                                                               WilliamShakespeareStyleExtension>();

    /// <summary>
    /// Handles the Unloaded event to restore the default proofreading extension.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event data.</param>
    private void OnUnloaded(
        object          sender,
        RoutedEventArgs e) => AIExtensionsContainerDesktop.Default.Register<ProofreadRequest, ProofreadExtension>();

}
