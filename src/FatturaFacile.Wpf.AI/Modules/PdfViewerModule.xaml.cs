// <copyright file="PdfViewerModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.Xpf.DemoBase.Helpers;
using FatturaFacile.Wpf.AI;

namespace FatturaFacile;

/// <summary>
/// Interaction logic and view module demonstrating AI-assisted reading, summarization, and explanation within the PDF Viewer control.
/// </summary>
public partial class PdfViewerModule
{

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfViewerModule"/> class and loads embedded demonstration PDF content.
    /// </summary>
    public PdfViewerModule()
    {
        InitializeComponent();

        WarningText = Resource.EndpointWarning;

        var assembly = typeof(PdfViewerModule).Assembly;

        var documentStream = assembly.GetManifestResourceStream(
                                                                name: DemoHelper.GetPath(
                                                                                         path: $"{assembly.GetName().Name}.Data.",
                                                                                         assembly: assembly) + "PageContent.pdf");

        pdfViewerControl.DocumentSource = documentStream;
    }

}
