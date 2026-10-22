using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DevExpress.AIIntegration.Wpf;
using DevExpress.AIIntegration.Wpf.SmartSearch;
using DevExpress.Mvvm.UI;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.DemoBase.DemoTesting;
using DevExpress.Xpf.Ribbon;
using DevExpress.Xpf.RichEdit;

namespace AIDemo {
    public partial class SmartSearchModule {
        ThemedWindow window;

        public SmartSearchModule() {
            InitializeComponent();
            WarningText = Properties.Resource.EndpointWarning;
            Loaded += TurnOnSearch;
            Unloaded += TurnOffSearch;
        }
        void TurnOnSearch(object sender, RoutedEventArgs e) {
            window = (ThemedWindow)Window.GetWindow(this);

            if(window != null) {
                window.SearchItemDisplayMode = SearchItemDisplayMode.Right;
                window.SearchDelay = 300;
                Interaction.GetBehaviors(window).Add(new SmartSearchBehavior());
            }
        }
        void TurnOffSearch(object sender, RoutedEventArgs e) {
            window.SearchItemDisplayMode = SearchItemDisplayMode.Hidden;
            var behaviors = Interaction.GetBehaviors(window);
            var behavior = behaviors.OfType<SmartSearchBehavior>().FirstOrDefault();
            if(behavior != null)
                behaviors.Remove(behavior);

            window = null;
        }

    }

    public class SmartSearchDemoRibbon : Control {
        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            var ribbon = LayoutTreeHelper.GetVisualChildren(this)
                                          .OfType<RibbonControl>()
                                          .FirstOrDefault();
            if(ribbon == null)
                return;

            foreach(var page in ribbon.ActualCategories.SelectMany(x => x.ActualPages))
                page.SetCurrentValue(RibbonPage.IsVisibleProperty, IsPageVisisble(page.Name));

            foreach(var category in ribbon.ActualCategories.Where(ShouldHideCategory))
                category.SetCurrentValue(RibbonPageCategory.IsVisibleProperty, false);

        }
        bool ShouldHideCategory(RibbonPageCategoryBase category) {
            switch(category.Name) {
                case DefaultBarItemNames.RibbonCategory_HeaderFooterTools:
                case DefaultBarItemNames.RibbonCategory_TableTools:
                case DefaultBarItemNames.RibbonCategory_PictureTools:
                    return true;
                default:
                    return false;
            }
        }
        bool IsPageVisisble(string pageName) {
            switch(pageName) {
                case DefaultBarItemNames.RibbonPage_MailMerge:
                case DefaultBarItemNames.RibbonPage_File:
                case DefaultBarItemNames.RibbonPage_Home:
                case DefaultBarItemNames.RibbonPage_Insert:
                case DefaultBarItemNames.RibbonPage_PageLayout:
                case DefaultBarItemNames.RibbonPage_References:
                case DefaultBarItemNames.RibbonPage_Review:
                case DefaultBarItemNames.RibbonPage_View:
                    return true;
                default:
                    return false;
            }
        }
    }
}
