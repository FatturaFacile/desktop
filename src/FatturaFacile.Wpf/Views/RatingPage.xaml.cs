using System;
using System.Windows.Controls;
using FatturaFacile.Wpf.ViewModels;
using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Tools.Controls;
namespace FatturaFacile.Wpf.Views
{
    public partial class RatingPage : Page
    {
		public string themeName = App.Current.Properties["Theme"]?.ToString()!= null? App.Current.Properties["Theme"]?.ToString(): "Windows11Light";
        public RatingPage(RatingViewModel viewModel)
        {
            InitializeComponent();		
            DataContext = viewModel;
			SfSkinManager.SetTheme(this, new Theme(themeName));
        }	
    }
}
