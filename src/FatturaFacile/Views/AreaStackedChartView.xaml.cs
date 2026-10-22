using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

using DevExpress.Xpf.Charts.RangeControlClient;

namespace FatturaFacile.Views;
/// <summary>
/// Interaction logic for StackedLineChartView.xaml
/// </summary>
public partial class AreaStackedChartView : UserControl
{
    public AreaStackedChartView()
    {
        InitializeComponent();
    }
}

public class DateTimeToChartValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return new ChartValue(value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return ((ChartValue)value).DataValue;
    }
}
