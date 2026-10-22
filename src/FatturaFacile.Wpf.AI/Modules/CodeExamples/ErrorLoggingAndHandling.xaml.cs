// <copyright file="ErrorLoggingAndHandling.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;

using DevExpress.AIIntegration;
using FatturaFacile.Wpf.AI.Modules.CodeExamples;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FatturaFacile.Modules.CodeExamples;

/// <summary>
/// Interaction logic and demonstration view for centralizing and configuring AI exception interceptors and logging pipelines.
/// </summary>
public partial class ErrorLoggingAndHandling : System.Windows.Controls.UserControl
{

    /// <summary>
    /// The demonstration AI exception interceptor.
    /// </summary>
    private readonly AIExceptionHandler exceptionHandler;

    /// <summary>
    /// The demonstration AI extensions container.
    /// </summary>
    private AIExtensionsContainerDefault defaultAIContainer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorLoggingAndHandling"/> class.
    /// </summary>
    public ErrorLoggingAndHandling()
    {
        InitializeComponent();
        ILogger logger = NullLogger<AIExceptionHandler>.Instance;
        exceptionHandler   = new AIExceptionHandler(logger: logger);
        defaultAIContainer = new AIExtensionsContainerDefault();
    }

    /// <summary>
    /// Handles the Loaded event to register the AI exception handler.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event data.</param>
    private void OnLoaded(
        object          sender,
        RoutedEventArgs e) => defaultAIContainer.RegisterAIExceptionHandler(handler: exceptionHandler);

    /// <summary>
    /// Handles the Unloaded event to unregister the AI exception handler.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event data.</param>
    private void OnUnloaded(
        object          sender,
        RoutedEventArgs e) => defaultAIContainer.UnregisterAIExceptionHandler();

}
