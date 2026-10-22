using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DevExpress.AIIntegration.Wpf.Reporting;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Bars.Native;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.DemoBase.Helpers;
using DevExpress.Xpf.Reports.UserDesigner;
using DevExpress.Xpf.Reports.UserDesigner.Native;
using DevExpress.Xpf.Ribbon;

namespace AIDemo {

    public partial class ReportDesignerModule {

        public List<StartScreenItem> FeatureItems { get; }
        bool isGuideShown = false;

        public ReportDesignerModule() {
            InitializeComponent();

            WarningText = Properties.Resource.EndpointWarning;
            FeatureItems = CreateStartScreenItems();
        }

        StartScreenItem CreateStartScreenItem(string title, string imageName, string description, string link) {
            var thumbImage = new BitmapImage(new Uri($"pack://application:,,,/Resources/Reports/{imageName}Thumb.png"));
            var guideImage = new BitmapImage(new Uri($"pack://application:,,,/Resources/Reports/{imageName}Guide.png"));
            thumbImage.Freeze();
            guideImage.Freeze();
            var item = new StartScreenItem() {
                Title = title,
                ThumbImage = thumbImage,
                GuideImage = guideImage,
                Description = description,
                Link = link
            };
            return item;
        }

        List<StartScreenItem> CreateStartScreenItems() {
            var startScreenItems = new List<StartScreenItem>();

            var localizationItem = CreateStartScreenItem("Localization", "AILocalization", "Use AI-powered localization to translate report content to a different language with a single click.", "https://docs.devexpress.com/XtraReports/405605/ai-powered-functionality/desktop-reporting/ai-powered-report-localization-wpf-report-designer?v=26.1");
            var wizardItem = CreateStartScreenItem("Report Creation", "AIReportWizard", "Run the Report Wizard to generate a new report layout based on a natural language prompt.", "https://docs.devexpress.com/XtraReports/405625/ai-powered-functionality/desktop-reporting/prompt-to-report-in-wpf-report-designer?v=26.1");
            var testDataSourceItem = CreateStartScreenItem("Test Data Preview", "AITestData", "Generate test data with AI to preview reports without connecting to a live data source.", "https://docs.devexpress.com/XtraReports/405567/ai-powered-functionality/desktop-reporting/preview-reports-with-ai-generated-test-data-wpf-report-designer?v=26.1");
            var expressionItem = CreateStartScreenItem("Prompt-to-Expression", "AIExpression", "Generate a data binding expression or a filter condition based on a natural language prompt.", "https://docs.devexpress.com/XtraReports/405777/ai-powered-functionality/desktop-reporting/generate-expressions-from-prompts-wpf-report-designer?v=26.1");

            startScreenItems.Add(wizardItem);
            startScreenItems.Add(localizationItem);
            startScreenItems.Add(testDataSourceItem);
            startScreenItems.Add(expressionItem);

            return startScreenItems;
        }

        void OnDesignerLoaded(object sender, RoutedEventArgs args) {
            SplashScreenManager.CloseAll();
            ReportDesigner designer = sender as ReportDesigner;
            if(designer == null)
                return;

            XRScrollViewer scroll = TreeHelper.GetChild<XRScrollViewer>(designer, x => x.Name == "ScrollViewer");
            if(scroll == null)
                return;
            var topMargin = designer.ActiveDocument.Report.Bands[DevExpress.XtraReports.UI.BandKind.TopMargin];
            var topMarginItem = designer.ActiveDocument.ReportModel.Factory.GetModel(topMargin);
            var point = new Point(topMarginItem.RenderSize.Width / 2, topMarginItem.Position.Y - 150);
            designer.ActiveDocument.Diagram.ScrollToPoint(point, verticalAlignment: VerticalAlignment.Top);
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => ShowGuide(designer)));
        }

        void ShowGuide(ReportDesigner designer) {
            var guideAdorner = new GuideAdorner(designer);
            if(isGuideShown)
                return;

            if(guideAdorner.IsRunning) {
                guideAdorner.UpdateGeometry();
                return;
            }

            ToggleButton smartTagButton = TreeHelper.GetChild<ToggleButton>(designer, x => x.Name == "SmartTagButton");
            var ribbonPageHeader = TreeHelper.GetChild<RibbonPageHeaderLinksControl>(designer);
            if(ribbonPageHeader.Items.Count < 2)
                return;
            BarItemLinkInfo testDataSourceButton = ribbonPageHeader.Items[2] as BarItemLinkInfo;
            BarItemLinkInfo newReportButton = TreeHelper.GetChild<BarItemLinkInfo>(designer, x => x.LinkControl?.ActualContent?.ToString() == "New Report");

            var adornerLayer = AdornerLayer.GetAdornerLayer(designer);
            adornerLayer?.Add(guideAdorner);

            var wizardItem = FeatureItems[0];
            guideAdorner.AddStep(new GuideAdorner.GuideStep {
                Target = designer,
                FillTarget = true,
                Image = wizardItem.GuideImage,
                Title = wizardItem.Title,
                Description = wizardItem.Description
            });

            var localizationItem = FeatureItems[1];
            guideAdorner.AddStep(new GuideAdorner.GuideStep {
                Target = smartTagButton,
                Image = localizationItem.GuideImage,
                Title = localizationItem.Title,
                Description = localizationItem.Description
            });

            var testDataSourceItem = FeatureItems[2];
            guideAdorner.AddStep(new GuideAdorner.GuideStep {
                Target = testDataSourceButton,
                Image = testDataSourceItem.GuideImage,
                Title = testDataSourceItem.Title,
                Description = testDataSourceItem.Description
            });

            var expressionItem = FeatureItems[3];
            guideAdorner.AddStep(new GuideAdorner.GuideStep {
                Target = designer,
                FillTarget = true,
                Image = expressionItem.GuideImage,
                Title = expressionItem.Title,
                Description = expressionItem.Description
            });

            guideAdorner.Start();

            EventHandler handler = null;
            handler = (s, a) => {
                guideAdorner.Closed -= handler;
                adornerLayer?.Remove(guideAdorner);
                isGuideShown = true;
            };
            guideAdorner.Closed += handler;
        }

        void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
            ThemedWindow designerWindow = new ThemedWindow() { WindowState = WindowState.Maximized };
            ReportDesigner designer = new ReportDesigner();
            designer.DocumentSource = new XtraReportsDemos.NorthwindTraders.InvoiceReport();

            designerWindow.Content = designer;

            BehaviorCollection behaviors = Interaction.GetBehaviors(designer);
            behaviors.Add(new ReportTestDataSourceBehavior() { Temperature = 1 });
            behaviors.Add(new ReportLocalizationBehavior());
            behaviors.Add(new ReportPromptToReportBehavior() { Temperature = 1 });
            behaviors.Add(new ReportPromptToExpressionBehavior() { Temperature = 1 });

            designer.Loaded += OnDesignerLoaded;
            var waitIndicator = SplashScreenManager.CreateWaitIndicator();
            waitIndicator.Show(owner: this, startupLocation: WindowStartupLocation.CenterOwner);
            designerWindow.ShowDialog();
            designer.Loaded -= OnDesignerLoaded;
            isGuideShown = false;
        }
    }

    public class StartScreenItem {
        public string Title { get; set; }
        public ImageSource ThumbImage { get; set; }
        public ImageSource GuideImage { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
    }
}
