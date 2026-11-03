// <copyright file="IReportService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.XtraReports;

namespace FatturaFacile.Wpf.Reporting.Services;

/// <summary>
/// Encapsulates metadata and factory operations for creating a report instance.
/// </summary>
public interface IReportInfo
{

    /// <summary>
    /// Gets the parameter ViewModel associated with the report, if any.
    /// </summary>
    public object ParametersViewModel { get; }

    /// <summary>
    /// Creates and returns a new <see cref="IReport"/> instance.
    /// </summary>
    /// <returns>The created report.</returns>
    public IReport CreateReport();

}

/// <summary>
/// Service contract for activating and presenting report previews.
/// </summary>
public interface IReportService
{

    /// <summary>
    /// Sets the default report displayed when no specific report is active.
    /// </summary>
    /// <param name="reportInfo">The default report descriptor.</param>
    public void SetDefaultReport(IReportInfo reportInfo);

    /// <summary>
    /// Activates and displays the specified report in the viewer.
    /// </summary>
    /// <param name="reportInfo">The report descriptor to display.</param>
    public void ShowReport(IReportInfo reportInfo);

}

/// <summary>
/// Report information descriptor for reports that do not require external parameters.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ParameterlessReportInfo"/> class.
/// </remarks>
/// <param name="report">The report instance.</param>
public class ParameterlessReportInfo(IReport report) : IReportInfo
{

    /// <summary>
    /// The encapsulated report instance.
    /// </summary>
    private readonly IReport report = report;

    object IReportInfo.ParametersViewModel => null;

    public IReport CreateReport() => report;

}

/// <summary>
/// Report information descriptor for parameterized reports.
/// </summary>
/// <typeparam name="TParametersViewModel">The type of the parameters view model.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="ReportInfo{TParametersViewModel}"/> class.
/// </remarks>
/// <param name="parametersViewModel">The view model containing report parameters.</param>
/// <param name="reportFactory">A factory delegate producing an <see cref="IReport"/> from the parameters.</param>
public class ReportInfo<TParametersViewModel>(TParametersViewModel parametersViewModel, Func<TParametersViewModel, IReport> reportFactory) : IReportInfo
{

    /// <summary>
    /// The view model containing report parameters.
    /// </summary>
    private readonly TParametersViewModel parametersViewModel = parametersViewModel;

    /// <summary>
    /// The factory delegate creating the report from parameters.
    /// </summary>
    private readonly Func<TParametersViewModel, IReport> reportFactory = reportFactory;

    object IReportInfo.ParametersViewModel => parametersViewModel;

    public IReport CreateReport() => reportFactory(arg: parametersViewModel);

}
