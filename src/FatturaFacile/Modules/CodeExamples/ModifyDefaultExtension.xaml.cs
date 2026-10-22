// <copyright file="ModifyDefaultExtension.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;
using DevExpress.AIIntegration;
using DevExpress.AIIntegration.Extensions;

namespace FatturaFacile.Modules.CodeExamples; 
public partial class ModifyDefaultExtension {
    public ModifyDefaultExtension() {
        InitializeComponent();
    }

    void OnLoaded(object sender, RoutedEventArgs e) {
        AIExtensionsContainerDesktop.Default.Register<ProofreadRequest, WilliamShakespeareStyleExtension>();
    }

    void OnUnloaded(object sender, RoutedEventArgs e) {
        AIExtensionsContainerDesktop.Default.Register<ProofreadRequest, ProofreadExtension>();
    }
}
