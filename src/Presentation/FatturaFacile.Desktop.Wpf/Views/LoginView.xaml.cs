using System.Windows;
using System.Windows.Controls;
using FatturaFacile.Desktop.Wpf.ViewModels;

namespace FatturaFacile.Desktop.Wpf.Views;

/// <summary>
/// Interaction logic for LoginView.xaml.
/// </summary>
public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm && sender is PasswordBox pb)
        {
            vm.Password = pb.Password;
        }
    }
}
