// <copyright file="ReportViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Linq;

using DevExpress.DataAccess.ObjectBinding;
using DevExpress.Mvvm.CodeGenerators;
using DevExpress.XtraReports.UI;

using FatturaFacile.Infrastructure;

namespace FatturaFacile.ViewModels;
//Properties and commands are generated automatically for the following class when compiling:
//Refer to the following help topic for details: https://docs.devexpress.com/WPF/402989/mvvm-framework/viewmodels/compile-time-generated-viewmodels
[GenerateViewModel]
public partial class ReportViewModel
{
    [GenerateProperty]
    XtraReport report;

    public ReportViewModel()
    {
        Report = new Report();
    }
}
