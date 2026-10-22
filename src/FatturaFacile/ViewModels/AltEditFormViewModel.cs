using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DevExpress.Mvvm.CodeGenerators;
using DevExpress.Xpf.Core;

using FatturaFacile.Infrastructure;

namespace FatturaFacile.ViewModels;
//Properties and commands are generated automatically for the following class when compiling:
//Refer to the following help topic for details: https://docs.devexpress.com/WPF/402989/mvvm-framework/viewmodels/compile-time-generated-viewmodels
[GenerateViewModel]
public partial class AltEditFormViewModel
{
    [GenerateProperty]
    IEnumerable<string> countries;
    public AltEditFormViewModel() { }
    public AltEditFormViewModel(ICommonDataService dataService)
    {
        Countries = dataService.GetCountries();
    }

    [GenerateCommand]
    async Task Save()
    {
        await Task.Delay(700);
        ThemedMessageBox.Show("Asynchronous save action");
    }
}
