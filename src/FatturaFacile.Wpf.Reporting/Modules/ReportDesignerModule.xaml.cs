// <copyright file="ReportDesignerModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

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
using FatturaFacile.Wpf.Reporting;

namespace FatturaFacile;

/// <summary>
/// Interaction logic and feature showcase for the DevExpress End-User Report Designer module.
/// </summary>
public partial class ReportDesignerModule
{

    /// <summary>
    /// Gets the list of featured capabilities displayed on the report designer start screen.
    /// </summary>
    public List<StartScreenItem> FeatureItems { get; }

    /// <summary>
    /// Indicates whether the interactive feature guide adorner has already been shown.
    /// </summary>
    private bool isGuideShown = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportDesignerModule"/> class.
    /// </summary>
    public ReportDesignerModule()
    {
        InitializeComponent();

        WarningText  = Resource.EndpointWarning;
        FeatureItems = CreateStartScreenItems();
    }

    /// <summary>
    /// Creates a start screen feature card item with thumbnails, guide images, and documentation hyperlinks.
    /// </summary>
    /// <param name="title">The feature title.</param>
    /// <param name="imageName">The base resource image name.</param>
    /// <param name="description">The short summary of the feature.</param>
    /// <param name="link">The external documentation URL.</param>
    /// <returns>A populated <see cref="StartScreenItem"/> instance.</returns>
    private StartScreenItem CreateStartScreenItem(string title, string imageName, string description, string link)
    {
        var thumbImage = new BitmapImage(uriSource: new Uri(uriString: $"pack://application:,,,/Resources/Reports/{imageName}Thumb.png"));
        var guideImage = new BitmapImage(uriSource: new Uri(uriString: $"pack://application:,,,/Resources/Reports/{imageName}Guide.png"));
        thumbImage.Freeze();
        guideImage.Freeze();

        var item = new StartScreenItem()
                   {
                       Title       = title,
                       ThumbImage  = thumbImage,
                       GuideImage  = guideImage,
                       Description = description,
                       Link        = link
                   };

        return item;
    }

    /// <summary>
    /// Builds the list of feature items showcased on the start screen.
    /// </summary>
    /// <returns>A list of initialized <see cref="StartScreenItem"/> instances.</returns>
    private List<StartScreenItem> CreateStartScreenItems()
    {
        var startScreenItems = new List<StartScreenItem>();

        var localizationItem = CreateStartScreenItem(
                                                     title: "Localization",
                                                     imageName: "AILocalization",
                                                     description: "Use AI-powered localization to translate report content to a different language with a single click.",
                                                     link: "https://docs.devexpress.com/XtraReports/405605/ai-powered-functionality/" +
                                                           "desktop-reporting/ai-powered-report-localization-wpf-report-designer?v=26.1");

        var wizardItem = CreateStartScreenItem(
                                               title: "Report Creation",
                                               imageName: "AIReportWizard",
                                               description: "Run the Report Wizard to generate a new report layout based on a natural language prompt.",
                                               link: "https://docs.devexpress.com/XtraReports/405625/ai-powered-functionality/desktop-reporting/prompt-to-report-in-wpf-report-designer?v=26.1");

        var testDataSourceItem = CreateStartScreenItem(
                                                       title: "Test Data Preview",
                                                       imageName: "AITestData",
                                                       description: "Generate test data with AI to preview reports without connecting to a live data source.",
                                                       link: "https://docs.devexpress.com/XtraReports/405567/ai-powered-functionality/" +
                                                             "desktop-reporting/preview-reports-with-ai-generated-test-data-wpf-report-designer?v=26.1");

        var expressionItem = CreateStartScreenItem(
                                                   title: "Prompt-to-Expression",
                                                   imageName: "AIExpression",
                                                   description: "Generate a data binding expression or a filter condition based on a natural language prompt.",
                                                   link: "https://docs.devexpress.com/XtraReports/405777/ai-powered-functionality/" +
                                                         "desktop-reporting/generate-expressions-from-prompts-wpf-report-designer?v=26.1");

        startScreenItems.Add(item: wizardItem);
        startScreenItems.Add(item: localizationItem);
        startScreenItems.Add(item: testDataSourceItem);
        startScreenItems.Add(item: expressionItem);

        return startScreenItems;
    }

    /// <summary>
    /// Handles the Loaded event of the ReportDesigner to scroll to initial content and show the interactive guide.
    /// </summary>
    /// <param name="sender">The report designer instance.</param>
    /// <param name="args">The event arguments.</param>
    private void OnDesignerLoaded(object sender, RoutedEventArgs args)
    {
        SplashScreenManager.CloseAll();
        var designer = sender as ReportDesigner;

        if (designer == null)
        {
            return;
        }

        var scroll = TreeHelper.GetChild<XRScrollViewer>(root: designer, predicate: x => x.Name == "ScrollViewer");

        if (scroll == null)
        {
            return;
        }

        var topMargin     = designer.ActiveDocument.Report.Bands[kind: DevExpress.XtraReports.UI.BandKind.TopMargin];
        var topMarginItem = designer.ActiveDocument.ReportModel.Factory.GetModel(source: topMargin);
        var point         = new System.Windows.Point(x: topMarginItem.RenderSize.Width / 2, y: topMarginItem.Position.Y - 150);
        designer.ActiveDocument.Diagram.ScrollToPoint(diagramPoint: point, verticalAlignment: VerticalAlignment.Top);

        _ = System.Windows.Application.Current.Dispatcher.BeginInvoke(
                                                                      priority: DispatcherPriority.Loaded,
                                                                      method: new Action(() => ShowGuide(designer: designer)));
    }

