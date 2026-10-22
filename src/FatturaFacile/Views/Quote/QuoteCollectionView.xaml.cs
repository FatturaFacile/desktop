using System.Windows;
using System.Windows.Controls;
using DevExpress.Xpf.PivotGrid;
#if NET
using DevExpress.AIIntegration.Wpf;
using DevExpress.DevAV.AiTools;
using DevExpress.Mvvm.UI.Interactivity;
using System.Linq;
#endif

namespace DevExpress.DevAV.Views {
    public partial class QuoteCollectionView : UserControl {
        public QuoteCollectionView() {
            InitializeComponent();
#if NET
            AttachAIBehavior();
#endif
        }
#if NET
        void AttachAIBehavior() {
            if(pivotGrid == null) return;

            var behaviors = Interaction.GetBehaviors(pivotGrid);

            if(!behaviors.OfType<AIToolsBehavior>().Any()) {
                behaviors.Add(new AIToolsBehavior {
                    ToolType = typeof(GridControlAITools),
                    TargetDescription = "Grid control displaying quotes data",
                    ExtensionsContainer = App.ToolChatContainerDefault,
                });
            }
        }
#endif

        void CustomSummary(object sender, PivotCustomSummaryEventArgs e) {
            switch(e.DataField.FieldName) {
                case "Percentage":
                    decimal quoteSummary = 0;
                    decimal opportunitySummary = 0;
                    foreach(PivotDrillDownDataRow row in e.CreateDrillDownDataSource()) {
                        quoteSummary += (decimal)row["Total"];
                        opportunitySummary += (decimal)row["MoneyOpportunity"];
                    }
                    if(quoteSummary != 0)
                        e.CustomValue = 100M * opportunitySummary / quoteSummary;
                    break;
            }
        }
    }
}
