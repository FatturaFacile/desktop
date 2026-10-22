// <copyright file="DiagramControlView.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace FatturaFacile.Views;
/// <summary>
/// Interaction logic for DiagramControlView.xaml
/// </summary>
public partial class DiagramControlView : UserControl
{
    public DiagramControlView()
    {
        InitializeComponent();
    }
}

public class GroupNameToColorConverter : IValueConverter
{
    public Brush ExecutiveBrush { get; set; }
    public Brush SalesBrush { get; set; }
    public Brush DesignBrush { get; set; }
    public Brush DefaultBrush { get; set; }
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string groupName)
        {
            switch (groupName)
            {
                case "Executive General and Administration":
                    return ExecutiveBrush;
                case "Sales":
                    return SalesBrush;
                case "Design":
                    return DesignBrush;
            }

        }
        return DefaultBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}
