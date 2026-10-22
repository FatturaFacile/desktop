// <copyright file="BackstageDocumentPreviewReportService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;
using FatturaFacile.Common.ViewModel.Services;
using FatturaFacile.Controls;
using DevExpress.Xpf.Ribbon;
using DevExpress.XtraReports;

namespace FatturaFacile.Common.View.Services;

/// <summary>
/// Report service implementation coordinating backstage document preview display within a DevExpress Ribbon backstage menu.
/// </summary>
public class BackstageDocumentPreviewReportService : ReportServiceBase {
    /// <summary>
    /// Identifies the <see cref="BackstageViewIsOpen"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty BackstageViewIsOpenProperty =
        DependencyProperty.Register("BackstageViewIsOpen", typeof(bool), typeof(BackstageDocumentPreviewReportService), new PropertyMetadata(false, (d, e) =>
            ((BackstageDocumentPreviewReportService)d).OnBackstageViewIsOpenChanged()));

    /// <summary>
    /// Identifies the <see cref="BackstageItem"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty BackstageItemProperty =
        DependencyProperty.Register("BackstageItem", typeof(BackstageItemBase), typeof(BackstageDocumentPreviewReportService), new PropertyMetadata(null, (d, e) =>
            ((BackstageDocumentPreviewReportService)d).OnBackstageItemChanged((BackstageItemBase)e.OldValue, (BackstageItemBase)e.NewValue)));

    /// <summary>
    /// Identifies the <see cref="BackstageDocumentPreview"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty BackstageDocumentPreviewProperty =
        DependencyProperty.Register("BackstageDocumentPreview", typeof(BackstageDocumentPreview), typeof(BackstageDocumentPreviewReportService), new PropertyMetadata(null, (d, e) =>
            ((BackstageDocumentPreviewReportService)d).OnBackstageDocumentPreviewChanged(e)));

    /// <summary>
    /// Gets or sets a value indicating whether the ribbon backstage view is open.
    /// </summary>
    public bool BackstageViewIsOpen {
        get => (bool)GetValue(BackstageViewIsOpenProperty);
        set => SetValue(BackstageViewIsOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets the target backstage item base control.
    /// </summary>
    public BackstageItemBase BackstageItem {
        get => (BackstageItemBase)GetValue(BackstageItemProperty);
        set => SetValue(BackstageItemProperty, value);
    }

    /// <summary>
    /// Gets or sets the associated <see cref="Controls.BackstageDocumentPreview"/> control.
    /// </summary>
    public BackstageDocumentPreview BackstageDocumentPreview {
        get => (BackstageDocumentPreview)GetValue(BackstageDocumentPreviewProperty);
        set => SetValue(BackstageDocumentPreviewProperty, value);
    }

    private bool isDocumentFirstLoading = true;

    /// <inheritdoc />
    protected override void ShowReport(IReportInfo reportInfo) {
        base.ShowReport(reportInfo);
        Dispatcher.BeginInvoke((Action)OpenBackstageView);
    }

    private void OpenBackstageView() {
        ((BackstageTabItem)BackstageItem).Backstage.SelectedTab = (BackstageTabItem)BackstageItem;
        BackstageItem.Backstage.IsOpen = true;
    }

    /// <inheritdoc />
    protected override void UpdateReportCore(IReportInfo? actualReportInfo) {
        base.UpdateReportCore(actualReportInfo);
        if (BackstageItem != null)
        {
            BackstageItem.IsEnabled = actualReportInfo != null;
        }
    }

    private void OnBackstageViewIsOpenChanged() => IsVisible = BackstageViewIsOpen;

    private void OnBackstageItemChanged(BackstageItemBase oldItem, BackstageItemBase newItem) {
        if(newItem != null)
        {
            newItem.IsEnabled = oldItem != null && oldItem.IsEnabled;
        }
    }

    private void OnBackstageDocumentPreviewChanged(DependencyPropertyChangedEventArgs e) {
        if(e.OldValue != null)
        {
            ((BackstageDocumentPreview)e.OldValue).Loaded -= BackstageDocumentPreview_Loaded;
        }

        if(e.NewValue != null)
        {
            ((BackstageDocumentPreview)e.NewValue).Loaded += BackstageDocumentPreview_Loaded;
        }
    }

    private void BackstageDocumentPreview_Loaded(object sender, RoutedEventArgs e) {
        var updateReportAction = () => CreateReport();
        var document = (BackstageDocumentPreview)sender;
        if(document.IsVisible && isDocumentFirstLoading) {
            Dispatcher.BeginInvoke(updateReportAction);
            isDocumentFirstLoading = false;
        }
    }

    /// <inheritdoc />
    protected override void SetDocumentSource(IReport report) => BackstageDocumentPreview.DocumentSource = report;

    /// <inheritdoc />
    protected override void SetCustomSettingsViewModel(object? customSettingsViewModel) => BackstageDocumentPreview.CustomSettings = customSettingsViewModel!;
}
