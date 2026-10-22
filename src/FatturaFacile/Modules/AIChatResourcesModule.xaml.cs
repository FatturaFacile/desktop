// <copyright file="AIChatResourcesModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile; 
public partial class AIChatResourcesModule {
    public AIChatResourcesModule() {
        InitializeComponent();

        WarningText = Properties.Resource.EndpointWarning;
        aiChatControl.ChatResources = AIChatResourcesDataProvider.GetData();
    }
}
