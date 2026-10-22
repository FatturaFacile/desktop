// <copyright file="RichEditModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.Xpf.DemoBase.Helpers;
using FatturaFacile.Wpf.AI;

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

        WarningText = Resource.EndpointWarning;

        var assembly = typeof(RichEditModule).Assembly;

        var documentStream = assembly.GetManifestResourceStream(
                                                                name: DemoHelper.GetPath(
                                                                                         path: $"{assembly.GetName().Name}.Data.",
                                                                                         assembly: assembly) + "FirstLook.docx");

        _ = mainRichEditControl.LoadDocument(stream: documentStream);
    }

}
