using System;

using DevExpress.Mvvm.CodeGenerators;

namespace FatturaFacile.Model;

[GenerateViewModel]
public partial class ModuleInfo(string caption, Type viewType, Type viewModelType)
{
    [GenerateProperty]
    string _caption = caption;

    [GenerateProperty]
    Type _viewType = viewType;

    [GenerateProperty]
    Type _viewModelType = viewModelType;
    public string ViewTypeName => _viewType.Name;
}