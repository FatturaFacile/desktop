// <copyright file="ReportViewerModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.Xpf.DemoBase.Helpers;
using FatturaFacile.Wpf.Reporting;

namespace FatturaFacile;

/// <summary>
/// Interaction logic and document viewer presentation for the report preview module.
/// </summary>
public partial class ReportViewerModule
{

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportViewerModule"/> class, configuring default resources and sample report document streams.
    /// </summary>
    public ReportViewerModule()
    {
        InitializeComponent();

        WarningText = Resource.EndpointWarning;

        var assembly = typeof(ReportViewerModule).Assembly;

        var documentStream = assembly.GetManifestResourceStream(
                                                                name: DemoHelper.GetPath(
                                                                                         path: $"{assembly.GetName().Name}.Data.",
                                                                                         assembly: assembly) + "TableReport.prnx");

        documentPreview.DocumentSource = documentStream;
    }

}
