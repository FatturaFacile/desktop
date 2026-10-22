// <copyright file="Bootstrapper.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using Common;
using DevExpress.Mvvm.ModuleInjection;

namespace FatturaFacile;

/// <summary>
/// Legacy bootstrapper for module injection testing and backward compatibility.
/// </summary>
public class Bootstrapper
{
    public static Bootstrapper? Default { get; protected set; }

    public static void Run()
    {
        Default = new Bootstrapper();
        Default.RunCore();
    }

    protected Bootstrapper()
    {
    }

    protected static IModuleManager Manager => ModuleManager.DefaultManager;

    public virtual void RunCore()
    {
        ConfigureTypeLocators();
        RegisterModules();
        if (!RestoreState())
        {
            InjectModules();
        }

        ConfigureNavigation();
        ShowMainWindow();
    }

    protected virtual void ConfigureTypeLocators()
    {
    }

    protected virtual void RegisterModules() => Manager.GetRegion(Common.Regions.Documents).VisualSerializationMode = VisualSerializationMode.PerKey;

    protected virtual bool RestoreState() => false;

    protected virtual void InjectModules()
    {
    }

    protected virtual void ConfigureNavigation()
    {
        Manager.GetEvents(Common.Regions.Navigation).Navigation += OnNavigation;
        Manager.GetEvents(Common.Regions.Documents).Navigation += OnDocumentsNavigation;
    }

    protected virtual void ShowMainWindow()
    {
    }

    private static void OnNavigation(object? sender, NavigationEventArgs e)
    {
        if (e.NewViewModelKey == null)
        {
            return;
        }

        Manager.InjectOrNavigate(Common.Regions.Documents, e.NewViewModelKey);
        SyncDocumentActiveState(e.NewViewModelKey);
    }

    private static void OnDocumentsNavigation(object? sender, NavigationEventArgs e)
    {
        if (e.NewViewModelKey == null)
        {
            return;
        }

        Manager.Navigate(Common.Regions.Navigation, e.NewViewModelKey);
        SyncDocumentActiveState(e.NewViewModelKey);
    }

    private static void SyncDocumentActiveState(string activeKey)
    {
        var docRegion = Manager.GetRegion(Common.Regions.Documents);
        if (docRegion == null)
        {
            return;
        }

        foreach (var vm in docRegion.ViewModels)
        {
            if (vm is IDocumentModule doc)
            {
                doc.IsActive = (docRegion.GetKey(vm) == activeKey);
            }
        }
    }
}
