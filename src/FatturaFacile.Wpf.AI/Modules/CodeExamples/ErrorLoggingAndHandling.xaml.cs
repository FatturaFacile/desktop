// <copyright file="ErrorLoggingAndHandling.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;

using DevExpress.AIIntegration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FatturaFacile.Modules.CodeExamples;

/// <summary>
/// Interaction logic and demonstration view for centralizing and configuring AI exception interceptors and logging pipelines.
/// </summary>
public partial class ErrorLoggingAndHandling : System.Windows.Controls.UserControl
{
    private readonly AIExceptionHandler exceptionHandler;
    private AIExtensionsContainerDefault defaultAIContainer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorLoggingAndHandling"/> class.
    /// </summary>
    public ErrorLoggingAndHandling()
    {
        InitializeComponent();
        ILogger logger = NullLogger<AIExceptionHandler>.Instance;
        exceptionHandler = new AIExceptionHandler(logger);
        defaultAIContainer = new AIExtensionsContainerDefault();
    }

    private void OnLoaded(
        object sender,
        RoutedEventArgs e) => defaultAIContainer.RegisterAIExceptionHandler(exceptionHandler);

    private void OnUnloaded(
        object sender,
        RoutedEventArgs e) => defaultAIContainer.UnregisterAIExceptionHandler();
}
