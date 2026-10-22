using System.Collections.ObjectModel;

using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;

using FatturaFacile.Infrastructure;
using FatturaFacile.Model;

namespace FatturaFacile.ViewModels;
//Properties and commands are generated automatically for the following class when compiling:
//Refer to the following help topic for details: https://docs.devexpress.com/WPF/402989/mvvm-framework/viewmodels/compile-time-generated-viewmodels
[GenerateViewModel(ImplementISupportServices = true)]
public partial class DiagramControlViewModel(ICommonDataService dataService)
{
    [GenerateProperty]
    ObservableCollection<CompanyUser> employees;
    ICommonDataService DataService = dataService;
    bool isInitialized;
    ISplashScreenManagerService SplashScreenService => this.GetRequiredService<ISplashScreenManagerService>();

    public DiagramControlViewModel() : this(null) { }

    [GenerateCommand]
    void Init()
    {
        if (isInitialized)
            return;
        SplashScreenService.Show();
        Employees = new(DataService.GetCompanyUsers());
        isInitialized = true;
    }

    [GenerateCommand]
    void HideSplashScreen()
    {
        SplashScreenService.Close();
    }
}
