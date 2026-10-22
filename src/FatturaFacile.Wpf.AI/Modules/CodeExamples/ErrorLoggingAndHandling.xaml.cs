// <copyright file="ErrorLoggingAndHandling.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using DevExpress.AIIntegration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FatturaFacile.Modules.CodeExamples;

public partial class ErrorLoggingAndHandling : System.Windows.Controls.UserControl
{
    private readonly AIExceptionHandler exceptionHandler;
    private AIExtensionsContainerDefault defaultAIContainer;

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
