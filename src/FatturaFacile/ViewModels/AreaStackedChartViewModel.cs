// <copyright file="AreaStackedChartViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using DevExpress.Mvvm.CodeGenerators;

using FatturaFacile.Infrastructure;
using FatturaFacile.Model;

namespace FatturaFacile.ViewModels;
//Properties and commands are generated automatically for the following class when compiling:
//Refer to the following help topic for details: https://docs.devexpress.com/WPF/402989/mvvm-framework/viewmodels/compile-time-generated-viewmodels
[GenerateViewModel]
public partial class AreaStackedChartViewModel
{
    [GenerateProperty]
    ObservableCollection<Order> orders;

    [GenerateProperty]
    DateTime startRange;

    [GenerateProperty]
    DateTime endRange;

    public AreaStackedChartViewModel() { }
    public AreaStackedChartViewModel(ICommonDataService dataService)
    {
        Orders = new(dataService.GetOrders());
        EndRange = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);
        StartRange = EndRange.AddMonths(-4);
    }
}
