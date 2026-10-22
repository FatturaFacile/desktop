using DevExpress.Xpf.DemoBase.Helpers;

namespace AIDemo {
    public partial class ReportViewerModule {
        public ReportViewerModule() {
            InitializeComponent();

            WarningText = Properties.Resource.EndpointWarning;

            var assembly = typeof(ReportViewerModule).Assembly;
            var documentStream = assembly.GetManifestResourceStream(
                DemoHelper.GetPath($"{assembly.GetName().Name}.Data.", assembly) + "TableReport.prnx");
            documentPreview.DocumentSource = documentStream;
        }
    }
}
