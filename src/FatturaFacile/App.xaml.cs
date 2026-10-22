// <copyright file="App.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core;
using FatturaFacile.Application;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Extensions;
using FatturaFacile.Infrastructure;
using FatturaFacile.Infrastructure;
using FatturaFacile.Infrastructure.Localization;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Services;
using FatturaFacile.ViewModels;
using FatturaFacile.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile;

/// <summary>
/// Represents app.
/// </summary>
public partial class App : System.Windows.Application
{
    private IServiceProvider serviceProvider = null!;
    static IDisposable singleInstanceApplicationGuard;

    static App()
    {
        CompatibilitySettings.UseLightweightThemes = true;
        ApplicationThemeHelper.Preload(PreloadCategories.Core);
        ApplicationThemeHelper.ApplicationThemeName = Theme.Win11Light.Name;

        CompatibilitySettings.UseLightweightThemes = true;
        ApplicationThemeHelper.Preload(PreloadCategories.Core);
        ApplicationThemeHelper.ApplicationThemeName = Theme.Win11Light.Name;


        CompatibilitySettings.UseLightweightThemes = true;
        ApplicationThemeHelper.Preload(PreloadCategories.Core);
        ApplicationThemeHelper.Preload(PreloadCategories.Ribbon);
        ApplicationThemeHelper.ApplicationThemeName = Theme.Win11Light.Name;


        CompatibilitySettings.UseLightweightThemes = true;
        ApplicationThemeHelper.Preload(PreloadCategories.Core);
        ApplicationThemeHelper.ApplicationThemeName = Theme.Win11Light.Name;

        CompatibilitySettings.UseLightweightThemes = true;
        ApplicationThemeHelper.ApplicationThemeName = Theme.Win11Light.Name;
        SplashScreenManager.CreateThemed().ShowOnStartup();
        ApplicationThemeHelper.Preload(PreloadCategories.Core);
        ApplicationThemeHelper.Preload(PreloadCategories.Grid);
        ApplicationThemeHelper.Preload(PreloadCategories.Ribbon);
        ApplicationThemeHelper.Preload(PreloadCategories.Docking);


        // AI provider-specific ChatClient initialization
        var ollamaUri = ConfigurationManager.AppSettings["Ollama.Uri"];
        var ollamaModel = ConfigurationManager.AppSettings["Ollama.Model"];
        if (string.IsNullOrEmpty(ollamaUri) || string.IsNullOrEmpty(ollamaModel))
            throw new InvalidOperationException("Specify the Ollama host address and model name in the 'App.config' file.");

        var chatClient = new OllamaSharp.OllamaApiClient(new Uri(ollamaUri), ollamaModel);

        var container = AIExtensionsContainerDesktop.Default;
        container.RegisterChatClient(chatClient);


        CompatibilitySettings.UseLightweightThemes = true;
        ApplicationThemeHelper.Preload(PreloadCategories.Core);
        ApplicationThemeHelper.ApplicationThemeName = Theme.Win11Light.Name;

        // AI provider-specific ChatClient initialization
        var openAIKey = ConfigurationManager.AppSettings["OpenAI.Key"];
        var openAIModel = ConfigurationManager.AppSettings["OpenAI.Model"];
        if (string.IsNullOrEmpty(openAIKey) || string.IsNullOrEmpty(openAIModel))
            throw new InvalidOperationException("Specify the OpenAI key and model name in the 'App.config' file.");

        var chatClient = new OpenAI.OpenAIClient(openAIKey)
            .GetChatClient(openAIModel)
            .AsIChatClient();

        var container = AIExtensionsContainerDesktop.Default;
        container.RegisterChatClient(chatClient);.

           CompatibilitySettings.UseLightweightThemes = true;
        ApplicationThemeHelper.Preload(PreloadCategories.Core);
        ApplicationThemeHelper.ApplicationThemeName = Theme.Win11Light.Name;

        // AI provider-specific ChatClient initialization
        // Demo AI services are rate limited and intended for demonstration purposes only.
        // DevExpress does not offer a REST API and does not ship any built-in LLMs/SLMs.
        // Use of demo credentials in production is strictly prohibited.

        var azureEndpoint = ConfigurationManager.AppSettings["AzureOpenAI.Endpoint"];
        var azureKey = ConfigurationManager.AppSettings["AzureOpenAI.Key"];
        var azureDeployment = ConfigurationManager.AppSettings["AzureOpenAI.Deployment"];
        if (string.IsNullOrEmpty(azureEndpoint) || string.IsNullOrEmpty(azureKey) || string.IsNullOrEmpty(azureDeployment))
            throw new InvalidOperationException("Specify the Azure OpenAI endpoint, key, and deployment name in the 'App.config' file.");

        var azureOpenAIClient = new Azure.AI.OpenAI.AzureOpenAIClient(new Uri(azureEndpoint), new Azure.AzureKeyCredential(azureKey));
        var chatClient = azureOpenAIClient.GetChatClient(azureDeployment).AsIChatClient();

        var container = AIExtensionsContainerDesktop.Default;
        container.RegisterChatClient(chatClient);

        //Enable Multi Core JIT to improve startup performance
        ProfileOptimization.SetProfileRoot(AppContext.BaseDirectory);
        ProfileOptimization.StartProfile("Startup.Profile");

        CompatibilitySettings.UseLightweightThemes = true;
        ApplicationThemeHelper.ApplicationThemeName = LightweightTheme.Win11System.Name;



        //CompatibilitySettings.UseLightweightThemes = true;
        //ApplicationThemeHelper.ApplicationThemeName = "Win11Light";
        //bool exit;
        //singleInstanceApplicationGuard = DevAVDataDirectoryHelper.SingleInstanceApplicationGuard("DevExpressWpfOutlookInspiredApp", out exit);
        //if (exit)
        //{
        //    SafeEnvironment.Exit(0);
        //}

        //DemoRunner.ShowApplicationSplashScreen();
        //DevExpress.Xpf.Core.ClearAutomationEventsHelper.IsEnabled = false;

        //DemoShellApp.Init(enablePGO: false);

    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        IChatClient? baseChatClient = null;
        IEmbeddingGenerator? embeddingGenerator = null;

        // Demo AI services are rate limited and intended for demonstration purposes only.
        // DevExpress does not offer a REST API and does not ship any built-in LLMs/SLMs.
        // Use of demo credentials in production is strictly prohibited.

        var azureEndpoint = ConfigurationManager.AppSettings["AzureOpenAI.Endpoint"];
        var azureKey = ConfigurationManager.AppSettings["AzureOpenAI.Key"];
        var azureDeployment = ConfigurationManager.AppSettings["AzureOpenAI.Deployment"];
        var azureEmbeddingsDeployment = ConfigurationManager.AppSettings["AzureOpenAI.EmbeddingsDeployment"];

        if (string.IsNullOrEmpty(azureEndpoint) || string.IsNullOrEmpty(azureKey) || string.IsNullOrEmpty(azureDeployment) || string.IsNullOrEmpty(azureEmbeddingsDeployment))
            throw new InvalidOperationException("Configure Azure OpenAI settings in App.config.");

        var azureClient = new Azure.AI.OpenAI.AzureOpenAIClient(new Uri(azureEndpoint), new System.ClientModel.ApiKeyCredential(azureKey));
        baseChatClient = azureClient.GetChatClient(azureDeployment).AsIChatClient();
        embeddingGenerator = azureClient.GetEmbeddingClient(azureEmbeddingsDeployment).AsIEmbeddingGenerator();
        if (baseChatClient == null || embeddingGenerator == null)
            throw new InvalidOperationException("AI provider is not configured.");

        RagEnvironment.InitializeVectorStores(embeddingGenerator);

        var wrappedChatClient = new ChatClientBuilder(baseChatClient)
            .ConfigureOptions(opt =>
            {
                opt.Tools = new[] { AIFunctionFactory.Create(RagEnvironment.SearchAsync) };
                opt.ToolMode = ChatToolMode.RequireAny;
            })
            .UseFunctionInvocation()
            .Build();

        AIExtensionsContainerDesktop.Default.RegisterChatClient(wrappedChatClient);
    }

