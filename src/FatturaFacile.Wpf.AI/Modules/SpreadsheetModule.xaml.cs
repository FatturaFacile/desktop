// <copyright file="SpreadsheetModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.Xpf.DemoBase.Helpers;
using FatturaFacile.Wpf.AI;

namespace FatturaFacile;

/// <summary>
/// Interaction logic and view module for spreadsheet formula generation, cell data synthesis, and tabular analysis via AI extensions.
/// </summary>
public partial class SpreadsheetModule
{

    /// <summary>
    /// Initializes a new instance of the <see cref="SpreadsheetModule"/> class and loads an amortization schedule workbook.
    /// </summary>
    public SpreadsheetModule()
    {
        InitializeComponent();

        WarningText = Resource.EndpointWarning;

        var assembly = typeof(SpreadsheetModule).Assembly;

        var documentStream = assembly.GetManifestResourceStream(
                                                                name: DemoHelper.GetPath(
                                                                                         path: $"{assembly.GetName().Name}.Data.",
                                                                                         assembly: assembly) +
                                                                      "LoanAmortizationSchedule.xlsx");

        _ = mainSpreadsheetControl.LoadDocument(stream: documentStream);
    }

}
