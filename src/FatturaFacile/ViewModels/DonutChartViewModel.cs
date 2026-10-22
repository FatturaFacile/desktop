using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using DevExpress.Mvvm.CodeGenerators;

using FatturaFacile.Infrastructure;
using FatturaFacile.Model;

namespace FatturaFacile.ViewModels;
//Properties and commands are generated automatically for the following class when compiling:
//Refer to the following help topic for details: https://docs.devexpress.com/WPF/402989/mvvm-framework/viewmodels/compile-time-generated-viewmodels
[GenerateViewModel]
public partial class DonutChartViewModel
{
    IEnumerable<Order> orders;
    [GenerateProperty]
    ObservableCollection<ChartDataPoint<string, decimal>> chartData;

    [GenerateProperty]
    ChartDataPoint<string, decimal> selectedSegment;

    [GenerateProperty]
    int? numberOfOrders;

    [GenerateProperty]
    string bestSellingProduct;

    [GenerateProperty]
    decimal totalAmount;

    public DonutChartViewModel() { }
    public DonutChartViewModel(ICommonDataService dataService)
    {
        orders = dataService.GetOrders();
        ChartData = new(orders
            .SelectMany(o => o.Items)
            .GroupBy(oi => oi.Order.Customer.Country)
            .Select(g => new ChartDataPoint<string, decimal>(g.Key, g.Sum(oi => oi.Amount)))
            .OrderByDescending(c => c.Value));
        SelectedSegment = ChartData.FirstOrDefault();
    }

    void OnSelectedSegmentChanged()
    {
        List<Order> selectedOrders = null;
        if (SelectedSegment == null)
        {
            selectedOrders = orders.ToList();
        }
        else
        {
            selectedOrders = orders.Where(o => o.Customer.Country == SelectedSegment.Argument).ToList();
        }
        NumberOfOrders = selectedOrders.Count;
        TotalAmount = selectedOrders.Sum(o => o.TotalAmount);
        BestSellingProduct = selectedOrders
            .SelectMany(o => o.Items)
            .GroupBy(oi => oi.Product.Name)
            .OrderByDescending(g => g.Sum(oi => oi.Quantity))
            .First().Key;
    }
}