    private async void OnStartup(
        object sender,
        StartupEventArgs e)
    {

        base.OnStartup(e);

        IServiceProvider services = new ServiceCollection()
            .AddTransient<IDataService, DataService>()
            .AddTransient<MainViewModel>()
            .BuildServiceProvider();

        IocServiceProvider.Default.ConfigureServices(services);

        var services = new ServiceCollection();

        // 1. Connection string for local SQLite DB
        var dbPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "FatturaFacile.db");
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

        serviceProvider = services.BuildServiceProvider();

        // 5. Seed Database & Initialize Localization Cache
        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FatturaFacileDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            await DbInitializer.SeedAsync(
                db,
                hasher);

            var locService = (SqliteLocalizationService)serviceProvider.GetRequiredService<ILocalizationService>();
            await locService.InitializeCacheAsync();

            LocalizationManager.Instance.Initialize(locService);
        }

        // 6. Launch MainWindow
        var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
        var mainVm = serviceProvider.GetRequiredService<MainViewModel>();
        mainWindow.DataContext = mainVm;
        await mainVm.InitializeAsync();

        mainWindow.Show();
    }


    protected override void OnStartup(StartupEventArgs e)
    {
        SplashScreenManager.CreateThemed().ShowOnStartup();

        base.OnStartup(e);

        var host = RegisterServices().Build();
        Resources.Add("services", host.Services);
        IocServiceProvider.Default.ConfigureServices(host.Services);

        ApplicationThemeHelper.PreloadAsync(
            PreloadCategories.Core
            , PreloadCategories.Ribbon
            , PreloadCategories.Grid
            , PreloadCategories.PropertyGrid
            , PreloadCategories.Charts
            , PreloadCategories.Scheduling
            , PreloadCategories.RichEdit
            , PreloadCategories.Spreadsheet
            , PreloadCategories.PdfViewer
            , PreloadCategories.Map
            , PreloadCategories.Gantt
            , PreloadCategories.Diagram
            , PreloadCategories.Printing
            , PreloadCategories.Dashboard
            , PreloadCategories.PivotGrid
            );
    }

    //Register services in the Dependency Injection container
    IHostBuilder RegisterServices()
    {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<TabbedDocShellViewModel>();
                foreach (var item in AppModules.Modules)
                {
                    services.AddTransient(item.ViewModelType);
                    services.AddTransient(item.ViewType);
                }
                services.AddSingleton<ICommonDataService>(new DataService());
                services.AddWpfBlazorWebView();
                services.AddDevExpressBlazor();


                services.AddSingleton<WeatherForecastService>();
            });
        return builder;
    }

