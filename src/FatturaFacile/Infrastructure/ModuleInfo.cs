using System;
using System.ComponentModel;

namespace FatturaFacile.Infrastructure;

sealed public partial class ModuleInfo(string caption, Type viewType, Type viewModelType) : INotifyPropertyChanged
{
    public string Caption { get; private set; } = caption;
    public Type ViewType { get; private set; } = viewType;
    public Type ViewModelType { get; private set; } = viewModelType;
    public string ViewTypeName => ViewType.Name;

#pragma warning disable CS0067
    //Empty INotifyPropertyChanged to avoid memory leaks
    event PropertyChangedEventHandler propertyChanged;
    event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
    {
        add => propertyChanged += value;
        remove => propertyChanged -= value;
    }
#pragma warning restore CS0067
}
