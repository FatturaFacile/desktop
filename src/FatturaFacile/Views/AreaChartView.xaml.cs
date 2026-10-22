// <copyright file="AreaChartView.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FatturaFacile.Views;
/// <summary>
/// Interaction logic for AreaSeriesDateSelectionView.xaml
/// </summary>
public partial class AreaChartView : UserControl
{
    public AreaChartView()
    {
        InitializeComponent();
    }

    private void chartControl_BoundDataChanged(object sender, RoutedEventArgs e)
    {
        diagram.Series.First().CheckedInLegend = true;
    }
}