#if DEBUG
    public bool IsDebug { get { return true; } }
#endif


    //#if NET



    //    public static AIExtensionsContainerDefault ToolChatContainerDefault { get; set; } = new AIExtensionsContainerDefault();
    //#endif
    //    protected override void OnStartup(StartupEventArgs e)
    //    {
    //        ExceptionHelper.Initialize();
    //        DevAVDataDirectoryHelper.LocalPrefix = "WpfOutlookInspiredApp";
    //#if !NET
    //            ServiceContainer.Default.RegisterService(new ApplicationJumpListService());
    //#endif
    //        AssemblyResolver.Subscribe();
    //        Theme.RegisterPredefinedPaletteThemes();
    //        ImagesAssemblyLoader.Load();
    //        Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata(200));
    //        base.OnStartup(e);
    //        ViewLocator.Default = new ViewLocator(typeof(App).Assembly);
    //        Theme.TouchlineDark.ShowInThemeSelector = false;
    //        SetCultureInfo();
    //#if NET
    //        RegisterChatClient();
    //#endif
    //    }
    //    static void SetCultureInfo()
    //    {
    //        CultureInfo demoCI = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
    //        demoCI.NumberFormat.CurrencySymbol = "$";
    //        demoCI.DateTimeFormat = new DateTimeFormatInfo();
    //        Thread.CurrentThread.CurrentCulture = demoCI;
    //        CultureInfo demoUI = (CultureInfo)Thread.CurrentThread.CurrentUICulture.Clone();
    //        demoUI.NumberFormat.CurrencySymbol = "$";
    //        demoUI.DateTimeFormat = new DateTimeFormatInfo();
    //        Thread.CurrentThread.CurrentUICulture = demoUI;
    //    }

    //#if NET
    //    static void RegisterChatClient()
    //    {
    //        AzureOpenAIClient azureOpenAIClient = new AzureOpenAIClient(AzureOpenAIEndpoint, AzureOpenAIKey, new AzureOpenAIClientOptions()
    //        {
    //            Transport = new PromoteHttpStatusErrorsPipelineTransport()
    //        });
    //        IChatClient chatClient = azureOpenAIClient.GetChatClient("demo").AsIChatClient();
    //        var container = AIExtensionsContainerDesktop.Default;

    //        IChatClient toolChatClient = new ChatClientBuilder(chatClient)
    //            .UseDXTools()
    //            .UseFunctionInvocation(configure: x => x.IncludeDetailedErrors = true)
    //            .Build(ToolChatContainerDefault);

    //        ToolChatContainerDefault.RegisterChatClient(toolChatClient);
    //        ToolChatContainerDefault.RegisterAIExceptionHandler(new AIExceptionHandler());

    //        AIExtensionsContainerDesktop.QueryIAIExtensionsContainer += (s, e) => {
    //            if (s is AIChatControl)
    //                e.Container = ToolChatContainerDefault;
    //        };

    //        container.RegisterChatClient(chatClient);
    //        container.RegisterAIExceptionHandler(new AIExceptionHandler());
    //    }

    //    static Uri AzureOpenAIEndpoint
    //    {
    //        get
    //        {
    //            var azureOpenAIEndpoint = GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
    //            if (string.IsNullOrEmpty(azureOpenAIEndpoint))
    //                azureOpenAIEndpoint = "https://api.devexpress.com/demo-openai";
    //            return new Uri(azureOpenAIEndpoint);
    //        }
    //    }

    //    static System.ClientModel.ApiKeyCredential AzureOpenAIKey
    //    {
    //        get
    //        {
    //            var azureOpenAIKey = GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
    //            if (string.IsNullOrEmpty(azureOpenAIKey))
    //                azureOpenAIKey = "DEMO";
    //            return new System.ClientModel.ApiKeyCredential(azureOpenAIKey);
    //        }
    //    }

    //    static string GetEnvironmentVariable(string variableName)
    //    {
    //        return SafeEnvironment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.User);
    //    }

    //#endif


    //    #region DemoTester
    //    public static void Test(ThemedWindow window)
    //    {
    //        string testerPath = null;
    //        bool allowDemoTest = false;
    //        var frameworkPrefix =
    //#if NET
    //            "NETCoreDesktop";
    //#else
    //                "Framework4";
    //#endif
    //#pragma warning disable DX0025
    //        testerPath = string.Format(@"c:\DevExpress.Tester\{0}\DevExpress.Reallife.Tester.v{1}.dll", frameworkPrefix, AssemblyInfo.VersionShort);
    //#pragma warning restore DX0025
    //        var args = SafeEnvironment.GetCommandLineArgs();
    //        allowDemoTest = args.Length > 0 && args.Contains(@"/testallexcept:");

    //        if (File.Exists(testerPath) && allowDemoTest)
    //        {
    //            ExceptionHelper.IsEnabled = false;
    //            var assembly = SafeTypeResolver.GetOrLoadAssemblyFrom(testerPath);
    //            var type = SafeTypeResolver.GetKnownType(assembly, "DevExpress.Reallife.Tester.OutlookInspiredAppDemoTester");
    //            type.GetMethod("Create")?.Invoke(null, new object[] { window, true });
    //        }
    //    }

    //    #endregion

    //    public static string ApplicationID
    //    {
    //        get { return string.Format("Components_{0}_Demo_Center_OutlookInspired_{0}", AssemblyInfo.VersionShort.Replace(".", "_")); }
    //    }
    //}

    //[Guid("86882B9F-1EAE-41D9-B9CF-BD7ACCA51523"), ComVisible(true)]
    //public class OutlookInspiredAppNotificationActivator : ToastNotificationActivator
    //{
    //    public override void OnActivate(string arguments, System.Collections.Generic.Dictionary<string, string> data) { }
    //}
    //}

    //#if NET

    //class PromoteHttpStatusErrorsPipelineTransport : HttpClientPipelineTransport
    //{
    //    protected override void OnReceivedResponse(PipelineMessage message, HttpResponseMessage httpResponse)
    //    {
    //        if (!httpResponse.IsSuccessStatusCode)
    //        {
    //            if ((int)httpResponse.StatusCode == 429)
    //                throw new AIDemoException("You have reached demo request limit. Further requests are temporarily suspended. Please try again in a few minutes. Thank you for your patience and understanding.");
    //            throw new HttpRequestException("HTTP request failed with status code: " + httpResponse.StatusCode);
    //        }

    //        base.OnReceivedResponse(message, httpResponse);
    //    }
    //}
    //#endif

    //namespace DevExpress.Internal.DemoLauncher
    //{
    //    public static class DemoLauncherEntryPoint
    //    {
    //        public static Tuple<Action, System.Threading.Tasks.Task<Window>> Run()
    //        {
    //            var done = new System.Threading.Tasks.TaskCompletionSource<Window>();
    //            Action run = () => {
    //                var app = new DevExpress.DevAV.App();
    //                app.InitializeComponent();
    //                typeof(Application).GetField("_startupUri", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(app, null);
    //                app.Startup += (s, e) => {
    //                    var window = new DevExpress.DevAV.MainWindow();
    //                    window.Show();
    //                    app.MainWindow = window;
    //                    done.SetResult(window);
    //                };
    //                app.Run();
    //            };
    //            return Tuple.Create(run, done.Task);
    //        }
    //    }
    //}

 protected override void OnExit(ExitEventArgs e)
    {
        ApplicationThemeHelper.SaveApplicationThemeName();
        base.OnExit(e);
    }
    void OnApplicationStartup(object sender, StartupEventArgs e)
    {
        Bootstrapper.Run();
    }
}
public class Bootstrapper
{
    public static Bootstrapper? Default { get; protected set; }
    public static void Run()
    {
        Default = new Bootstrapper();
        Default.RunCore();
    }
    protected Bootstrapper() { }

