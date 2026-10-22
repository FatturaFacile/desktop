// <copyright file="ReportViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.Mvvm.CodeGenerators;
using DevExpress.XtraReports.UI;



namespace FatturaFacile.ViewModels;
//Properties and commands are generated automatically for the following class when compiling:
//Refer to the following help topic for details: https://docs.devexpress.com/WPF/402989/mvvm-framework/viewmodels/compile-time-generated-viewmodels
[GenerateViewModel]
public partial class ReportViewModel
{
    [GenerateProperty] private XtraReport report;

    public ReportViewModel()
    {
        Report = new Report();
    }
}

