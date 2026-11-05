using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FatturaFacile.Wpf.ViewModels;
using Syncfusion.SfSkinManager;
using Syncfusion.UI.Xaml.SfToastNotification;
using Syncfusion.Windows.Tools.Controls;
namespace FatturaFacile.Wpf.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ToastNotificationPage : Page
    {
        public ToastNotificationPage(ToastNotificationViewModel viewModel) 
        {
            InitializeComponent();
            DataContext = viewModel;
        }
        private async void DefaultUI_Click(object sender, RoutedEventArgs e)
        {
             SfToastNotification.Show(this, new ToastOptions
            {
                Severity=ToastSeverity.Info,
                Mode=ToastMode.Window,
                Title = "Application Update",
                Header = "Configuration applied",
                Message = "Changes are now in effect.",
                Placement=ToastPlacement.BottomRight,
                PreventAutoClose=true,
                ShowActionButtons=true,
                ShowCloseButton=true,
                Variant =ToastVariant.Text,
                ShowAnimationType=ToastAnimation.FadeIn,
                CloseAnimationType =ToastAnimation.FadeOut
             });
        }
        private void CloseAll_Click(object sender, RoutedEventArgs e)
        {
            SfToastNotification.CloseAll();
        }
    }
}
