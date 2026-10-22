// <copyright file="SmartPasteModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile;

[DevExpress.Xpf.DemoBase.CodeFile("ViewModels/SmartPasteModuleViewModel.cs")]
public partial class SmartPasteModule
{
    public SmartPasteModule()
    {
        InitializeComponent();

        WarningText = Properties.Resource.EndpointWarning;
    }
}
