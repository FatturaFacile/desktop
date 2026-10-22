using DevExpress.Xpf.DemoBase.Helpers;

namespace AIDemo {
    public partial class RichEditModule {
        public RichEditModule() {
            InitializeComponent();

            WarningText = Properties.Resource.EndpointWarning;

            var assembly = typeof(RichEditModule).Assembly;
            var documentStream = assembly.GetManifestResourceStream(
                DemoHelper.GetPath($"{assembly.GetName().Name}.Data.", assembly) + "FirstLook.docx");
            mainRichEditControl.LoadDocument(documentStream);
        }
    }
}
