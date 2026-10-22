using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Printing;
using DevExpress.Printing.Native.PrintEditor;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Printing;
using DevExpress.Xpf.Printing.PreviewControl.Native;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports;

namespace DevExpress.DevAV.Controls {
    public class BackstageDocumentPreview : Control {
        static BackstageDocumentPreview() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BackstageDocumentPreview), new FrameworkPropertyMetadata(typeof(BackstageDocumentPreview)));
        }
        internal static readonly DependencyPropertyKey IsPrintersAvailablePropertyKey =
            DependencyProperty.RegisterReadOnly("IsPrintersAvailable", typeof(bool), typeof(BackstageDocumentPreview), new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty IsPrintersAvailableProperty = IsPrintersAvailablePropertyKey.DependencyProperty;
        public bool IsPrintersAvailable {
            get { return (bool)GetValue(IsPrintersAvailableProperty); }
            protected set { SetValue(IsPrintersAvailablePropertyKey, value); }
        }
        public static readonly DependencyProperty DocumentSourceProperty =
            DependencyProperty.Register("DocumentSource", typeof(IReport), typeof(BackstageDocumentPreview), new PropertyMetadata(null));
        public IReport DocumentSource { get { return (IReport)GetValue(DocumentSourceProperty); } set { SetValue(DocumentSourceProperty, value); } }

        internal static readonly DependencyPropertyKey PrintersErrorMessagePropertyKey =
            DependencyProperty.RegisterReadOnly("PrintersErrorMessage", typeof(string), typeof(BackstageDocumentPreview), new FrameworkPropertyMetadata(string.Empty));
        public static readonly DependencyProperty PrintersErrorMessageProperty = PrintersErrorMessagePropertyKey.DependencyProperty;
        public string PrintersErrorMessage {
            get { return (string)GetValue(PrintersErrorMessageProperty); }
            protected set { SetValue(PrintersErrorMessagePropertyKey, value); }
        }

        public PrinterViewModel SelectedPrinter {
            get { return (PrinterViewModel)GetValue(SelectedPrinterProperty); }
            set { SetValue(SelectedPrinterProperty, value); }
        }
        public static readonly DependencyProperty SelectedPrinterProperty =
            DependencyProperty.Register("SelectedPrinter", typeof(PrinterViewModel), typeof(BackstageDocumentPreview), new PropertyMetadata(null));

        public List<PrinterViewModel> Printers {
            get { return (List<PrinterViewModel>)GetValue(PrintersProperty); }
            protected set { SetValue(PrintersPropertyKey, value); }
        }
        static readonly DependencyPropertyKey PrintersPropertyKey =
            DependencyProperty.RegisterReadOnly("Printers", typeof(List<PrinterViewModel>), typeof(BackstageDocumentPreview), new PropertyMetadata(null));
        public static readonly DependencyProperty PrintersProperty = PrintersPropertyKey.DependencyProperty;

        public BackstagePreviewOptionsMode OptionsMode {
            get { return (BackstagePreviewOptionsMode)GetValue(OptionsModeProperty); }
            set { SetValue(OptionsModeProperty, value); }
        }
        public static readonly DependencyProperty OptionsModeProperty =
            DependencyProperty.Register("OptionsMode", typeof(BackstagePreviewOptionsMode), typeof(BackstageDocumentPreview), new PropertyMetadata(BackstagePreviewOptionsMode.Printing, (d, e) => ((BackstageDocumentPreview)d).IsPrintOptionsVisible = ((BackstageDocumentPreview)d).OptionsMode == BackstagePreviewOptionsMode.Printing));

        public bool IsPrintOptionsVisible {
            get { return (bool)GetValue(IsPrintOptionsVisibleProperty); }
            protected set { SetValue(IsPrintOptionsVisiblePropertyKey, value); }
        }
        static readonly DependencyPropertyKey IsPrintOptionsVisiblePropertyKey =
            DependencyProperty.RegisterReadOnly("IsPrintOptionsVisible", typeof(bool), typeof(BackstageDocumentPreview), new PropertyMetadata(true));
        public static readonly DependencyProperty IsPrintOptionsVisibleProperty = IsPrintOptionsVisiblePropertyKey.DependencyProperty;

        public string SelectedFormat {
            get { return (string)GetValue(SelectedFormatProperty); }
            set { SetValue(SelectedFormatProperty, value); }
        }
        public static readonly DependencyProperty SelectedFormatProperty =
            DependencyProperty.Register("SelectedFormat", typeof(string), typeof(BackstageDocumentPreview), new PropertyMetadata(null));

        public IEnumerable<string> ExportFormats {
            get { return (IEnumerable<string>)GetValue(ExportFormatsProperty); }
            protected set { SetValue(ExportFormatsPropertyKey, value); }
        }
        static readonly DependencyPropertyKey ExportFormatsPropertyKey =
            DependencyProperty.RegisterReadOnly("ExportFormats", typeof(IEnumerable<string>), typeof(BackstageDocumentPreview), new PropertyMetadata(null));
        public static readonly DependencyProperty ExportFormatsProperty = ExportFormatsPropertyKey.DependencyProperty;

        public object CustomSettingsHeader {
            get { return GetValue(CustomSettingsHeaderProperty); }
            set { SetValue(CustomSettingsHeaderProperty, value); }
        }
        public static readonly DependencyProperty CustomSettingsHeaderProperty =
            DependencyProperty.Register("CustomSettingsHeader", typeof(object), typeof(BackstageDocumentPreview), new PropertyMetadata(null));

        public object CustomSettings {
            get { return GetValue(CustomSettingsProperty); }
            set { SetValue(CustomSettingsProperty, value); }
        }
        public static readonly DependencyProperty CustomSettingsProperty =
            DependencyProperty.Register("CustomSettings", typeof(object), typeof(BackstageDocumentPreview), new PropertyMetadata(null));

        public BackstageDocumentPreview() {
            try {
                Printers = new List<PrinterViewModel>();
                LoadPrinters();
            }
            catch(Exception ex) {
                IsPrintersAvailable = false;
                PrintersErrorMessage = ex.Message;
            }
            
            ExportFormats = Enum.GetNames(typeof(ExportFormat)).Except(new string[] { "Prnx" });
            SelectedFormat = ExportFormats.FirstOrDefault();
        }

        void LoadPrinters() {
            var imageConverter = new PrinterTypeToImageUriConverter();
            var printContainer = new PrinterItemContainer();
            foreach(var item in printContainer.Items) {
                var model = new PrinterViewModel() {
                    IsOffline = item.PrinterType.HasFlag(PrinterType.Offline),
                    DisplayName = item.DisplayName,
                    Name = item.FullName,
                    Status = item.Status,
                    ImageUri = imageConverter.Convert(item.PrinterType, null, null, null).ToString()
                };
                Printers.Add(model);
                if(item.PrinterType.HasFlag(PrinterType.Default) && !model.IsOffline)
                    SelectedPrinter = model;
            }
            if(SelectedPrinter == null)
                SelectedPrinter = Printers.FirstOrDefault(x => !x.IsOffline);
        }
    }

    public class PrinterViewModel {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string ImageUri { get; set; }
        public string Status { get; set; }
        public bool IsOffline { get; set; }
    }

    public enum BackstagePreviewOptionsMode {
        Printing,
        Export,
    }
}
