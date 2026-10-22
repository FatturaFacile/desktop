// <copyright file="PdfViewerModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.Xpf.DemoBase.Helpers;

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

        WarningText = Properties.Resource.EndpointWarning;

        var assembly = typeof(PdfViewerModule).Assembly;
        var documentStream = assembly.GetManifestResourceStream(
            DemoHelper.GetPath($"{assembly.GetName().Name}.Data.", assembly) + "PageContent.pdf");
        pdfViewerControl.DocumentSource = documentStream;
    }
}
