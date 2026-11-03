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
using FatturaFacile.Wpf.AI;

namespace FatturaFacile;

/// <summary>
/// Interaction logic and view module demonstrating AI-enhanced smart search capabilities within WPF window headers and ribbons.
/// </summary>
public partial class SmartSearchModule
{

    /// <summary>
    /// The parent themed window hosting the module.
    /// </summary>
    private ThemedWindow window;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartSearchModule"/> class.
    /// </summary>
    public SmartSearchModule()
    {
        InitializeComponent();
        WarningText = Resource.EndpointWarning;
        Loaded += TurnOnSearch;
        Unloaded += TurnOffSearch;
    }

    /// <summary>
    /// Enables AI smart search in the host window title area when the module loads.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event data.</param>
    private void TurnOnSearch(
        object sender,
        RoutedEventArgs e)
    {
        window = (ThemedWindow)Window.GetWindow(dependencyObject: this);

        if (window != null)
        {
            window.SearchItemDisplayMode = SearchItemDisplayMode.Right;
            window.SearchDelay = 300;
            Interaction.GetBehaviors(d: window).Add(value: new SmartSearchBehavior());
        }
    }

    /// <summary>
    /// Disables smart search behavior on the host window when navigating away from this module.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event data.</param>
    private void TurnOffSearch(
        object sender,
        RoutedEventArgs e)
    {
        window.SearchItemDisplayMode = SearchItemDisplayMode.Hidden;
        var behaviors = Interaction.GetBehaviors(d: window);
        var behavior = behaviors.OfType<SmartSearchBehavior>().FirstOrDefault();

        if (behavior != null)
        {
            _ = behaviors.Remove(value: behavior);
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

        var ribbon = LayoutTreeHelper.GetVisualChildren(parent: this)
                                     .OfType<RibbonControl>()
                                     .FirstOrDefault();

        if (ribbon == null)
        {
            return;
        }

        foreach (var page in ribbon.ActualCategories.SelectMany(selector: x => x.ActualPages))
        {
            page.SetCurrentValue(dp: RibbonPage.IsVisibleProperty, value: IsPageVisisble(pageName: page.Name));
        }

        foreach (var category in ribbon.ActualCategories.Where(predicate: ShouldHideCategory))
        {
            category.SetCurrentValue(dp: RibbonPageCategoryBase.IsVisibleProperty, value: false);
        }
    }

    /// <summary>
    /// Determines whether the specified ribbon category should be hidden in the demo.
    /// </summary>
    /// <param name="category">The ribbon page category to evaluate.</param>
    /// <returns><see langword="true"/> to hide; otherwise <c>false</c>.</returns>
    private static bool ShouldHideCategory(RibbonPageCategoryBase category)
    {
        return category.Name switch
        {
            DefaultBarItemNames.RibbonCategory_HeaderFooterTools or DefaultBarItemNames.RibbonCategory_TableTools or DefaultBarItemNames.RibbonCategory_PictureTools => true,
            _ => false,
        };
    }

    /// <summary>
    /// Determines whether the specified ribbon page should be visible in the demo.
    /// </summary>
    /// <param name="pageName">The internal name of the ribbon page.</param>
    /// <returns><see langword="true"/> if visible; otherwise <c>false</c>.</returns>
    private static bool IsPageVisisble(string pageName)
    {
        return pageName switch
        {
            DefaultBarItemNames.RibbonPage_MailMerge or DefaultBarItemNames.RibbonPage_File or DefaultBarItemNames.RibbonPage_Home or DefaultBarItemNames.RibbonPage_Insert or DefaultBarItemNames.RibbonPage_PageLayout or DefaultBarItemNames.RibbonPage_References or DefaultBarItemNames.RibbonPage_Review or DefaultBarItemNames.RibbonPage_View => true,
            _ => false,
        };
    }

}
