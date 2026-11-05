using System;
using System.Windows;
using System.Windows.Controls;

using FatturaFacile.Wpf.Contracts.Services;
using FatturaFacile.Wpf.Contracts.Views;
using FatturaFacile.Wpf.ViewModels;

using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Shared;
using Syncfusion.Windows.Tools.Controls;

namespace FatturaFacile.Wpf.Views
{
    public partial class ShellWindow : ChromelessWindow, IShellWindow
    {
        public string themeName = App.Current.Properties["Theme"]?.ToString()!= null? App.Current.Properties["Theme"]?.ToString(): "Windows11Light";
        public ShellWindow(IPageService pageService, ShellViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            SfSkinManager.SetTheme(this, new Theme(themeName));
        }

        public Frame GetNavigationFrame()
            => shellFrame;

        public void ShowWindow()
            => Show();

        public void CloseWindow()
            => Close();
    }
}
