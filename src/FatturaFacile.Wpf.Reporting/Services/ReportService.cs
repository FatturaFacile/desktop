// <copyright file="ReportService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Windows;
using FatturaFacile.Common.ViewModel.Services;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.DocumentViewer;
using DevExpress.Xpf.Printing;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports;

namespace FatturaFacile.Common.View.Services;

/// <summary>
/// Abstract base service managing report lifecycle, parameter observation, and presentation binding.
/// </summary>
public abstract class ReportServiceBase : ServiceBase, IReportService
{
    private bool isVisible;
    private IReportInfo? defaultReportInfo;
    private IReportInfo? reportInfo;
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
    protected virtual void SetDefaultReport(
        IReportInfo reportInfo)
    {
        defaultReportInfo = reportInfo;
        UpdateReport();
    }

    /// <summary>
    /// Activates and displays the specified report.
    /// </summary>
    /// <param name="reportInfo">The report descriptor to display.</param>
    protected virtual void ShowReport(
        IReportInfo reportInfo)
    {
        this.reportInfo = reportInfo;
        UpdateReport();
    }

    private void UpdateReport() => UpdateReportCore(IsVisible ? (reportInfo ?? defaultReportInfo) : null);

    /// <summary>
    /// Updates the core report binding, subscribing to parameter change notifications.
    /// </summary>
    /// <param name="actualReportInfo">The resolved active report descriptor.</param>
    protected virtual void UpdateReportCore(
        IReportInfo? actualReportInfo)
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

    private void OnParametersViewModelPropertyChanged(
        object? sender,
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

        SetCustomSettingsViewModel(actualReportInfo.ParametersViewModel);
        SetDocumentSource(report);
        report.PrintingSystemBase.ClearContent();
        report.CreateDocument(true);
    }

    private void DestroyReport() => SetCustomSettingsViewModel(null);

    /// <summary>
    /// Sets the underlying document source on the target report viewer control.
    /// </summary>
    /// <param name="report">The report to display.</param>
    protected abstract void SetDocumentSource(
        IReport report);

    /// <summary>
    /// Binds custom parameters or settings viewmodel to the report viewer.
    /// </summary>
    /// <param name="customSettingsViewModel">The custom settings view model.</param>
    protected abstract void SetCustomSettingsViewModel(
        object? customSettingsViewModel);

    private object? ActualParametersViewModel => actualReportInfo?.ParametersViewModel;

    private void SubscribeToParametersViewModel()
    {
        var parametersViewModel = ActualParametersViewModel as INotifyPropertyChanged;
        if (parametersViewModel != null)
        {
            parametersViewModel.PropertyChanged += OnParametersViewModelPropertyChanged;
        }
    }

    private void UnsubscribeFromParametersViewModel()
    {
        var parametersViewModel = ActualParametersViewModel as INotifyPropertyChanged;
        if (parametersViewModel != null)
        {
            parametersViewModel.PropertyChanged -= OnParametersViewModelPropertyChanged;
        }
    }

    #region IReportService

    void IReportService.SetDefaultReport(
        IReportInfo reportInfo) => SetDefaultReport(reportInfo);

    void IReportService.ShowReport(
        IReportInfo reportInfo) => ShowReport(reportInfo);

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
        ZoomMode = ZoomMode.FitToWidth;
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        base.OnDetaching();
        IsVisible = false;
    }

    /// <inheritdoc />
    protected override void SetCustomSettingsViewModel(
        object? customSettingsViewModel)
    {
    }

    /// <inheritdoc />
    protected override void SetDocumentSource(
        IReport report)
    {
        DocumentViewer.DocumentSource = report;
        DocumentViewer.ZoomMode = ZoomMode;
    }
}
