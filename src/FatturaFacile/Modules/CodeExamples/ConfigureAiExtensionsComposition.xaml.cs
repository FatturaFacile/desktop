using System.Windows;
using System.Windows.Controls;
using DevExpress.AIIntegration;
using DevExpress.AIIntegration.Extensions;

namespace AIDemo.Modules.CodeExamples {
    public partial class ConfigureAiExtensionsComposition : UserControl {
        AIExtensionsContainerLocal localContainer = null;
        public ConfigureAiExtensionsComposition() {
            InitializeComponent();
        }

        void OnLoaded(object sender, RoutedEventArgs e) {
            localContainer = new AIExtensionsContainerLocal(AIExtensionsContainerDesktop.Default);
            localContainer.Register<ProofreadRequest, ErnestHemingwayStyleExtension>();

            AIExtensionsContainerDesktop.QueryIAIExtensionsContainer += QueryAiExtensionsContainerHandler;
        }

        void OnUnloaded(object sender, RoutedEventArgs e) {
            AIExtensionsContainerDesktop.QueryIAIExtensionsContainer -= QueryAiExtensionsContainerHandler;
            localContainer = null;
        }

        void QueryAiExtensionsContainerHandler(object sender, AIExtensionsContainerQueryEventArgs e) {
            if(e.Source == textEdit)
                e.Container = localContainer;
        }
    }
}
