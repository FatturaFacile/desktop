using DevExpress.Xpf.DemoBase.Helpers;

namespace AIDemo {
    public partial class PdfViewerModule {
        public PdfViewerModule() {
            InitializeComponent();

            WarningText = Properties.Resource.EndpointWarning;

            var assembly = typeof(ReportViewerModule).Assembly;
            var documentStream = assembly.GetManifestResourceStream(
                DemoHelper.GetPath($"{assembly.GetName().Name}.Data.", assembly) + "PageContent.pdf");
            pdfViewerControl.DocumentSource = documentStream;
        }
    }
}
