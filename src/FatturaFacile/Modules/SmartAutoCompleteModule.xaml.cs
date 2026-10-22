// <copyright file="SmartAutoCompleteModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile; 
public partial class SmartAutoCompleteModule {
    public SmartAutoCompleteModule() {
        InitializeComponent();

        WarningText = Properties.Resource.EndpointWarning;
    }
}

public class UserRoleItem {
    public string UserRole { get; set; }
    public string DisplayText { get; set; }
}
