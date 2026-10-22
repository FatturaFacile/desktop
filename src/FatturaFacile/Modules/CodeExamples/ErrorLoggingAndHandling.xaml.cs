// <copyright file="ErrorLoggingAndHandling.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using DevExpress.AIIntegration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FatturaFacile.Modules.CodeExamples; 
public partial class ErrorLoggingAndHandling : UserControl {
    readonly AIExceptionHandler exceptionHandler;
    AIExtensionsContainerDefault defaultAIContainer;
    public ErrorLoggingAndHandling() {
        InitializeComponent();
        ILogger logger = NullLogger<AIExceptionHandler>.Instance;
        exceptionHandler = new AIExceptionHandler(logger);
        defaultAIContainer = new AIExtensionsContainerDefault();
    }
    void OnLoaded(object sender, RoutedEventArgs e) {
        defaultAIContainer.RegisterAIExceptionHandler(exceptionHandler);
    }
    void OnUnloaded(object sender, RoutedEventArgs e) {
        defaultAIContainer.UnregisterAIExceptionHandler();
    }
}
