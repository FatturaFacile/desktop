using System.Windows.Controls;

using FatturaFacile.Wpf.ViewModels;

namespace FatturaFacile.Wpf.Views
{
    public partial class MainPage : Page
    {
        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
