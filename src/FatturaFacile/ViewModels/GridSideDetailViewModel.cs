using System.Collections.ObjectModel;
using System.Linq;

using DevExpress.Mvvm.CodeGenerators;
using DevExpress.Xpf.Core;

using FatturaFacile.Infrastructure;
using FatturaFacile.Model;

namespace FatturaFacile.ViewModels;
//Properties and commands are generated automatically for the following class when compiling:
//Refer to the following help topic for details: https://docs.devexpress.com/WPF/402989/mvvm-framework/viewmodels/compile-time-generated-viewmodels
[GenerateViewModel]
public partial class GridSideDetailViewModel(ICommonDataService dataService)
{
    [GenerateProperty]
    ObservableCollection<Customer> customers;

    [GenerateProperty]
    Customer selectedCustomer;

    ICommonDataService DataService = dataService;
    bool isInitialized;

    public GridSideDetailViewModel() : this(null) { }

    [GenerateCommand]
    void Init()
    {
        if (isInitialized)
            return;
        Customers = new(DataService.GetCustomers());
        SelectedCustomer = Customers.FirstOrDefault();
        isInitialized = true;
    }

    [GenerateCommand]
    void Action(object parameter)
    {
        ThemedMessageBox.Show($"Action on {parameter}");
    }
}
