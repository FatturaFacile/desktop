using System.Windows.Controls;

using FatturaFacile.Wpf.Contracts.Views;
using FatturaFacile.Wpf.ViewModels;

using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Shared;

namespace FatturaFacile.Wpf.Views
{
    public partial class ShellDialogWindow : ChromelessWindow, IShellDialogWindow
    {
        public ShellDialogWindow(ShellDialogViewModel viewModel)
        {
            InitializeComponent();
            viewModel.SetResult = OnSetResult;
            DataContext = viewModel;
        }

        public Frame GetDialogFrame()
            => dialogFrame;

        private void OnSetResult(bool? result)
        {
            DialogResult = result;
            Close();
        }
    }
}
