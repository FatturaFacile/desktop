using System;
using System.Windows.Controls;

namespace FatturaFacile.Wpf.Contracts.Services
{
    public interface IPageService
    {
        Type GetPageType(string key);

        Page GetPage(string key);
    }
}
