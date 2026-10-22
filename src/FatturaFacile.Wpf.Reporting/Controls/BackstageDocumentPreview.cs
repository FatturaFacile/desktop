// <copyright file="BackstageDocumentPreview.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;

using DevExpress.Printing;
using DevExpress.Printing.Native.PrintEditor;
using DevExpress.Xpf.Printing.PreviewControl.Native;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports;

namespace FatturaFacile.Controls;

/// <summary>
/// Control providing print and export backstage preview capabilities for reports.
/// </summary>
public class BackstageDocumentPreview : System.Windows.Controls.Control
{
    static BackstageDocumentPreview()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(BackstageDocumentPreview), new FrameworkPropertyMetadata(typeof(BackstageDocumentPreview)));
    }

    internal static readonly DependencyPropertyKey IsPrintersAvailablePropertyKey =
        DependencyProperty.RegisterReadOnly(
            "IsPrintersAvailable", typeof(bool), typeof(BackstageDocumentPreview), new FrameworkPropertyMetadata(true));

    /// <summary>
    /// Identifies the <see cref="IsPrintersAvailable"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsPrintersAvailableProperty =
        IsPrintersAvailablePropertyKey.DependencyProperty;

    /// <summary>
    /// Gets a value indicating whether local or network printers are available.
    /// </summary>
    public bool IsPrintersAvailable
    {
        get => (bool)GetValue(IsPrintersAvailableProperty);
        protected set => SetValue(IsPrintersAvailablePropertyKey, value);
    }

    /// <summary>
    /// Identifies the <see cref="DocumentSource"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DocumentSourceProperty =
        DependencyProperty.Register(
            "DocumentSource", typeof(IReport), typeof(BackstageDocumentPreview), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the report document source displayed in the preview.
    /// </summary>
    public IReport DocumentSource
    {
        get => (IReport)GetValue(DocumentSourceProperty);
        set => SetValue(DocumentSourceProperty, value);
    }

    internal static readonly DependencyPropertyKey PrintersErrorMessagePropertyKey =
        DependencyProperty.RegisterReadOnly(
            "PrintersErrorMessage", typeof(string), typeof(BackstageDocumentPreview),
            new FrameworkPropertyMetadata(string.Empty));

    /// <summary>
    /// Identifies the <see cref="PrintersErrorMessage"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PrintersErrorMessageProperty =
        PrintersErrorMessagePropertyKey.DependencyProperty;

    /// <summary>
    /// Gets the error message encountered during printer discovery, if any.
    /// </summary>
    public string PrintersErrorMessage
    {
        get => (string)GetValue(PrintersErrorMessageProperty);
        protected set => SetValue(PrintersErrorMessagePropertyKey, value);
    }

    /// <summary>
    /// Gets or sets the currently selected printer.
    /// </summary>
    public PrinterViewModel SelectedPrinter
    {
        get => (PrinterViewModel)GetValue(SelectedPrinterProperty);
        set => SetValue(SelectedPrinterProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="SelectedPrinter"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SelectedPrinterProperty =
        DependencyProperty.Register(
            "SelectedPrinter", typeof(PrinterViewModel), typeof(BackstageDocumentPreview), new PropertyMetadata(null));

    /// <summary>
    /// Gets the list of detected printers.
    /// </summary>
    public List<PrinterViewModel> Printers
    {
        get => (List<PrinterViewModel>)GetValue(PrintersProperty);
        protected set => SetValue(PrintersPropertyKey, value);
    }

    private static readonly DependencyPropertyKey PrintersPropertyKey =
        DependencyProperty.RegisterReadOnly(
            "Printers", typeof(List<PrinterViewModel>), typeof(BackstageDocumentPreview), new PropertyMetadata(null));

    /// <summary>
    /// Identifies the <see cref="Printers"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PrintersProperty = PrintersPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets or sets the active backstage options mode (printing or export).
    /// </summary>
    public BackstagePreviewOptionsMode OptionsMode
    {
        get => (BackstagePreviewOptionsMode)GetValue(OptionsModeProperty);
        set => SetValue(OptionsModeProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="OptionsMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty OptionsModeProperty =
        DependencyProperty.Register(
            "OptionsMode", typeof(BackstagePreviewOptionsMode), typeof(BackstageDocumentPreview),
            new PropertyMetadata(
                BackstagePreviewOptionsMode.Printing,
                (d, e) => ((BackstageDocumentPreview)d).IsPrintOptionsVisible =
                    ((BackstageDocumentPreview)d).OptionsMode == BackstagePreviewOptionsMode.Printing));

    /// <summary>
    /// Gets a value indicating whether print options are currently visible.
    /// </summary>
    public bool IsPrintOptionsVisible
    {
        get => (bool)GetValue(IsPrintOptionsVisibleProperty);
        protected set => SetValue(IsPrintOptionsVisiblePropertyKey, value);
    }

    private static readonly DependencyPropertyKey IsPrintOptionsVisiblePropertyKey =
        DependencyProperty.RegisterReadOnly(
            "IsPrintOptionsVisible", typeof(bool), typeof(BackstageDocumentPreview), new PropertyMetadata(true));

    /// <summary>
    /// Identifies the <see cref="IsPrintOptionsVisible"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsPrintOptionsVisibleProperty =
        IsPrintOptionsVisiblePropertyKey.DependencyProperty;

    /// <summary>
    /// Gets or sets the selected export format.
    /// </summary>
    public string SelectedFormat
    {
        get => (string)GetValue(SelectedFormatProperty);
        set => SetValue(SelectedFormatProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="SelectedFormat"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SelectedFormatProperty =
        DependencyProperty.Register(
            "SelectedFormat", typeof(string), typeof(BackstageDocumentPreview), new PropertyMetadata(null));

    /// <summary>
    /// Gets the available export formats.
    /// </summary>
    public IEnumerable<string> ExportFormats
    {
        get => (IEnumerable<string>)GetValue(ExportFormatsProperty);
        protected set => SetValue(ExportFormatsPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ExportFormatsPropertyKey =
        DependencyProperty.RegisterReadOnly(
            "ExportFormats", typeof(IEnumerable<string>), typeof(BackstageDocumentPreview), new PropertyMetadata(null));

    /// <summary>
    /// Identifies the <see cref="ExportFormats"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ExportFormatsProperty = ExportFormatsPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets or sets the custom settings header object.
    /// </summary>
    public object CustomSettingsHeader
    {
        get => GetValue(CustomSettingsHeaderProperty);
        set => SetValue(CustomSettingsHeaderProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="CustomSettingsHeader"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CustomSettingsHeaderProperty =
        DependencyProperty.Register(
            "CustomSettingsHeader", typeof(object), typeof(BackstageDocumentPreview), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the custom settings view or viewmodel.
    /// </summary>
    public object CustomSettings
    {
        get => GetValue(CustomSettingsProperty);
        set => SetValue(CustomSettingsProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="CustomSettings"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CustomSettingsProperty =
        DependencyProperty.Register(
            "CustomSettings", typeof(object), typeof(BackstageDocumentPreview), new PropertyMetadata(null));

    /// <summary>
    /// Initializes a new instance of the <see cref="BackstageDocumentPreview"/> class.
    /// </summary>
    public BackstageDocumentPreview()
    {
        try
        {
            Printers = new List<PrinterViewModel>();
            LoadPrinters();
        }
        catch (Exception ex)
        {
            IsPrintersAvailable = false;
            PrintersErrorMessage = ex.Message;
        }

        ExportFormats = Enum.GetNames(typeof(ExportFormat)).Except(new string[] { "Prnx" });
        SelectedFormat = ExportFormats.FirstOrDefault()!;
    }

    private void LoadPrinters()
    {
        var imageConverter = new PrinterTypeToImageUriConverter();
        var printContainer = new PrinterItemContainer();
        foreach (var item in printContainer.Items)
        {
            var model = new PrinterViewModel()
            {
                IsOffline = item.PrinterType.HasFlag(PrinterType.Offline),
                DisplayName = item.DisplayName,
                Name = item.FullName,
                Status = item.Status,
                ImageUri = imageConverter.Convert(item.PrinterType, null!, null!, null!)?.ToString() ?? string.Empty
            };
            Printers.Add(model);
            if (item.PrinterType.HasFlag(PrinterType.Default) && !model.IsOffline)
            {
                SelectedPrinter = model;
            }
        }

        if (SelectedPrinter == null)
        {
            SelectedPrinter = Printers.FirstOrDefault(x => !x.IsOffline)!;
        }
    }
}

/// <summary>
/// ViewModel representing an individual printer and its operational status.
/// </summary>
public class PrinterViewModel
{
    /// <summary>
    /// Gets or sets the full printer name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user-friendly display name of the printer.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URI of the printer icon image.
    /// </summary>
    public string ImageUri { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current printer status description.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the printer is offline.
    /// </summary>
    public bool IsOffline { get; set; }
}

/// <summary>
/// Defines display mode options for the backstage preview control.
/// </summary>
public enum BackstagePreviewOptionsMode
{
    /// <summary>
    /// Displays print setup options.
    /// </summary>
    Printing,

    /// <summary>
    /// Displays file export options.
    /// </summary>
    Export,
}
