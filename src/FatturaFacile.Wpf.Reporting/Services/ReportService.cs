// <copyright file="ReportService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.ComponentModel;

using DevExpress.Mvvm.UI;
using DevExpress.Xpf.DocumentViewer;
using DevExpress.Xpf.Printing;
using DevExpress.XtraReports;

namespace FatturaFacile.Wpf.Reporting.Services;

/// <summary>
/// Abstract base service managing report lifecycle, parameter observation, and presentation binding.
/// </summary>
public abstract class ReportServiceBase : ServiceBase, IReportService
{

    /// <summary>
    /// Indicates whether the report service is currently visible.
    /// </summary>
    private bool isVisible;

    /// <summary>
    /// The default report descriptor used when no specific report is set.
    /// </summary>
    private IReportInfo? defaultReportInfo;

    /// <summary>
    /// The currently assigned report descriptor.
    /// </summary>
    private IReportInfo? reportInfo;

    /// <summary>
    /// The actual active report descriptor being rendered.
    /// </summary>
    private IReportInfo? actualReportInfo;

    /// <summary>
    /// Gets or sets a value indicating whether the report service is currently visible.
    /// </summary>
    protected bool IsVisible
    {
        get => isVisible;
        set
        {
            isVisible = value;
            UpdateReport();

            if (!isVisible)
            {
                reportInfo = null;
            }
        }
    }

    /// <summary>
    /// Sets the default report displayed when no active report is selected.
    /// </summary>
    /// <param name="reportInfo">The default report descriptor.</param>
    protected virtual void SetDefaultReport(IReportInfo reportInfo)
    {
        defaultReportInfo = reportInfo;
        UpdateReport();
    }

    /// <summary>
    /// Activates and displays the specified report.
    /// </summary>
    /// <param name="reportInfo">The report descriptor to display.</param>
    protected virtual void ShowReport(IReportInfo reportInfo)
    {
        this.reportInfo = reportInfo;
        UpdateReport();
    }

    private void UpdateReport() => UpdateReportCore(actualReportInfo: IsVisible ? (reportInfo ?? defaultReportInfo) : null);

    /// <summary>
    /// Updates the core report binding, subscribing to parameter change notifications.
    /// </summary>
    /// <param name="actualReportInfo">The resolved active report descriptor.</param>
    protected virtual void UpdateReportCore(IReportInfo? actualReportInfo)
    {
        UnsubscribeFromParametersViewModel();
        this.actualReportInfo = actualReportInfo;
        SubscribeToParametersViewModel();

        if (this.actualReportInfo == null)
        {
            DestroyReport();
        }
        else
        {
            CreateReport();
        }
    }

    /// <summary>
    /// Handles property change notifications on the parameters view model to re-generate the report.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The property change event arguments.</param>
    private void OnParametersViewModelPropertyChanged(
        object?                  sender,
        PropertyChangedEventArgs e) => CreateReport();

    /// <summary>
    /// Creates and renders the report document from the active report descriptor.
    /// </summary>
    protected void CreateReport()
    {
        if (actualReportInfo == null)
        {
            return;
        }

        var report = actualReportInfo.CreateReport();

        if (report == null)
        {
            return;
        }

        SetCustomSettingsViewModel(customSettingsViewModel: actualReportInfo.ParametersViewModel);
        SetDocumentSource(report: report);
        report.PrintingSystemBase.ClearContent();
        report.CreateDocument(buildForInstantPreview: true);
    }

    /// <summary>
    /// Cleans up the custom settings view model when destroying the report.
    /// </summary>
    private void DestroyReport() => SetCustomSettingsViewModel(customSettingsViewModel: null);

    /// <summary>
    /// Sets the underlying document source on the target report viewer control.
    /// </summary>
    /// <param name="report">The report to display.</param>
    protected abstract void SetDocumentSource(IReport report);

    /// <summary>
    /// Binds custom parameters or settings viewmodel to the report viewer.
    /// </summary>
    /// <param name="customSettingsViewModel">The custom settings view model.</param>
    protected abstract void SetCustomSettingsViewModel(object? customSettingsViewModel);

    /// <summary>
    /// Gets the actual parameters view model from the active report descriptor.
    /// </summary>
    private object? ActualParametersViewModel => actualReportInfo?.ParametersViewModel;

    /// <summary>
    /// Subscribes to property change events on the parameters view model.
    /// </summary>
    private void SubscribeToParametersViewModel()
    {
        var parametersViewModel = ActualParametersViewModel as INotifyPropertyChanged;

        if (parametersViewModel != null)
        {
            parametersViewModel.PropertyChanged += OnParametersViewModelPropertyChanged;
        }
    }

    /// <summary>
    /// Unsubscribes from property change events on the parameters view model.
    /// </summary>
    private void UnsubscribeFromParametersViewModel()
    {
        var parametersViewModel = ActualParametersViewModel as INotifyPropertyChanged;

        if (parametersViewModel != null)
        {
            parametersViewModel.PropertyChanged -= OnParametersViewModelPropertyChanged;
        }
    }

    #region IReportService

    void IReportService.SetDefaultReport(IReportInfo reportInfo) => SetDefaultReport(reportInfo: reportInfo);

    void IReportService.ShowReport(IReportInfo reportInfo) => ShowReport(reportInfo: reportInfo);

    #endregion

}

/// <summary>
/// DevExpress <see cref="DocumentPreviewControl"/> implementation of <see cref="IReportService"/>.
/// </summary>
public class DocumentViewerReportService : ReportServiceBase
{

    private DocumentPreviewControl DocumentViewer => (DocumentPreviewControl)AssociatedObject;

    /// <summary>
    /// Gets or sets the document preview zoom mode.
    /// </summary>
    public ZoomMode ZoomMode { get; set; }

    /// <inheritdoc />
    protected override void OnAttached()
    {
        base.OnAttached();
        IsVisible = true;
        ZoomMode  = ZoomMode.FitToWidth;
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        base.OnDetaching();
        IsVisible = false;
    }

    /// <inheritdoc />
    protected override void SetCustomSettingsViewModel(object? customSettingsViewModel)
    {
    }

    /// <inheritdoc />
    protected override void SetDocumentSource(IReport report)
    {
        DocumentViewer.DocumentSource = report;
        DocumentViewer.ZoomMode       = ZoomMode;
    }

}
