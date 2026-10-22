// <copyright file="NavigationTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using Common;

using AppModules = Common.Modules;

namespace FatturaFacile.Tests;

/// <summary>
/// Contains unit tests that verify navigation initialization and module
/// activation behavior within the module manager.
/// </summary>
[TestFixture]
public class NavigationTests : BaseTestFixture
{
    /// <summary>
    /// Verifies that the expected modules are injected into their initial
    /// regions and that no document modules are created at startup.
    /// </summary>
    [Test]
    public void InitialModules()
    {
        Assert.IsNotNull(Manager.GetRegion(Regions.MainWindow).GetViewModel(AppModules.Main));
        Assert.IsNotNull(Manager.GetRegion(Regions.Navigation).GetViewModel(AppModules.Module1));
        Assert.IsNotNull(Manager.GetRegion(Regions.Navigation).GetViewModel(AppModules.Module2));
        Assert.IsNull(Manager.GetRegion(Regions.Documents).GetViewModel(AppModules.Module1));
        Assert.IsNull(Manager.GetRegion(Regions.Documents).GetViewModel(AppModules.Module2));
    }

    /// <summary>
    /// Verifies that navigating through the navigation region creates document
    /// modules on demand and keeps the selected navigation item synchronized
    /// with the active document.
    /// </summary>
    [Test]
    public void Navigation()
    {
        Assert.IsNull(Manager.GetRegion(Regions.Documents).GetViewModel(AppModules.Module1));
        Assert.IsNull(Manager.GetRegion(Regions.Documents).GetViewModel(AppModules.Module2));

        Manager.Navigate(Regions.Navigation, AppModules.Module1);
        Assert.IsNotNull(Manager.GetRegion(Regions.Documents).GetViewModel(AppModules.Module1));
        Assert.IsNull(Manager.GetRegion(Regions.Documents).GetViewModel(AppModules.Module2));

        Manager.Navigate(Regions.Navigation, AppModules.Module2);
        Assert.IsNotNull(Manager.GetRegion(Regions.Documents).GetViewModel(AppModules.Module1));
        Assert.IsNotNull(Manager.GetRegion(Regions.Documents).GetViewModel(AppModules.Module2));

        Manager.Navigate(Regions.Documents, AppModules.Module1);
        Assert.AreEqual(AppModules.Module1, Manager.GetRegion(Regions.Navigation).SelectedKey);
    }
}
