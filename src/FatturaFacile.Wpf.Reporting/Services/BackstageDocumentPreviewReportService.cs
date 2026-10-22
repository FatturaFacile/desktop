// <copyright file="BackstageDocumentPreviewReportService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;

using DevExpress.Xpf.Ribbon;
using DevExpress.XtraReports;
using FatturaFacile.Wpf.Reporting.Controls;

namespace FatturaFacile.Wpf.Reporting.Services;

/// <summary>
/// Report service implementation coordinating backstage document preview display within a DevExpress Ribbon backstage menu.
/// </summary>
public class BackstageDocumentPreviewReportService : ReportServiceBase
{

    /// <summary>
    /// Identifies the <see cref="BackstageViewIsOpen"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty BackstageViewIsOpenProperty = DependencyProperty.Register(
                                                                                                        name: "BackstageViewIsOpen",
                                                                                                        propertyType: typeof(bool),
                                                                                                        ownerType: typeof(
                                                                                                            BackstageDocumentPreviewReportService),
                                                                                                        typeMetadata: new
                                                                                                            PropertyMetadata(
                                                                                                                             defaultValue: false,
                                                                                                                             propertyChangedCallback: (
                                                                                                                                                              d,
                                                                                                                                                              e) =>
                                                                                                                                                          ((BackstageDocumentPreviewReportService)
                                                                                                                                                              d)
                                                                                                                                                         .OnBackstageViewIsOpenChanged()));

    /// <summary>
    /// Identifies the <see cref="BackstageItem"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty BackstageItemProperty = DependencyProperty.Register(
                                                                                                  name: "BackstageItem",
                                                                                                  propertyType: typeof(
                                                                                                      BackstageItemBase),
                                                                                                  ownerType: typeof(
                                                                                                      BackstageDocumentPreviewReportService),
                                                                                                  typeMetadata: new PropertyMetadata(
                                                                                                                                     defaultValue: null,
                                                                                                                                     propertyChangedCallback: (
                                                                                                                                                                      d,
                                                                                                                                                                      e) =>
                                                                                                                                                                  ((BackstageDocumentPreviewReportService)
                                                                                                                                                                      d)
                                                                                                                                                                 .OnBackstageItemChanged(
                                                                                                                                                                                         oldItem: (BackstageItemBase)
                                                                                                                                                                                         e.OldValue,
                                                                                                                                                                                         newItem: (BackstageItemBase)
                                                                                                                                                                                         e.NewValue)));

    /// <summary>
    /// Identifies the <see cref="BackstageDocumentPreview"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty BackstageDocumentPreviewProperty = DependencyProperty.Register(
                                                                                                             name: "BackstageDocumentPreview",
                                                                                                             propertyType: typeof(
                                                                                                                 BackstageDocumentPreview),
                                                                                                             ownerType: typeof(
                                                                                                                 BackstageDocumentPreviewReportService),
                                                                                                             typeMetadata: new
                                                                                                                 PropertyMetadata(
                                                                                                                                  defaultValue: null,
                                                                                                                                  propertyChangedCallback: (
                                                                                                                                                                   d,
                                                                                                                                                                   e) =>
                                                                                                                                                               ((BackstageDocumentPreviewReportService)
                                                                                                                                                                   d)
                                                                                                                                                              .OnBackstageDocumentPreviewChanged(
                                                                                                                                                                                                 e: e)));

    /// <summary>
    /// Gets or sets a value indicating whether the ribbon backstage view is open.
    /// </summary>
    public bool BackstageViewIsOpen
    {
        get => (bool)GetValue(dp: BackstageViewIsOpenProperty);
        set => SetValue(dp: BackstageViewIsOpenProperty, value: value);
    }

    /// <summary>
    /// Gets or sets the target backstage item base control.
    /// </summary>
    public BackstageItemBase BackstageItem
    {
        get => (BackstageItemBase)GetValue(dp: BackstageItemProperty);
        set => SetValue(dp: BackstageItemProperty, value: value);
    }

    /// <summary>
    /// Gets or sets the associated <see cref="Controls.BackstageDocumentPreview"/> control.
    /// </summary>
    public BackstageDocumentPreview BackstageDocumentPreview
    {
        get => (BackstageDocumentPreview)GetValue(dp: BackstageDocumentPreviewProperty);
        set => SetValue(dp: BackstageDocumentPreviewProperty, value: value);
    }

    /// <summary>
    /// Indicates whether the document is performing its initial load.
    /// </summary>
    private bool isDocumentFirstLoading = true;

    /// <inheritdoc />
    protected override void ShowReport(IReportInfo reportInfo)
    {
        base.ShowReport(reportInfo: reportInfo);
        _ = Dispatcher.BeginInvoke(method: (Action)OpenBackstageView);
    }

    /// <summary>
    /// Opens the Ribbon Backstage view and selects the active report preview tab.
    /// </summary>
    private void OpenBackstageView()
    {
        ((BackstageTabItem)BackstageItem).Backstage.SelectedTab = (BackstageTabItem)BackstageItem;
        BackstageItem.Backstage.IsOpen                          = true;
    }

    /// <inheritdoc />
    protected override void UpdateReportCore(IReportInfo? actualReportInfo)
    {
        base.UpdateReportCore(actualReportInfo: actualReportInfo);

        if (BackstageItem != null)
        {
            BackstageItem.IsEnabled = actualReportInfo != null;
        }
    }

    /// <summary>
    /// Synchronizes visibility with the Backstage view open state.
    /// </summary>
    private void OnBackstageViewIsOpenChanged() => IsVisible = BackstageViewIsOpen;

    /// <summary>
    /// Synchronizes enablement state between the old and new backstage items.
    /// </summary>
    /// <param name="oldItem">The old backstage item.</param>
    /// <param name="newItem">The new backstage item.</param>
    private void OnBackstageItemChanged(BackstageItemBase oldItem, BackstageItemBase newItem)
    {
        if (newItem != null)
        {
            newItem.IsEnabled = oldItem != null && oldItem.IsEnabled;
        }
    }

    /// <summary>
    /// Handles changes to the BackstageDocumentPreview dependency property and hooks up the Loaded event.
    /// </summary>
    /// <param name="e">The dependency property change event data.</param>
    private void OnBackstageDocumentPreviewChanged(DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue != null)
        {
            ((BackstageDocumentPreview)e.OldValue).Loaded -= BackstageDocumentPreview_Loaded;
        }

        if (e.NewValue != null)
        {
            ((BackstageDocumentPreview)e.NewValue).Loaded += BackstageDocumentPreview_Loaded;
        }
    }

    /// <summary>
    /// Handles the Loaded event of BackstageDocumentPreview to trigger initial report rendering.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event arguments.</param>
    private void BackstageDocumentPreview_Loaded(object sender, RoutedEventArgs e)
    {
        var updateReportAction = () => CreateReport();
        var document           = (BackstageDocumentPreview)sender;

        if (document.IsVisible && isDocumentFirstLoading)
        {
            _                      = Dispatcher.BeginInvoke(method: updateReportAction);
            isDocumentFirstLoading = false;
        }
    }

    /// <inheritdoc />
    protected override void SetDocumentSource(IReport report) => BackstageDocumentPreview.DocumentSource = report;

    /// <inheritdoc />
    protected override void SetCustomSettingsViewModel(object? customSettingsViewModel) =>
        BackstageDocumentPreview.CustomSettings = customSettingsViewModel!;

}
