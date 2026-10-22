// <copyright file="RichEditModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.Xpf.DemoBase.Helpers;

namespace FatturaFacile;

/// <summary>
/// Interaction logic and view module for AI-powered document authoring, proofreading, tone rewriting, and translation in the Rich Text Editor.
/// </summary>
public partial class RichEditModule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RichEditModule"/> class and loads embedded demonstration document content.
    /// </summary>
    public RichEditModule()
    {
        InitializeComponent();

        WarningText = Properties.Resource.EndpointWarning;

        var assembly = typeof(RichEditModule).Assembly;
        var documentStream = assembly.GetManifestResourceStream(
            DemoHelper.GetPath($"{assembly.GetName().Name}.Data.", assembly) + "FirstLook.docx");
        _ = mainRichEditControl.LoadDocument(documentStream);
    }
}
