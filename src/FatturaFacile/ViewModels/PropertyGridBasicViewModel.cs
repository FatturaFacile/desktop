using System;
using System.Collections.ObjectModel;
using System.Linq;

using DevExpress.Mvvm.CodeGenerators;

using FatturaFacile.Infrastructure;
using FatturaFacile.Model;

namespace FatturaFacile.ViewModels;
//Properties and commands are generated automatically for the following class when compiling:
//Refer to the following help topic for details: https://docs.devexpress.com/WPF/402989/mvvm-framework/viewmodels/compile-time-generated-viewmodels
[GenerateViewModel]
public partial class PropertyGridBasicViewModel
{
    [GenerateProperty]
    Order editedOrder;

    [GenerateProperty]
    ObservableCollection<Customer> customers;

    [GenerateProperty]
    ObservableCollection<Product> products;
    public PropertyGridBasicViewModel() : this(null) { }
    public PropertyGridBasicViewModel(ICommonDataService dataService)
    {
        EditedOrder = dataService.GetOrders().First();
        Customers = new(dataService.GetCustomers());
        Products = new(dataService.GetProducts());
    }
}
