// <copyright file="SmartPasteModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Wpf.AI;

namespace FatturaFacile;

/// <summary>
/// Interaction logic and view module demonstrating AI-driven clipboard parsing and automatic form filling (Smart Paste).
/// </summary>
[DevExpress.Xpf.DemoBase.CodeFile(path: "ViewModels/SmartPasteModuleViewModel.cs")]
public partial class SmartPasteModule
{

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartPasteModule"/> class.
    /// </summary>
    public SmartPasteModule()
    {
        InitializeComponent();

        WarningText = Resource.EndpointWarning;
    }

}
