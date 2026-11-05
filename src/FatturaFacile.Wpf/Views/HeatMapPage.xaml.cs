using System.Windows.Controls;

using FatturaFacile.Wpf.ViewModels;

using Syncfusion.SfSkinManager;

namespace FatturaFacile.Wpf.Views
{
    public partial class HeatMapPage : Page
    {
		public string themeName = App.Current.Properties["Theme"]?.ToString()!= null? App.Current.Properties["Theme"]?.ToString(): "Windows11Light";
        public HeatMapPage(HeatMapViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
			SfSkinManager.SetTheme(this, new Theme(themeName));
        }
    }
}
