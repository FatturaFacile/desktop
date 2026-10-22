using System.Windows;
using DevExpress.AIIntegration;
using DevExpress.AIIntegration.Extensions;

namespace AIDemo.Modules.CodeExamples {
    public partial class ModifyDefaultExtension {
        public ModifyDefaultExtension() {
            InitializeComponent();
        }

        void OnLoaded(object sender, RoutedEventArgs e) {
            AIExtensionsContainerDesktop.Default.Register<ProofreadRequest, WilliamShakespeareStyleExtension>();
        }

        void OnUnloaded(object sender, RoutedEventArgs e) {
            AIExtensionsContainerDesktop.Default.Register<ProofreadRequest, ProofreadExtension>();
        }
    }
}
