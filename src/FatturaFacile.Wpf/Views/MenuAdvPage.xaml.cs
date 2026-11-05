using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using FatturaFacile.Wpf.ViewModels;
using Syncfusion.SfSkinManager;
namespace FatturaFacile.Wpf.Views
{
    public partial class MenuAdvPage : Page
    {
		public string themeName = App.Current.Properties["Theme"]?.ToString()!= null? App.Current.Properties["Theme"]?.ToString(): "Windows11Light";
        public MenuAdvPage(MenuAdvViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
			SfSkinManager.SetTheme(this, new Theme(themeName));
        }
    }
}
