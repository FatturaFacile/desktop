using System.Windows.Controls;

namespace FatturaFacile.Wpf.Contracts.Views
{
    public interface IShellWindow
    {
        Frame GetNavigationFrame();

        void ShowWindow();

        void CloseWindow();
    }
}
