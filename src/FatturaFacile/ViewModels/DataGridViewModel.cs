using System.Collections.ObjectModel;

using DevExpress.Mvvm.CodeGenerators;

using FatturaFacile.Infrastructure;
using FatturaFacile.Model;

namespace FatturaFacile.ViewModels;
//Properties and commands are generated automatically for the following class when compiling:
//Refer to the following help topic for details: https://docs.devexpress.com/WPF/402989/mvvm-framework/viewmodels/compile-time-generated-viewmodels
[GenerateViewModel]
public partial class DataGridViewModel(ICommonDataService dataService)
{
    [GenerateProperty]
    ObservableCollection<Customer> customers;
    ICommonDataService DataService = dataService;
    bool isInitialized;

    public DataGridViewModel() : this(null) { }

    [GenerateCommand]
    void Init()
    {
        if (isInitialized)
            return;
        Customers = new(DataService.GetCustomers());
        isInitialized = true;
    }
}
