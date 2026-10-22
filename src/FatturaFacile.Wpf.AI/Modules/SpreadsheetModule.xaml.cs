// <copyright file="SpreadsheetModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.Xpf.DemoBase.Helpers;

namespace FatturaFacile;

public partial class SpreadsheetModule
{
    public SpreadsheetModule()
    {
        InitializeComponent();

        WarningText = Properties.Resource.EndpointWarning;

        var assembly = typeof(SpreadsheetModule).Assembly;
        var documentStream = assembly.GetManifestResourceStream(
            DemoHelper.GetPath($"{assembly.GetName().Name}.Data.", assembly) + "LoanAmortizationSchedule.xlsx");
        _ = mainSpreadsheetControl.LoadDocument(documentStream);
    }
}
