// <copyright file="TreeListBasicViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;

using DevExpress.Mvvm.CodeGenerators;

using FatturaFacile.Infrastructure;
using FatturaFacile.Model;

namespace FatturaFacile.ViewModels;

[GenerateViewModel]
public partial class TreeListBasicViewModel(ICommonDataService dataService)
{
    [GenerateProperty]
    ObservableCollection<CompanyUser> employees;
    ICommonDataService DataService = dataService;
    bool isInitialized;

    public TreeListBasicViewModel() : this(null) { }

    [GenerateCommand]
    void Init()
    {
        if (isInitialized)
            return;
        Employees = new(DataService.GetCompanyUsers());
        isInitialized = true;
    }
}
