using System.IO;
using System.Windows;
using FatturaFacile.Application;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Infrastructure;
using FatturaFacile.Infrastructure.Localization;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Services;
using FatturaFacile.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile;

public partial class App : System.Windows.Application
{
    private IServiceProvider _serviceProvider = null!;

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        var services = new ServiceCollection();

        // 1. Connection string for local SQLite DB
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FatturaFacile.db");
        var connectionString = $"Data Source={dbPath}";

        // 2. Layer registrations
        _ = services.AddApplication();
        _ = services.AddInfrastructure(connectionString);

        // 3. Register WPF ViewModels
        _ = services.AddSingleton<DashboardViewModel>();
        _ = services.AddSingleton<ClientsViewModel>();
        _ = services.AddSingleton<InvoicesViewModel>();
        _ = services.AddSingleton<TireHotelViewModel>();
        _ = services.AddSingleton<TimeTrackingViewModel>();
        _ = services.AddSingleton<LocalizationAdminViewModel>();
        _ = services.AddSingleton<LoginViewModel>();
        _ = services.AddSingleton<MainViewModel>();

        // 4. Register Views
        _ = services.AddSingleton<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();

        // 5. Seed Database & Initialize Localization Cache
        using (var scope = _serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FatturaFacileDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            await DbInitializer.SeedAsync(db, hasher);

            var locService = (SqliteLocalizationService)_serviceProvider.GetRequiredService<ILocalizationService>();
            await locService.InitializeCacheAsync();

            LocalizationManager.Instance.Initialize(locService);
        }

        // 6. Launch MainWindow
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        var mainVm = _serviceProvider.GetRequiredService<MainViewModel>();
        mainWindow.DataContext = mainVm;
        await mainVm.InitializeAsync();

        mainWindow.Show();
    }
}
