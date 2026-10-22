using DevExpress.Xpf.DemoBase.Helpers;

namespace AIDemo {
    public partial class SpreadsheetModule {
        public SpreadsheetModule() {
            InitializeComponent();

            WarningText = Properties.Resource.EndpointWarning;

            var assembly = typeof(SpreadsheetModule).Assembly;
            var documentStream = assembly.GetManifestResourceStream(
                DemoHelper.GetPath($"{assembly.GetName().Name}.Data.", assembly) + "LoanAmortizationSchedule.xlsx");
            mainSpreadsheetControl.LoadDocument(documentStream);
        }
    }
}
