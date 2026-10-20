using CommunityToolkit.Mvvm.ComponentModel;

namespace FatturaFacile.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    public partial bool IsBusy { get; set; }
    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    public virtual Task InitializeAsync() => Task.CompletedTask;
}
