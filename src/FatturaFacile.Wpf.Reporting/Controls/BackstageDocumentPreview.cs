// <copyright file="BackstageDocumentPreview.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;

using DevExpress.Printing;
using DevExpress.Printing.Native.PrintEditor;
using DevExpress.Xpf.Printing.PreviewControl.Native;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports;

namespace FatturaFacile.Wpf.Reporting.Controls;

/// <summary>
/// Control providing print and export backstage preview capabilities for reports.
/// </summary>
public class BackstageDocumentPreview : System.Windows.Controls.Control
{

    static BackstageDocumentPreview()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
                                                 forType: typeof(BackstageDocumentPreview),
                                                 typeMetadata: new FrameworkPropertyMetadata(defaultValue: typeof(BackstageDocumentPreview)));
    }

    /// <summary>
    /// Key for the read-only <see cref="IsPrintersAvailable"/> dependency property.
    /// </summary>
    internal static readonly DependencyPropertyKey IsPrintersAvailablePropertyKey = DependencyProperty.RegisterReadOnly(
                                                                                                                        name: "IsPrintersAvailable",
                                                                                                                        propertyType: typeof
                                                                                                                            (bool),
                                                                                                                        ownerType: typeof
                                                                                                                            (BackstageDocumentPreview),
                                                                                                                        typeMetadata: new
                                                                                                                            FrameworkPropertyMetadata(
                                                                                                                                                      defaultValue: true));

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
        get => (bool)GetValue(dp: IsPrintersAvailableProperty);
        protected set => SetValue(key: IsPrintersAvailablePropertyKey, value: value);
    }

    /// <summary>
    /// Identifies the <see cref="DocumentSource"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DocumentSourceProperty = DependencyProperty.Register(
                                                                                                   name: "DocumentSource",
                                                                                                   propertyType: typeof(IReport),
                                                                                                   ownerType: typeof(
                                                                                                       BackstageDocumentPreview),
                                                                                                   typeMetadata: new PropertyMetadata(
                                                                                                                                      propertyChangedCallback: null));

    /// <summary>
    /// Gets or sets the report document source displayed in the preview.
    /// </summary>
    public IReport DocumentSource
    {
        get => (IReport)GetValue(dp: DocumentSourceProperty);
        set => SetValue(dp: DocumentSourceProperty, value: value);
    }

    /// <summary>
    /// Key for the read-only <see cref="PrintersErrorMessage"/> dependency property.
    /// </summary>
    internal static readonly DependencyPropertyKey PrintersErrorMessagePropertyKey =
        DependencyProperty.RegisterReadOnly(
                                            name: "PrintersErrorMessage",
                                            propertyType: typeof(string),
                                            ownerType: typeof(BackstageDocumentPreview),
                                            typeMetadata: new FrameworkPropertyMetadata(defaultValue: string.Empty));

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
        get => (string)GetValue(dp: PrintersErrorMessageProperty);
        protected set => SetValue(key: PrintersErrorMessagePropertyKey, value: value);
    }

    /// <summary>
    /// Gets or sets the currently selected printer.
    /// </summary>
    public PrinterViewModel SelectedPrinter
    {
        get => (PrinterViewModel)GetValue(dp: SelectedPrinterProperty);
        set => SetValue(dp: SelectedPrinterProperty, value: value);
    }

    /// <summary>
    /// Identifies the <see cref="SelectedPrinter"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SelectedPrinterProperty = DependencyProperty.Register(
                                                                                                    name: "SelectedPrinter",
                                                                                                    propertyType: typeof(
                                                                                                        PrinterViewModel),
                                                                                                    ownerType: typeof(
                                                                                                        BackstageDocumentPreview),
                                                                                                    typeMetadata: new
                                                                                                        PropertyMetadata(
                                                                                                                         propertyChangedCallback: null));

    /// <summary>
    /// Gets the list of detected printers.
    /// </summary>
    public List<PrinterViewModel> Printers
    {
        get => (List<PrinterViewModel>)GetValue(dp: PrintersProperty);
        protected set => SetValue(key: PrintersPropertyKey, value: value);
    }

    /// <summary>
    /// Identifies the read-only <see cref="Printers"/> dependency property key.
    /// </summary>
    private static readonly DependencyPropertyKey PrintersPropertyKey = DependencyProperty.RegisterReadOnly(
                                                                                                            name: "Printers",
                                                                                                            propertyType: typeof(List<
                                                                                                                PrinterViewModel>),
                                                                                                            ownerType: typeof(
                                                                                                                BackstageDocumentPreview),
                                                                                                            typeMetadata: new
                                                                                                                PropertyMetadata(
                                                                                                                                 propertyChangedCallback: null));

    /// <summary>
    /// Identifies the <see cref="Printers"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PrintersProperty = PrintersPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets or sets the active backstage options mode (printing or export).
    /// </summary>
    public BackstagePreviewOptionsMode OptionsMode
    {
        get => (BackstagePreviewOptionsMode)GetValue(dp: OptionsModeProperty);
        set => SetValue(dp: OptionsModeProperty, value: value);
    }

    /// <summary>
    /// Identifies the <see cref="OptionsMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty OptionsModeProperty = DependencyProperty.Register(
                                                                                                name: "OptionsMode",
                                                                                                propertyType: typeof(
                                                                                                    BackstagePreviewOptionsMode),
                                                                                                ownerType: typeof(
                                                                                                    BackstageDocumentPreview),
                                                                                                typeMetadata: new PropertyMetadata(
                                                                                                                                   defaultValue: BackstagePreviewOptionsMode
                                                                                                                                      .Printing,
                                                                                                                                   propertyChangedCallback: (
                                                                                                                                                                    d,
                                                                                                                                                                    e) =>
                                                                                                                                                                ((BackstageDocumentPreview)
                                                                                                                                                                    d)
                                                                                                                                                               .IsPrintOptionsVisible =
                                                                                                                                                                ((BackstageDocumentPreview)
                                                                                                                                                                    d)
                                                                                                                                                               .OptionsMode ==
                                                                                                                                                                BackstagePreviewOptionsMode
                                                                                                                                                                   .Printing));

    /// <summary>
    /// Gets a value indicating whether print options are currently visible.
    /// </summary>
    public bool IsPrintOptionsVisible
    {
        get => (bool)GetValue(dp: IsPrintOptionsVisibleProperty);
        protected set => SetValue(key: IsPrintOptionsVisiblePropertyKey, value: value);
    }

    /// <summary>
    /// Identifies the read-only <see cref="IsPrintOptionsVisible"/> dependency property key.
    /// </summary>
    private static readonly DependencyPropertyKey IsPrintOptionsVisiblePropertyKey =
        DependencyProperty.RegisterReadOnly(
                                            name: "IsPrintOptionsVisible",
                                            propertyType: typeof(bool),
                                            ownerType: typeof(BackstageDocumentPreview),
                                            typeMetadata: new PropertyMetadata(defaultValue: true));

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
        get => (string)GetValue(dp: SelectedFormatProperty);
        set => SetValue(dp: SelectedFormatProperty, value: value);
    }

    /// <summary>
    /// Identifies the <see cref="SelectedFormat"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SelectedFormatProperty = DependencyProperty.Register(
                                                                                                   name: "SelectedFormat",
                                                                                                   propertyType: typeof(string),
                                                                                                   ownerType: typeof(
                                                                                                       BackstageDocumentPreview),
                                                                                                   typeMetadata: new PropertyMetadata(
                                                                                                                                      propertyChangedCallback: null));

    /// <summary>
    /// Gets the available export formats.
    /// </summary>
    public IEnumerable<string> ExportFormats
    {
        get => (IEnumerable<string>)GetValue(dp: ExportFormatsProperty);
        protected set => SetValue(key: ExportFormatsPropertyKey, value: value);
    }

    /// <summary>
    /// Identifies the read-only <see cref="ExportFormats"/> dependency property key.
    /// </summary>
    private static readonly DependencyPropertyKey ExportFormatsPropertyKey = DependencyProperty.RegisterReadOnly(
                                                                                                                 name: "ExportFormats",
                                                                                                                 propertyType: typeof(
                                                                                                                     IEnumerable
                                                                                                                     <string>),
                                                                                                                 ownerType: typeof(
                                                                                                                     BackstageDocumentPreview),
                                                                                                                 typeMetadata: new
                                                                                                                     PropertyMetadata(
                                                                                                                                      propertyChangedCallback: null));

    /// <summary>
    /// Identifies the <see cref="ExportFormats"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ExportFormatsProperty = ExportFormatsPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets or sets the custom settings header object.
    /// </summary>
    public object CustomSettingsHeader
    {
        get => GetValue(dp: CustomSettingsHeaderProperty);
        set => SetValue(dp: CustomSettingsHeaderProperty, value: value);
    }

    /// <summary>
    /// Identifies the <see cref="CustomSettingsHeader"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CustomSettingsHeaderProperty = DependencyProperty.Register(
                                                                                                         name: "CustomSettingsHeader",
                                                                                                         propertyType: typeof(object),
                                                                                                         ownerType: typeof(
                                                                                                             BackstageDocumentPreview),
                                                                                                         typeMetadata: new
                                                                                                             PropertyMetadata(
                                                                                                                              propertyChangedCallback: null));

    /// <summary>
    /// Gets or sets the custom settings view or viewmodel.
    /// </summary>
    public object CustomSettings
    {
        get => GetValue(dp: CustomSettingsProperty);
        set => SetValue(dp: CustomSettingsProperty, value: value);
    }

    /// <summary>
    /// Identifies the <see cref="CustomSettings"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CustomSettingsProperty = DependencyProperty.Register(
                                                                                                   name: "CustomSettings",
                                                                                                   propertyType: typeof(object),
                                                                                                   ownerType: typeof(
                                                                                                       BackstageDocumentPreview),
                                                                                                   typeMetadata: new PropertyMetadata(
                                                                                                                                      propertyChangedCallback: null));

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
            IsPrintersAvailable  = false;
            PrintersErrorMessage = ex.Message;
        }

        ExportFormats = Enum.GetNames(enumType: typeof(ExportFormat))
                            .Except(
                                    second: new string[]
                                            {
                                                "Prnx",
                                            });

        SelectedFormat = ExportFormats.FirstOrDefault()!;
    }

    /// <summary>
    /// Enumerates and loads installed system printers into the view model list.
    /// </summary>
    private void LoadPrinters()
    {
        var imageConverter = new PrinterTypeToImageUriConverter();
        var printContainer = new PrinterItemContainer();

        foreach (var item in printContainer.Items)
        {
            var model = new PrinterViewModel()
                        {
                            IsOffline   = item.PrinterType.HasFlag(flag: PrinterType.Offline),
                            DisplayName = item.DisplayName,
                            Name        = item.FullName,
                            Status      = item.Status,
                            ImageUri = imageConverter.Convert(value: item.PrinterType, targetType: null!, parameter: null!, culture: null!)?.ToString() ??
                                       string.Empty,
                        };

            Printers.Add(item: model);

            if (item.PrinterType.HasFlag(flag: PrinterType.Default) && !model.IsOffline)
            {
                SelectedPrinter = model;
            }
        }

        if (SelectedPrinter == null)
        {
            SelectedPrinter = Printers.FirstOrDefault(predicate: x => !x.IsOffline)!;
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
