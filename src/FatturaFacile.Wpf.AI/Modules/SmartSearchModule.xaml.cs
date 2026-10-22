// <copyright file="SmartSearchModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;

using DevExpress.AIIntegration.Wpf;
using DevExpress.Mvvm.UI;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Ribbon;
using DevExpress.Xpf.RichEdit;

namespace FatturaFacile;

/// <summary>
/// Interaction logic and view module demonstrating AI-enhanced smart search capabilities within WPF window headers and ribbons.
/// </summary>
public partial class SmartSearchModule
{
    private ThemedWindow window;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartSearchModule"/> class.
    /// </summary>
    public SmartSearchModule()
    {
        InitializeComponent();
        WarningText = Properties.Resource.EndpointWarning;
        Loaded += TurnOnSearch;
        Unloaded += TurnOffSearch;
    }

    private void TurnOnSearch(
        object sender,
        RoutedEventArgs e)
    {
        window = (ThemedWindow)Window.GetWindow(this);

        if (window != null)
        {
            window.SearchItemDisplayMode = SearchItemDisplayMode.Right;
            window.SearchDelay = 300;
            Interaction.GetBehaviors(window).Add(new SmartSearchBehavior());
        }
    }

    private void TurnOffSearch(
        object sender,
        RoutedEventArgs e)
    {
        window.SearchItemDisplayMode = SearchItemDisplayMode.Hidden;
        var behaviors = Interaction.GetBehaviors(window);
        var behavior = behaviors.OfType<SmartSearchBehavior>().FirstOrDefault();
        if (behavior != null)
        {
            _ = behaviors.Remove(behavior);
        }

        window = null;
    }
}

/// <summary>
/// Demonstration ribbon control embedding DevExpress SmartSearch capabilities.
/// </summary>
public class SmartSearchDemoRibbon : System.Windows.Controls.Control
{
    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        var ribbon = LayoutTreeHelper.GetVisualChildren(this)
            .OfType<RibbonControl>()
            .FirstOrDefault();
        if (ribbon == null)
        {
            return;
        }

        foreach (var page in ribbon.ActualCategories.SelectMany(x => x.ActualPages))
        {
            page.SetCurrentValue(RibbonPage.IsVisibleProperty, IsPageVisisble(page.Name));
        }

        foreach (var category in ribbon.ActualCategories.Where(ShouldHideCategory))
        {
            category.SetCurrentValue(RibbonPageCategory.IsVisibleProperty, false);
        }
    }

    private bool ShouldHideCategory(
        RibbonPageCategoryBase category)
    {
        switch (category.Name)
        {
            case DefaultBarItemNames.RibbonCategory_HeaderFooterTools:
            case DefaultBarItemNames.RibbonCategory_TableTools:
            case DefaultBarItemNames.RibbonCategory_PictureTools:
                return true;
            default:
                return false;
        }
    }

    private bool IsPageVisisble(
        string pageName)
    {
        switch (pageName)
        {
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