    const string StateVersion = "1.0";
    public virtual void RunCore()
    {
        ConfigureTypeLocators();
        RegisterModules();
        if (!RestoreState())
            InjectModules();
        ConfigureNavigation();
        ShowMainWindow();
    }

    protected IModuleManager Manager { get { return ModuleManager.DefaultManager; } }
    protected virtual void ConfigureTypeLocators()
    {
        var mainAssembly = typeof(MainViewModel).Assembly;
        var modulesAssembly = typeof(ModuleViewModel).Assembly;
        var assemblies = new[] { mainAssembly, modulesAssembly };
        ViewModelLocator.Default = new ViewModelLocator(assemblies);
        ViewLocator.Default = new ViewLocator(assemblies);
    }
    protected virtual void RegisterModules()
    {
        Manager.GetRegion(Regions.Documents).VisualSerializationMode = VisualSerializationMode.PerKey;
        Manager.Register(Regions.MainWindow, new Module(AppModules.Main, MainViewModel.Create, typeof(MainView)));
        Manager.Register(Regions.Navigation, new Module(AppModules.Module1, () => new NavigationItem("Module1")));
        Manager.Register(Regions.Navigation, new Module(AppModules.Module2, () => new NavigationItem("Module2")));
        Manager.Register(Regions.Documents, new Module(AppModules.Module1, () => ModuleViewModel.Create("Module1"), typeof(ModuleView)));
        Manager.Register(Regions.Documents, new Module(AppModules.Module2, () => ModuleViewModel.Create("Module2"), typeof(ModuleView)));
    }
    protected virtual bool RestoreState()
    {
        if (Settings.Default.StateVersion != StateVersion)
            return false;
        return Manager.Restore(Settings.Default.LogicalState, Settings.Default.VisualState);
    }
    protected virtual void InjectModules()
    {
        Manager.Inject(Regions.MainWindow, AppModules.Main);
        Manager.Inject(Regions.Navigation, AppModules.Module1);
        Manager.Inject(Regions.Navigation, AppModules.Module2);
    }
    protected virtual void ConfigureNavigation()
    {
        Manager.GetEvents(Regions.Navigation).Navigation += OnNavigation;
        Manager.GetEvents(Regions.Documents).Navigation += OnDocumentsNavigation;
    }
    protected virtual void ShowMainWindow()
    {
        App.Current.MainWindow = new MainWindow();
        App.Current.MainWindow.Show();
        App.Current.MainWindow.Closing += OnClosing;
    }
    void OnNavigation(object sender, NavigationEventArgs e)
    {
        if (e.NewViewModelKey == null)
            return;
        Manager.InjectOrNavigate(Regions.Documents, e.NewViewModelKey);
    }
    void OnDocumentsNavigation(object sender, NavigationEventArgs e)
    {
        Manager.Navigate(Regions.Navigation, e.NewViewModelKey);
    }
    void OnClosing(object sender, CancelEventArgs e)
    {
        string logicalState;
        string visualState;
        Manager.Save(out logicalState, out visualState);
        Settings.Default.StateVersion = StateVersion;
        Settings.Default.LogicalState = logicalState;
        Settings.Default.VisualState = visualState;
        Settings.Default.Save();
    }
}
