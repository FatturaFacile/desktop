// <copyright file="App.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using System.Runtime;
using System.Windows;

using DevExpress.Xpf.Core;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Extensions;
using FatturaFacile.Configuration;
using FatturaFacile.Extensions;
using FatturaFacile.Infrastructure;
using FatturaFacile.Infrastructure.Localization;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Services;
using FatturaFacile.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FatturaFacile;

/// <summary>
/// Application entry point and composition root responsible for bootstrapping dependency injection,
/// theme initialization, local database migrations, and application lifecycle events.
/// </summary>
public partial class App : System.Windows.Application
{
    private static readonly string DatabaseDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FatturaFacile");

    private static readonly string DatabaseFilePath = Path.Combine(DatabaseDirectory, "FatturaFacile.db");

    /// <summary>
    /// Gets the shared generic <see cref="IHost"/> instance managing the dependency injection container,
    /// application configuration, and hosted background services for the entire desktop lifecycle.
    /// </summary>
    public static IHost AppHost { get; private set; } = null!;

    /// <summary>
    /// Initializes static members of the <see cref="App"/> class.
    /// Configures cold-start JIT optimizations via profile-guided optimization and preloads DevExpress lightweight themes.
    /// </summary>
    static App()
    {
        // 1. Ottimizzazione JIT per accelerare il cold-start
        ProfileOptimization.SetProfileRoot(AppContext.BaseDirectory);
        ProfileOptimization.StartProfile("Startup.Profile");

        // 2. Configurazione Temi DevExpress Lightweight
        CompatibilitySettings.UseLightweightThemes = true;
        ApplicationThemeHelper.ApplicationThemeName = LightweightTheme.Win11System.Name;

        // 3. Precaricamento asincrono delle categorie DevExpress utilizzate nell'applicazione
        _ = ApplicationThemeHelper.PreloadAsync(
            PreloadCategories.Core,
            PreloadCategories.Ribbon,
            PreloadCategories.Grid,
            PreloadCategories.Docking,
            PreloadCategories.Printing,
            PreloadCategories.Dashboard);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// Builds the Microsoft.Extensions host configuring configuration providers, application layers,
    /// persistence storage in LocalAppData, and presentation services.
    /// </summary>
    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                _ = config.SetBasePath(AppContext.BaseDirectory);
                _ = config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                _ = config.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                // Binda le sezioni di configurazione Strongly-Typed
                _ = services.Configure<AiOptions>(context.Configuration.GetSection(AiOptions.SectionName));
                _ = services.Configure<DevAVFilterOptions>(
                    context.Configuration.GetSection(DevAVFilterOptions.SectionName));

                // Layer Applicativo (CQRS, MediatR, FluentValidation)
                _ = services.AddApplicationServices();

                // Layer Infrastruttura (EF Core SQLite in LocalAppData, Repositories, Localization)
                _ = Directory.CreateDirectory(DatabaseDirectory);
                var connectionString = $"Data Source={DatabaseFilePath}";
                _ = services.AddInfrastructureServices(context.Configuration, connectionString);

                // Layer di Presentazione (IUserStateService, IViewModelFactory, ViewModels, Shell)
                _ = services.AddPresentationServices(context.Configuration);
            })
            .Build();
    }

    /// <summary>
    /// Handles the application startup sequence, displaying the themed splash screen, bootstrapping
    /// the generic host, running pre-migration SQLite database backups, executing pending migrations,
    /// seeding default tenant/user data, warming localization cache, and presenting the main application window.
    /// </summary>
    /// <param name="e">The <see cref="StartupEventArgs"/> containing event arguments.</param>
    protected override async void OnStartup(
        StartupEventArgs e)
    {
        // Mostra lo splash screen nativo tematizzato DevExpress
        SplashScreenManager.CreateThemed().ShowOnStartup();

        base.OnStartup(e);

        // Avvio dell'host generico di .NET
        await AppHost.StartAsync();

        // Inizializzazione Database, Backup preventivo, Migrazioni e Cache
        using (var scope = AppHost.Services.CreateScope())
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<App>>();
            var db = scope.ServiceProvider.GetRequiredService<FatturaFacileDbContext>();

            // 1. Backup di sicurezza preventivo del database SQLite
            var activeBackupPath = CreateDatabaseBackup(logger);

            // 2. Esecuzione migrazioni con ripristino automatico in caso di errore
            try
            {
                await db.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Errore durante l'applicazione delle migrazioni EF Core. Avvio ripristino di emergenza dal backup.");

                // Chiude la connessione del contesto prima di toccare i file
                await db.Database.CloseConnectionAsync();

                // Ripristina lo stato precedente alla migrazione
                RestoreDatabaseBackup(activeBackupPath, logger);

                _ = ThemedMessageBox.Show(
                    title: "Errore Aggiornamento Database",
                    text:
                    "Si è verificato un errore durante l'aggiornamento dello schema del database.\nI dati sono stati ripristinati alla versione precedente.\n\nContattare l'assistenza tecnica.",
                    messageBoxButtons: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);

                Shutdown(-1);
                return;
            }

            // 3. Esegue il seeding iniziale (creazione tenant/utenti/dati base)
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            await DbInitializer.SeedAsync(db, hasher);

            // 4. Inizializza la cache locale delle traduzioni
            if (scope.ServiceProvider.GetService<ILocalizationService>() is SqliteLocalizationService locService)
            {
                await locService.InitializeCacheAsync();
                LocalizationManager.Instance.Initialize(locService);
            }
        }

        // Risoluzione ed apertura della Shell MainWindow
        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        var mainVm = AppHost.Services.GetRequiredService<MainViewModel>();
        mainWindow.DataContext = mainVm;

        await mainVm.InitializeAsync();

        mainWindow.Show();
    }

    /// <summary>
    /// Handles the application shutdown sequence, persisting current application theme preferences,
    /// stopping the hosted services asynchronously, and disposing the host container.
    /// </summary>
    /// <param name="e">The <see cref="ExitEventArgs"/> containing exit state information.</param>
    protected override async void OnExit(
        ExitEventArgs e)
    {
        ApplicationThemeHelper.SaveApplicationThemeName();

        if (AppHost != null)
        {
            await AppHost.StopAsync();
            AppHost.Dispose();
        }

        base.OnExit(e);
    }

    /// <summary>
    /// Creates a pre-migration timestamped safety backup of the SQLite database and retains the last five snapshots.
    /// </summary>
    /// <param name="logger">The application logger for recording audit and diagnostic messages.</param>
    /// <returns>The fully-qualified path to the created backup archive, or <see langword="null"/> if the source database did not exist or backup failed.</returns>
    private static string? CreateDatabaseBackup(
        ILogger logger)
    {
        try
        {
            if (!File.Exists(DatabaseFilePath))
            {
                return null;
            }

            var backupDirectory = Path.Combine(DatabaseDirectory, "Backups");
            _ = Directory.CreateDirectory(backupDirectory);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFilePath = Path.Combine(backupDirectory, $"FatturaFacile_PreMigration_{timestamp}.db");

            File.Copy(DatabaseFilePath, backupFilePath, overwrite: true);
            logger.LogInformation("Backup del database pre-migrazione creato in: {BackupFilePath}", backupFilePath);

            // Mantiene solo gli ultimi 5 backup
            var backupFiles = new DirectoryInfo(backupDirectory)
                .GetFiles("FatturaFacile_PreMigration_*.db")
                .OrderByDescending(f => f.CreationTimeUtc)
                .Skip(5);

            foreach (var oldBackup in backupFiles)
            {
                oldBackup.Delete();
            }

            return backupFilePath;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex, "Impossibile creare il backup preventivo del database. Il processo prosegue a proprio rischio.");
            return null;
        }
    }

    /// <summary>
    /// Restores a previously created backup snapshot into the primary database file following a critical migration error.
    /// Clears active SQLite connection pools prior to copying to prevent file lock contention.
    /// </summary>
    /// <param name="backupFilePath">The file path to the verified database backup snapshot.</param>
    /// <param name="logger">The application logger for error and diagnostic reporting.</param>
    private static void RestoreDatabaseBackup(
        string? backupFilePath,
        ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(backupFilePath) || !File.Exists(backupFilePath))
        {
            logger.LogWarning("Nessun file di backup valido disponibile per il ripristino automatico.");
            return;
        }

        try
        {
            // 1. Rilascia tutti i pool di connessione aperti da SQLite sul file per evitare errori di blocco
            SqliteConnection.ClearAllPools();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // 2. Sovrascrive il file con la copia di sicurezza integra
            File.Copy(backupFilePath, DatabaseFilePath, overwrite: true);
            logger.LogInformation(
                "Ripristino di emergenza del database completato con successo da {BackupFilePath}", backupFilePath);
        }
        catch (Exception ex)
        {
            logger.LogCritical(
                ex, "Errore critico durante il ripristino di emergenza del database da {BackupFilePath}",
                backupFilePath);
        }
    }
}
