// <copyright file="App.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using System.Runtime;
using System.Windows;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Extensions;
using FatturaFacile.Configuration;
using FatturaFacile.Extensions;
using FatturaFacile.Infrastructure.Extensions;
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
/// Application entry point and composition root.
/// </summary>
public partial class App : System.Windows.Application
{
    private static readonly string DatabaseDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FatturaFacile");

    private static readonly string DatabaseFilePath = Path.Combine(DatabaseDirectory, "FatturaFacile.db");

    public static IHost AppHost { get; private set; } = null!;

    static App()
    {
        // 1. Ottimizzazione JIT per accelerare il cold-start
        ProfileOptimization.SetProfileRoot(AppContext.BaseDirectory);
        ProfileOptimization.StartProfile("Startup.Profile");

        // 2. Configurazione Temi DevExpress Lightweight
        CompatibilitySettings.UseLightweightThemes = true;
        ApplicationThemeHelper.ApplicationThemeName = LightweightTheme.Win11System.Name;

        // 3. Precaricamento asincrono delle categorie DevExpress utilizzate nell'applicazione
        ApplicationThemeHelper.PreloadAsync(
            PreloadCategories.Core,
            PreloadCategories.Ribbon,
            PreloadCategories.Grid,
            PreloadCategories.Docking,
            PreloadCategories.Printing,
            PreloadCategories.Dashboard);
    }

    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                // Binda le sezioni di configurazione Strongly-Typed
                services.Configure<AiOptions>(context.Configuration.GetSection(AiOptions.SectionName));
                services.Configure<DevAVFilterOptions>(context.Configuration.GetSection(DevAVFilterOptions.SectionName));

                // Layer Applicativo (CQRS, MediatR, FluentValidation)
                services.AddApplicationServices();

                // Layer Infrastruttura (EF Core SQLite in LocalAppData, Repositories, Localization)
                Directory.CreateDirectory(DatabaseDirectory);
                var connectionString = $"Data Source={DatabaseFilePath}";
                services.AddInfrastructureServices(context.Configuration, connectionString);

                // Layer di Presentazione (IUserStateService, IViewModelFactory, ViewModels, Shell)
                services.AddPresentationServices();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
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
            string? activeBackupPath = CreateDatabaseBackup(logger);

            // 2. Esecuzione migrazioni con ripristino automatico in caso di errore
            try
            {
                await db.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore durante l'applicazione delle migrazioni EF Core. Avvio ripristino di emergenza dal backup.");

                // Chiude la connessione del contesto prima di toccare i file
                await db.Database.CloseConnectionAsync();

                // Ripristina lo stato precedente alla migrazione
                RestoreDatabaseBackup(activeBackupPath, logger);

                ThemedMessageBox.Show(
                    title: "Errore Aggiornamento Database",
                    text: "Si è verificato un errore durante l'aggiornamento dello schema del database.\nI dati sono stati ripristinati alla versione precedente.\n\nContattare l'assistenza tecnica.",
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

    protected override async void OnExit(ExitEventArgs e)
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
    /// Crea un backup preventivo prima delle migrazioni e restituisce il percorso del file generato.
    /// Mantiene automaticamente uno storico rotativo delle ultime 5 copie.
    /// </summary>
    private static string? CreateDatabaseBackup(ILogger logger)
    {
        try
        {
            if (!File.Exists(DatabaseFilePath))
            {
                return null;
            }

            var backupDirectory = Path.Combine(DatabaseDirectory, "Backups");
            Directory.CreateDirectory(backupDirectory);

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
            logger.LogWarning(ex, "Impossibile creare il backup preventivo del database. Il processo prosegue a proprio rischio.");
            return null;
        }
    }

    /// <summary>
    /// Ripristina la copia di backup sul database principale in caso di fallimento della migrazione.
    /// </summary>
    private static void RestoreDatabaseBackup(string? backupFilePath, ILogger logger)
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
            logger.LogInformation("Ripristino di emergenza del database completato con successo da {BackupFilePath}", backupFilePath);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Errore critico durante il ripristino di emergenza del database da {BackupFilePath}", backupFilePath);
        }
    }
}