using System.Collections.ObjectModel;

using DevExpress.Mvvm.CodeGenerators;

using FatturaFacile.Infrastructure;

namespace FatturaFacile.ViewModels;
//Properties and commands are generated automatically for the following class when compiling:
//Refer to the following help topic for details: https://docs.devexpress.com/WPF/402989/mvvm-framework/viewmodels/compile-time-generated-viewmodels
[GenerateViewModel]
public partial class TabbedDocShellViewModel
{
    [GenerateProperty]
    ObservableCollection<ModuleInfo> modules;
    public TabbedDocShellViewModel()
    {
        Modules = new(AppModules.Modules);
    }
}