    /// <summary>
    /// Displays the interactive guide adorner on the report designer.
    /// </summary>
    /// <param name="designer">The report designer to adorn.</param>
    private void ShowGuide(ReportDesigner designer)
    {
        var guideAdorner = new GuideAdorner(adornedElement: designer);

        if (isGuideShown)
        {
            return;
        }

        if (guideAdorner.IsRunning)
        {
            guideAdorner.UpdateGeometry();

            return;
        }

        var smartTagButton   = TreeHelper.GetChild<ToggleButton>(root: designer, predicate: x => x.Name == "SmartTagButton");
        var ribbonPageHeader = TreeHelper.GetChild<RibbonPageHeaderLinksControl>(root: designer);

        if (ribbonPageHeader.Items.Count < 2)
        {
            return;
        }

        var testDataSourceButton = ribbonPageHeader.Items[index: 2] as BarItemLinkInfo;

        var newReportButton = TreeHelper.GetChild<BarItemLinkInfo>(
                                                                   root: designer,
                                                                   predicate: x => x.LinkControl?.ActualContent?.ToString() ==
                                                                                   "New Report");

        var adornerLayer = AdornerLayer.GetAdornerLayer(visual: designer);
        adornerLayer?.Add(adorner: guideAdorner);

        var wizardItem = FeatureItems[index: 0];

        guideAdorner.AddStep(
                             step: new GuideAdorner.GuideStep
                                   {
                                       Target      = designer,
                                       FillTarget  = true,
                                       Image       = wizardItem.GuideImage,
                                       Title       = wizardItem.Title,
                                       Description = wizardItem.Description
                                   });

        var localizationItem = FeatureItems[index: 1];

        guideAdorner.AddStep(
                             step: new GuideAdorner.GuideStep
                                   {
                                       Target      = smartTagButton,
                                       Image       = localizationItem.GuideImage,
                                       Title       = localizationItem.Title,
                                       Description = localizationItem.Description
                                   });

        var testDataSourceItem = FeatureItems[index: 2];

        guideAdorner.AddStep(
                             step: new GuideAdorner.GuideStep
                                   {
                                       Target      = testDataSourceButton,
                                       Image       = testDataSourceItem.GuideImage,
                                       Title       = testDataSourceItem.Title,
                                       Description = testDataSourceItem.Description
                                   });

        var expressionItem = FeatureItems[index: 3];

        guideAdorner.AddStep(
                             step: new GuideAdorner.GuideStep
                                   {
                                       Target      = designer,
                                       FillTarget  = true,
                                       Image       = expressionItem.GuideImage,
                                       Title       = expressionItem.Title,
                                       Description = expressionItem.Description
                                   });

        guideAdorner.Start();

        EventHandler handler = null!;

        handler = (s, a) =>
                  {
                      guideAdorner.Closed -= handler;
                      adornerLayer?.Remove(adorner: guideAdorner);
                      isGuideShown = true;
                  };

        guideAdorner.Closed += handler;
    }

    /// <summary>
    /// Handles the click event to open a standalone maximized window hosting the Report Designer.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event data.</param>
    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        var designerWindow = new ThemedWindow()
                             {
                                 WindowState = WindowState.Maximized
                             };

        var designer = new ReportDesigner();
        designer.DocumentSource = new XtraReportsDemos.NorthwindTraders.InvoiceReport();

        designerWindow.Content = designer;

        var behaviors = Interaction.GetBehaviors(d: designer);

        behaviors.Add(
                      value: new ReportTestDataSourceBehavior()
                             {
                                 Temperature = 1
                             });

        behaviors.Add(value: new ReportLocalizationBehavior());

        behaviors.Add(
                      value: new ReportPromptToReportBehavior()
                             {
                                 Temperature = 1
                             });

        behaviors.Add(
                      value: new ReportPromptToExpressionBehavior()
                             {
                                 Temperature = 1
                             });

        designer.Loaded += OnDesignerLoaded;
        var waitIndicator = SplashScreenManager.CreateWaitIndicator();
        waitIndicator.Show(owner: this, startupLocation: WindowStartupLocation.CenterOwner);
        _               =  designerWindow.ShowDialog();
        designer.Loaded -= OnDesignerLoaded;
        isGuideShown    =  false;
    }

}

/// <summary>
/// Feature card item displayed on the report designer start screen.
/// </summary>
public class StartScreenItem
{

    /// <summary>
    /// Gets or sets the card title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the thumbnail image source.
    /// </summary>
    public ImageSource ThumbImage { get; set; } = null!;

    /// <summary>
    /// Gets or sets the guide screenshot image source.
    /// </summary>
    public ImageSource GuideImage { get; set; } = null!;

    /// <summary>
    /// Gets or sets the explanatory description of the feature.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the documentation web hyperlink.
    /// </summary>
    public string Link { get; set; } = string.Empty;

}
