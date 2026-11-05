using System;

using FatturaFacile.Wpf.Models;

namespace FatturaFacile.Wpf.Contracts.Services
{
    public interface IThemeSelectorService
    {
        bool SetTheme(AppTheme? theme = null);

        AppTheme GetCurrentTheme();
    }
}
