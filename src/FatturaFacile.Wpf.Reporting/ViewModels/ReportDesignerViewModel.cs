// <copyright file="ReportDesignerViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.Mvvm.POCO;
using DevExpress.XtraReports.UI;

namespace FatturaFacile.ViewModels; 

/// <summary>
/// ViewModel managing report designer interactions, active report definition, and save state.
/// </summary>
public class ReportDesignerViewModel {
    /// <summary>
    /// Creates a new instance of <see cref="ReportDesignerViewModel"/> via DevExpress POCO factory.
    /// </summary>
    /// <param name="report">The report to be designed.</param>
    /// <returns>A new <see cref="ReportDesignerViewModel"/> instance.</returns>
    public static ReportDesignerViewModel Create(XtraReport report) => ViewModelSource.Create(() => new ReportDesignerViewModel(report));

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportDesignerViewModel"/> class.
    /// </summary>
    /// <param name="report">The report instance.</param>
    protected ReportDesignerViewModel(XtraReport report) {
        Report = report;
    }

    /// <summary>
    /// Gets the report instance currently opened in the designer.
    /// </summary>
    public XtraReport Report { get; private set; }

    /// <summary>
    /// Gets a value indicating whether changes to the report have been saved.
    /// </summary>
    public bool IsReportSaved { get; private set; }

    /// <summary>
    /// Marks the current report state as saved.
    /// </summary>
    public virtual void Save() => IsReportSaved = true;
}
