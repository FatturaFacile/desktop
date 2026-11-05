using System.Windows.Input;

using FatturaFacile.Wpf.Contracts.Services;
using FatturaFacile.Wpf.Helpers;
using FatturaFacile.Wpf.Properties;

using Syncfusion.Windows.Shared;

namespace FatturaFacile.Wpf.ViewModels
{
    public class ShellViewModel : Observable
    {
		private readonly INavigationService _navigationService;
        private readonly IRightPaneService _rightPaneService;
        private ICommand _loadedCommand;
        private ICommand _unloadedCommand;
		private ICommand _viewCommand;

        public ICommand LoadedCommand => _loadedCommand ?? (_loadedCommand = new RelayCommand(OnLoaded));

        public ICommand UnloadedCommand => _unloadedCommand ?? (_unloadedCommand = new RelayCommand(OnUnloaded));

        public ShellViewModel(INavigationService navigationService, IRightPaneService rightPaneService)
        {
			_navigationService = navigationService;
            _rightPaneService = rightPaneService;
        }

        private void OnLoaded()
        {
        }

        private void OnUnloaded()
        {
            _rightPaneService.CleanUp();
        }
		
		public ICommand ViewSelectionChangedCommand => _viewCommand ?? (_viewCommand = new DelegateCommand(OnViewSelected));
		
		 private void OnViewSelected(object viewName)
        {
            string header = viewName.ToString();
			if(header == null)
            {
                //_navigationService.NavigateTo(typeof(ItemNameViewModel).FullName, null, true);
            }
            //_navigationService.NavigateTo(typeof(ItemNameViewModel).FullName, null, true);
			else if(header == Resources.ShellMenuItemViewsTreeViewPageHeader)
					_navigationService.NavigateTo(typeof(TreeViewViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsTreeGridPageHeader)
					_navigationService.NavigateTo(typeof(TreeGridViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsToolBarAdvPageHeader)
					_navigationService.NavigateTo(typeof(ToolBarAdvViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsToastNotificationPageHeader)
					_navigationService.NavigateTo(typeof(ToastNotificationViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsTileViewPageHeader)
					_navigationService.NavigateTo(typeof(TileViewViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsTabControlExtPageHeader)
					_navigationService.NavigateTo(typeof(TabControlExtViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsSmartTextEditorPageHeader)
					_navigationService.NavigateTo(typeof(SmartTextEditorViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsSmartDataGridPageHeader)
					_navigationService.NavigateTo(typeof(SmartDataGridViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsSettingsPageHeader)
					_navigationService.NavigateTo(typeof(SettingsViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsSchedulerPageHeader)
					_navigationService.NavigateTo(typeof(SchedulerViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsRibbonPageHeader)
					_navigationService.NavigateTo(typeof(RibbonViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsRatingPageHeader)
					_navigationService.NavigateTo(typeof(RatingViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsRangeSliderPageHeader)
					_navigationService.NavigateTo(typeof(RangeSliderViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsRadialMenuPageHeader)
					_navigationService.NavigateTo(typeof(RadialMenuViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsPropertyGridPageHeader)
					_navigationService.NavigateTo(typeof(PropertyGridViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsPivotGridPageHeader)
					_navigationService.NavigateTo(typeof(PivotGridViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsNavigationDrawerPageHeader)
					_navigationService.NavigateTo(typeof(NavigationDrawerViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsMenuAdvPageHeader)
					_navigationService.NavigateTo(typeof(MenuAdvViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsMarkdownViewerPageHeader)
					_navigationService.NavigateTo(typeof(MarkdownViewerViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsMainPageHeader)
					_navigationService.NavigateTo(typeof(MainViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsKanbanPageHeader)
					_navigationService.NavigateTo(typeof(KanbanViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsHeatMapPageHeader)
					_navigationService.NavigateTo(typeof(HeatMapViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsGridControlPageHeader)
					_navigationService.NavigateTo(typeof(GridControlViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsEditorControlPageHeader)
					_navigationService.NavigateTo(typeof(EditorControlViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsDocumentContainerPageHeader)
					_navigationService.NavigateTo(typeof(DocumentContainerViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsDockingManagerPageHeader)
					_navigationService.NavigateTo(typeof(DockingManagerViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsDiagramsPageHeader)
					_navigationService.NavigateTo(typeof(DiagramsViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsDateTimeRangeNavigatorPageHeader)
					_navigationService.NavigateTo(typeof(DateTimeRangeNavigatorViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsDataGridPageHeader)
					_navigationService.NavigateTo(typeof(DataGridViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsChartsPageHeader)
					_navigationService.NavigateTo(typeof(ChartsViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsCarouselPageHeader)
					_navigationService.NavigateTo(typeof(CarouselViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsBusyIndicatorPageHeader)
					_navigationService.NavigateTo(typeof(BusyIndicatorViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsAutoCompletePageHeader)
					_navigationService.NavigateTo(typeof(AutoCompleteViewModel).FullName, null, true);
        

			else if(header == Resources.ShellMenuItemViewsAIAssistViewPageHeader)
					_navigationService.NavigateTo(typeof(AIAssistViewViewModel).FullName, null, true);
        
		}
    }
}
