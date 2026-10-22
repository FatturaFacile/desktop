using System.Collections.ObjectModel;

using DevExpress.Mvvm.CodeGenerators;

using FatturaFacile.Infrastructure;
using FatturaFacile.Model;

namespace FatturaFacile.ViewModels;
//Properties and commands are generated automatically for the following class when compiling:
//Refer to the following help topic for details: https://docs.devexpress.com/WPF/402989/mvvm-framework/viewmodels/compile-time-generated-viewmodels
[GenerateViewModel(ImplementISupportServices = true)]
public partial class GanttBasicViewModel(ICommonDataService dataService)
{
    [GenerateProperty]
    ObservableCollection<GanttTaskItem> tasks;
    ICommonDataService DataService = dataService;
    bool isInitialized;

    public GanttBasicViewModel() : this(null) { }

    [GenerateCommand]
    void Init()
    {
        if (isInitialized)
            return;
        Tasks = new(DataService.GetGanttTasks());
        isInitialized = true;
    }
}
