using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FatturaFacile.Wpf.Contracts.Services;
using FatturaFacile.Wpf.Helpers;
using FatturaFacile.Wpf.ViewModels;
using Syncfusion.Windows.Shared;
namespace FatturaFacile.Wpf.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MarkdownViewerPage : Page
    {
        public MarkdownViewerPage(MarkdownViewerViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
