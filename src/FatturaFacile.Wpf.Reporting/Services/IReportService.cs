// <copyright file="IReportService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.XtraReports;

namespace FatturaFacile.Common.ViewModel.Services;

/// <summary>
/// Encapsulates metadata and factory operations for creating a report instance.
/// </summary>
public interface IReportInfo {
    /// <summary>
    /// Gets the parameter ViewModel associated with the report, if any.
    /// </summary>
    object? ParametersViewModel { get; }

    /// <summary>
    /// Creates and returns a new <see cref="IReport"/> instance.
    /// </summary>
    /// <returns>The created report.</returns>
    IReport CreateReport();
}

/// <summary>
/// Report information descriptor for reports that do not require external parameters.
/// </summary>
public class ParameterlessReportInfo : IReportInfo {
    private readonly IReport report;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterlessReportInfo"/> class.
    /// </summary>
    /// <param name="report">The report instance.</param>
    public ParameterlessReportInfo(IReport report) {
        this.report = report;
    }

    object? IReportInfo.ParametersViewModel => null;

    IReport IReportInfo.CreateReport() => report;
}

/// <summary>
/// Report information descriptor for parameterized reports.
/// </summary>
/// <typeparam name="TParametersViewModel">The type of the parameters view model.</typeparam>
public class ReportInfo<TParametersViewModel> : IReportInfo {
    private readonly TParametersViewModel parametersViewModel;
    private readonly Func<TParametersViewModel, IReport> reportFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportInfo{TParametersViewModel}"/> class.
    /// </summary>
    /// <param name="parametersViewModel">The view model containing report parameters.</param>
    /// <param name="reportFactory">A factory delegate producing an <see cref="IReport"/> from the parameters.</param>
    public ReportInfo(TParametersViewModel parametersViewModel, Func<TParametersViewModel, IReport> reportFactory) {
        this.parametersViewModel = parametersViewModel;
        this.reportFactory = reportFactory;
    }

    object? IReportInfo.ParametersViewModel => parametersViewModel;

    IReport IReportInfo.CreateReport() => reportFactory(parametersViewModel);
}

/// <summary>
/// Service contract for activating and presenting report previews.
/// </summary>
public interface IReportService {
    /// <summary>
    /// Sets the default report displayed when no specific report is active.
    /// </summary>
    /// <param name="reportInfo">The default report descriptor.</param>
    void SetDefaultReport(IReportInfo reportInfo);

    /// <summary>
    /// Activates and displays the specified report in the viewer.
    /// </summary>
    /// <param name="reportInfo">The report descriptor to display.</param>
    void ShowReport(IReportInfo reportInfo);
}
