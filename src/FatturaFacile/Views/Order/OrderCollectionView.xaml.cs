using System;
using System.Windows;
using System.Windows.Controls;
using DevExpress.DevAV.ViewModels;
using DevExpress.Xpf.Grid;
#if NET
using DevExpress.AIIntegration.Wpf;
using DevExpress.DevAV.AiTools;
using DevExpress.Mvvm.UI.Interactivity;
using System.Linq;
#endif

namespace DevExpress.DevAV.Views {
    public partial class OrderCollectionView : UserControl {
        public OrderCollectionView() {
            InitializeComponent();
#if NET
            AttachAiBehavior();
#endif
        }
#if NET
        void AttachAiBehavior() {
            if(gridControl == null) return;

            var behaviors = Interaction.GetBehaviors(gridControl);

            if(!behaviors.OfType<AIToolsBehavior>().Any()) {
                behaviors.Add(new AIToolsBehavior {
                    ToolType = typeof(GridControlAITools),
                    TargetDescription = "Grid control displaying order data",
                    ExtensionsContainer = App.ToolChatContainerDefault,
                });
            }
        }
#endif
    }
}
