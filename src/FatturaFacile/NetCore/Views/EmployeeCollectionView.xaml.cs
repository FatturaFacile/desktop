using DevExpress.Mvvm.UI.Interactivity;
using System.Windows;
using System.Windows.Controls;
using DevExpress.DevAV.ViewModels;
using DevExpress.Xpf.Grid;
#if NET
using DevExpress.AIIntegration.Wpf;
using DevExpress.DevAV.AiTools;
using System.Linq;
#endif
namespace DevExpress.DevAV.Views {
    public partial class EmployeeCollectionView : UserControl {
        public EmployeeCollectionView() {
            InitializeComponent();
#if NET
            AttachAiBehavior();
#endif
        }
#if NET
        void AttachAiBehavior() {
            if(tableViewGridControl == null) return;

            var behaviors = Interaction.GetBehaviors(tableViewGridControl);

            if(!behaviors.OfType<AIToolsBehavior>().Any()) {
                behaviors.Add(new AIToolsBehavior {
                    ToolType = typeof(GridControlAITools),
                    TargetDescription = "Grid control displaying employee data",
                    ExtensionsContainer = App.ToolChatContainerDefault,
                });
            }
        }
#endif
    }
}
