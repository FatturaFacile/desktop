// <copyright file="KanbanViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;

using DevExpress.Mvvm.CodeGenerators;

using FatturaFacile.Infrastructure;
using FatturaFacile.Model;

namespace FatturaFacile.ViewModels;
//Properties and commands are generated automatically for the following class when compiling:
//Refer to the following help topic for details: https://docs.devexpress.com/WPF/402989/mvvm-framework/viewmodels/compile-time-generated-viewmodels
[GenerateViewModel]
public partial class KanbanViewModel
{
    [GenerateProperty]
    ObservableCollection<KanbanGroup> groups;
    public KanbanViewModel(ICommonDataService dataservice)
    {
        Groups = new(dataservice.GetKanbanData());
    }
}
