using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using DevExpress.Mvvm.CodeGenerators;
using DevExpress.Mvvm.Native;

using FatturaFacile.Infrastructure;
using FatturaFacile.Model;

namespace FatturaFacile.ViewModels;
//Properties and commands are generated automatically for the following class when compiling:
//Refer to the following help topic for details: https://docs.devexpress.com/WPF/402989/mvvm-framework/viewmodels/compile-time-generated-viewmodels
[GenerateViewModel]
public partial class BarChartViewModel
{
    [GenerateProperty]
    ObservableCollection<Order> orders;

    [GenerateProperty]
    Order selectedItem;

    [GenerateProperty]
    IList<Order> selectedItems;

    [GenerateProperty]
    decimal? yearTotalAmount;

    [GenerateProperty]
    int? yearNumberOfOrders;

    [GenerateProperty]
    string yearBestSellingProduct;

    public BarChartViewModel() { }
    public BarChartViewModel(ICommonDataService dataService)
    {
        Orders = new(dataService.GetOrders());
        SelectedItem = Orders.FirstOrDefault();
    }

    void OnSelectedItemChanged()
    {
        if (SelectedItem == null)
        {
            YearNumberOfOrders = null;
            YearTotalAmount = null;
            YearBestSellingProduct = null;
            SelectedItems = null;
            return;
        }
        var yearOrders = Orders.Where(o => o.OrderDate.Year == SelectedItem.OrderDate.Year).ToObservableCollection();
        SelectedItems = yearOrders;
        YearNumberOfOrders = yearOrders.Count;
        YearTotalAmount = yearOrders.Sum(o => o.TotalAmount);
        YearBestSellingProduct = yearOrders.SelectMany(o => o.Items)
            .GroupBy(oi => oi.Product.Name)
            .OrderByDescending(g => g.Sum(oi => oi.Quantity))
            .First().Key;
    }
}
