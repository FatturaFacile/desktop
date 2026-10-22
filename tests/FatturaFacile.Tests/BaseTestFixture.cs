// <copyright file="BaseTestFixture.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using System.Windows;
using Common;
using DevExpress.Mvvm;
using DevExpress.Mvvm.ModuleInjection;
using DevExpress.Xpf.Core;
using FatturaFacile;
using NUnit.Framework;
using AppModules = Common.Modules;
using WpfApplication = System.Windows.Application;
using WpfUserControl = System.Windows.Controls.UserControl;

namespace Tests;

public abstract class BaseTestFixture
{
    public IModuleManager Manager => ModuleManager.DefaultManager;
    protected virtual bool IsFunctionalTesting => false;

    [SetUp]
    public void SetUp()
    {
        EnsureApplicationResources();
        ModuleManager.DefaultManager = new ModuleManager(false, true);
        TestBootstrapper.TestRun(IsFunctionalTesting);
    }

    [TearDown]
    public void TearDown() => ModuleManager.DefaultManager = null!;

    protected void DoEvents()
    {
        DispatcherHelper.DoEvents();
    }

    private static void EnsureApplicationResources()
    {
        if (WpfApplication.Current == null)
        {
            _ = new WpfApplication();
        }

        if (WpfApplication.Current != null && WpfApplication.Current.Resources.MergedDictionaries.Count == 0)
        {
            WpfApplication.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/FatturaFacile.Wpf.Common;component/Themes/DarkTheme.xaml", UriKind.Absolute)
            });
            WpfApplication.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/FatturaFacile.Wpf.Common;component/Themes/Icons.xaml", UriKind.Absolute)
            });
            WpfApplication.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/FatturaFacile.Wpf.Common;component/Themes/Styles.xaml", UriKind.Absolute)
            });

            WpfApplication.Current.Resources["BooleanToVisibilityConverter"] = new System.Windows.Controls.BooleanToVisibilityConverter();
            WpfApplication.Current.Resources["InverseBooleanConverter"] = new FatturaFacile.Markup.InverseBooleanConverter();
            WpfApplication.Current.Resources["StringToVisibilityConverter"] = new FatturaFacile.Markup.StringToVisibilityConverter();
        }
    }
}

public class TestDocumentModule : BindableBase, IDocumentModule
{
    private readonly string key;
    private bool isActive;

    public TestDocumentModule(string key, string caption)
    {
        this.key = key;
        Caption = caption;
    }

    public string Caption { get; }

    public bool IsActive
    {
        get => isActive;
        set
        {
            if (SetProperty(ref isActive, value, nameof(IsActive)))
            {
                if (value)
                {
                    ModuleManager.DefaultManager?.Navigate(Common.Regions.Documents, key);
                }
            }
        }
    }
}

public class TestBootstrapper : Bootstrapper
{
    public static new TestBootstrapper? Default
    {
        get => Bootstrapper.Default as TestBootstrapper;
        protected set => Bootstrapper.Default = value;
    }

    public static void TestRun(bool showRealWindow)
    {
        Default = new TestBootstrapper(showRealWindow);
        Default.RunCore();
    }

    private readonly bool showRealWindow;
    public Window? MainWindow { get; private set; }

    public TestBootstrapper(bool showRealWindow)
    {
        this.showRealWindow = showRealWindow;
    }

    protected override void RegisterModules()
    {
        base.RegisterModules();
        Manager.Register(Regions.MainWindow, new Module(AppModules.Main, () => new object(), typeof(WpfUserControl)));
        Manager.Register(Regions.Navigation, new Module(AppModules.Module1, () => new object(), typeof(WpfUserControl)));
        Manager.Register(Regions.Navigation, new Module(AppModules.Module2, () => new object(), typeof(WpfUserControl)));
        Manager.Register(Regions.Documents, new Module(AppModules.Module1, () => new TestDocumentModule(AppModules.Module1, "Module 1"), typeof(WpfUserControl)));
        Manager.Register(Regions.Documents, new Module(AppModules.Module2, () => new TestDocumentModule(AppModules.Module2, "Module 2"), typeof(WpfUserControl)));
    }

    protected override void InjectModules()
    {
        base.InjectModules();
        Manager.Inject(Regions.MainWindow, AppModules.Main);
        Manager.Inject(Regions.Navigation, AppModules.Module1);
        Manager.Inject(Regions.Navigation, AppModules.Module2);
    }

    protected override void ShowMainWindow()
    {
    }
}
