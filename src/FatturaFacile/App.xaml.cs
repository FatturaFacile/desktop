// <copyright file="App.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using System.Runtime;
using System.Windows;

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Extensions;
using FatturaFacile.Application.Options;
using FatturaFacile.Extensions;
using FatturaFacile.Infrastructure;
using FatturaFacile.Infrastructure.Localization;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.ViewModels;
using FatturaFacile.Wpf.Common.Services;
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

    /// <summary>
    /// Gets the absolute directory path where local application database files and backups are stored.
    /// </summary>
    private static readonly string DatabaseDirectory = Path.Combine(
                                                                    path1: Environment.GetFolderPath(
                                                                                                     folder: Environment.SpecialFolder
                                                                                                                        .LocalApplicationData),
                                                                    path2: "FatturaFacile");

    /// <summary>
    /// Gets the absolute file path to the local SQLite database file.
    /// </summary>
    private static readonly string DatabaseFilePath = Path.Combine(path1: DatabaseDirectory, path2: "FatturaFacile.db");

    /// <summary>
    /// Gets the shared generic <see cref="IHost"/> instance managing the dependency injection container,
    /// application configuration, and hosted background services for the entire desktop lifecycle.
    /// </summary>
    public static IHost AppHost { get; private set; } = null!;

    /// <summary>
    /// Initializes static members of the <see cref="App"/> class.
    /// Configures cold-start JIT optimizations via profile-guided optimization and preloads lightweight themes.
    /// </summary>
    static App()
    {
        // 1. JIT optimization to accelerate cold-start
        ProfileOptimization.SetProfileRoot(directoryPath: AppContext.BaseDirectory);
        ProfileOptimization.StartProfile(profile: "Startup.Profile");

        // 2. DevExpress Lightweight Theme Configuration
        CompatibilitySettings.UseLightweightThemes  = true;
        ApplicationThemeHelper.ApplicationThemeName = LightweightTheme.Win11System.Name;

        // 3. Asynchronous preloading of DevExpress control categories used in the application
        _ = ApplicationThemeHelper.PreloadAsync(
                                                preloadCategories: [PreloadCategories.Core, PreloadCategories.Ribbon, PreloadCategories.Grid, PreloadCategories.Docking, PreloadCategories.Printing, PreloadCategories.Dashboard]);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// Builds the Microsoft.Extensions host configuring configuration providers, application layers,
    /// persistence storage in LocalAppData, and presentation services.
    /// </summary>
    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
                      .ConfigureAppConfiguration(configureDelegate: (context, config) =>
                                                                    {
                                                                        _ = config.SetBasePath(basePath: AppContext.BaseDirectory);

                                                                        _ = config.AddJsonFile(
                                                                                               path: "appsettings.json",
                                                                                               optional: false,
                                                                                               reloadOnChange: true);

                                                                        _ = config.AddEnvironmentVariables();
                                                                    })
                      .ConfigureServices(configureDelegate: (context, services) =>
                                                            {
                                                                // Bind strongly typed configuration sections
                                                                _ = services.Configure<AiOptions>(
                                                                                                  config: context.Configuration.GetSection(
                                                                                                                                           key: AiOptions
                                                                                                                                              .SectionName));

                                                                _ = services.Configure<DevAVFilterOptions>(
                                                                                                           config: context.Configuration
                                                                                                                          .GetSection(
                                                                                                                                      key: DevAVFilterOptions
                                                                                                                                         .SectionName));

                                                                // Application Layer (CQRS, MediatR, FluentValidation)
                                                                _ = services.AddApplicationServices();

                                                                // Infrastructure Layer (EF Core SQLite in LocalAppData, Repositories, Localization)
                                                                _ = Directory.CreateDirectory(path: DatabaseDirectory);
                                                                var connectionString = $"Data Source={DatabaseFilePath}";

                                                                _ = services.AddInfrastructureServices(
                                                                                                       configuration: context.Configuration,
                                                                                                       connectionString: connectionString);

                                                                // Presentation Layer (IUserStateService, IViewModelFactory, ViewModels, Shell)
                                                                _ = services.AddPresentationServices(configuration: context.Configuration);
                                                            })
                      .Build();
    }

    /// <summary>
    /// Handles the application startup sequence, displaying the themed splash screen, bootstrapping
    /// the generic host, running pre-migration SQLite database backups, executing pending migrations,
    /// seeding default tenant/user data, warming localization cache, and presenting the main application window.
    /// </summary>
    /// <param name="e">The <see cref="StartupEventArgs"/> containing event arguments.</param>
    protected override async void OnStartup(StartupEventArgs e)
    {
        // Display native themed DevExpress splash screen
        SplashScreenManager.CreateThemed().ShowOnStartup();

        base.OnStartup(e: e);

        // Start .NET generic host
        await AppHost.StartAsync();

        // Database initialization, preventive backup, migrations, and cache
        using (var scope = AppHost.Services.CreateScope())
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<App>>();
            var db     = scope.ServiceProvider.GetRequiredService<FatturaFacileDbContext>();

            // 1. Preventive safety backup of SQLite database
            var activeBackupPath = CreateDatabaseBackup(logger: logger);

            // 2. Execute migrations with automatic rollback on error
            try
            {
                await db.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(
                                exception: ex,
                                message: "Error applying EF Core migrations. Starting emergency restore from backup.");

                // Close context connection before accessing files
                await db.Database.CloseConnectionAsync();

                // Restore previous pre-migration state
                RestoreDatabaseBackup(backupFilePath: activeBackupPath, logger: logger);

                _ = ThemedMessageBox.Show(
                                          title: "Database Update Error",
                                          text:
                                          "An error occurred while updating the database schema.\n" +
                                          "Data has been restored to the previous version.\n\n" +
                                          "Please contact technical support.",
                                          messageBoxButtons: MessageBoxButton.OK,
                                          icon: MessageBoxImage.Error);

                Shutdown(exitCode: -1);

                return;
            }

            // 3. Execute initial seeding (create tenants, users, and base data)
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            await DbInitializer.SeedAsync(context: db, passwordHasher: hasher);

            // 4. Initialize local translation cache
            if (scope.ServiceProvider.GetService<ILocalizationService>() is SqliteLocalizationService locService)
            {
                await locService.InitializeCacheAsync();
                LocalizationManager.Instance.Initialize(localizationService: locService);
            }
        }

        // Resolve and display Shell MainWindow
        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        var mainVm     = AppHost.Services.GetRequiredService<MainViewModel>();
        mainWindow.DataContext = mainVm;

        await mainVm.InitializeAsync();

        mainWindow.Show();
    }

    /// <summary>
    /// Handles the application shutdown sequence, persisting current application theme preferences,
    /// stopping the hosted services asynchronously, and disposing the host container.
    /// </summary>
    /// <param name="e">The <see cref="ExitEventArgs"/> containing exit state information.</param>
    protected override async void OnExit(ExitEventArgs e)
    {
        ApplicationThemeHelper.SaveApplicationThemeName();

        if (AppHost != null)
        {
            await AppHost.StopAsync();
            AppHost.Dispose();
        }

        base.OnExit(e: e);
    }

    /// <summary>
    /// Creates a pre-migration timestamped safety backup of the SQLite database and retains the last five snapshots.
    /// </summary>
    /// <param name="logger">The application logger for recording audit and diagnostic messages.</param>
    /// <returns>The fully-qualified path to the created backup archive, or <see langword="null"/> if the source database did not exist or backup failed.</returns>
    private static string? CreateDatabaseBackup(ILogger logger)
    {
        try
        {
            if (!File.Exists(path: DatabaseFilePath))
            {
                return null;
            }

            var backupDirectory = Path.Combine(path1: DatabaseDirectory, path2: "Backups");
            _ = Directory.CreateDirectory(path: backupDirectory);

            var timestamp      = DateTime.Now.ToString(format: "yyyyMMdd_HHmmss");
            var backupFilePath = Path.Combine(path1: backupDirectory, path2: $"FatturaFacile_PreMigration_{timestamp}.db");

            File.Copy(sourceFileName: DatabaseFilePath, destFileName: backupFilePath, overwrite: true);
            logger.LogInformation(message: "Pre-migration database backup created at: {BackupFilePath}", args: backupFilePath);

            // Retain only the 5 most recent backups
            var backupFiles = new DirectoryInfo(path: backupDirectory)
                             .GetFiles(searchPattern: "FatturaFacile_PreMigration_*.db")
                             .OrderByDescending(keySelector: f => f.CreationTimeUtc)
                             .Skip(count: 5);

            foreach (var oldBackup in backupFiles)
            {
                oldBackup.Delete();
            }

            return backupFilePath;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                              exception: ex,
                              message: "Unable to create pre-migration database backup. Process continuing at own risk.");

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
        if (string.IsNullOrWhiteSpace(value: backupFilePath) || !File.Exists(path: backupFilePath))
        {
            logger.LogWarning(message: "No valid backup file available for automatic restoration.");

            return;
        }

        try
        {
            // 1. Clear all SQLite connection pools on the file to prevent file locking conflicts
            SqliteConnection.ClearAllPools();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // 2. Overwrite file with intact backup copy
            File.Copy(sourceFileName: backupFilePath, destFileName: DatabaseFilePath, overwrite: true);

            logger.LogInformation(
                                  message: "Emergency database restore successfully completed from {BackupFilePath}",
                                  args: backupFilePath);
        }
        catch (Exception ex)
        {
            logger.LogCritical(
                               exception: ex,
                               message: "Critical error during emergency database restore from {BackupFilePath}",
                               args: backupFilePath);
        }
    }

}
