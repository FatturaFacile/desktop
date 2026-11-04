using FatturaFacile.Desktop.Wpf.ViewModels;

namespace FatturaFacile.Desktop.Wpf.Services;

/// <summary>
/// Navigation contract for switching views and workspaces within the desktop ERP client.
/// </summary>
public interface INavigationService
{
    ViewModelBase? CurrentViewModel { get; }
    event Action<ViewModelBase>? CurrentViewModelChanged;

    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
    void NavigateTo(ViewModelBase viewModel);
}

/// <summary>
/// Default in-memory desktop navigation service coordinating active views and tabs.
/// </summary>
public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private ViewModelBase? _currentViewModel;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ViewModelBase? CurrentViewModel => _currentViewModel;

    public event Action<ViewModelBase>? CurrentViewModelChanged;

    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        var vm = (ViewModelBase)_serviceProvider.GetService(typeof(TViewModel))!;
        NavigateTo(vm);
    }

    public void NavigateTo(ViewModelBase viewModel)
    {
        _currentViewModel = viewModel;
        CurrentViewModelChanged?.Invoke(viewModel);
    }
}
