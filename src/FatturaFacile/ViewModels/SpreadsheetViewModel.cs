// <copyright file="SpreadsheetViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;

namespace FatturaFacile.ViewModels;
//Properties and commands are generated automatically for the following class when compiling:
//Refer to the following help topic for details: https://docs.devexpress.com/WPF/402989/mvvm-framework/viewmodels/compile-time-generated-viewmodels
[GenerateViewModel(ImplementISupportServices = true)]
public partial class SpreadsheetViewModel
{
    [GenerateProperty]
    string documentPath;
    bool isInitialized;

    [GenerateCommand]
    void Init()
    {
        if (isInitialized)
            return;
        ShowDocument(@"Resources/SampleExcelDoc.xlsx");
        isInitialized = true;
    }
    void ShowDocument(string path)
    {
        SplashScreenService.Show();
        DocumentPath = path;
    }
    ISplashScreenManagerService SplashScreenService => this.GetRequiredService<ISplashScreenManagerService>();

    [GenerateCommand]
    void HideSplashScreen()
    {
        SplashScreenService.Close();
    }
}
