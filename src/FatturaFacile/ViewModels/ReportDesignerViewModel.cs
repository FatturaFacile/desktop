// <copyright file="ReportDesignerViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.Mvvm.POCO;
using DevExpress.XtraReports.UI;

namespace FatturaFacile.ViewModels; 
public class ReportDesignerViewModel {
    public static ReportDesignerViewModel Create(XtraReport report) {
        return ViewModelSource.Create(() => new ReportDesignerViewModel(report));
    }
    protected ReportDesignerViewModel(XtraReport report) {
        Report = report;
    }

    public XtraReport Report { get; private set; }
    public bool IsReportSaved { get; private set; }

    public virtual void Save() {
        IsReportSaved = true;
    }
}
