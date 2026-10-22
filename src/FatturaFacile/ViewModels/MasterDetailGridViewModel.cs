// <copyright file="MasterDetailGridViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Linq;

using DevExpress.Mvvm.CodeGenerators;

using FatturaFacile.Infrastructure;
using FatturaFacile.Model;

namespace FatturaFacile.ViewModels;
//Properties and commands are generated automatically for the following class when compiling:
//Refer to the following help topic for details: https://docs.devexpress.com/WPF/402989/mvvm-framework/viewmodels/compile-time-generated-viewmodels
[GenerateViewModel]
public partial class MasterDetailGridViewModel(ICommonDataService dataService)
{
    [GenerateProperty]
    ObservableCollection<Customer> customers;

    [GenerateProperty]
    Customer selectedCustomer;

    ICommonDataService DataService = dataService;
    bool isInitialized;

    public MasterDetailGridViewModel() : this(null) { }

    [GenerateCommand]
    void Init()
    {
        if (isInitialized)
            return;
        Customers = new(DataService.GetCustomers());
        SelectedCustomer = Customers.FirstOrDefault();
        isInitialized = true;
    }
}
