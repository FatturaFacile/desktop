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
        Assert.IsNotNull(anObject: Manager.GetRegion(regionName: Regions.MainWindow).GetViewModel(key: AppModules.Main));
        Assert.IsNotNull(anObject: Manager.GetRegion(regionName: Regions.Navigation).GetViewModel(key: AppModules.Module1));
        Assert.IsNotNull(anObject: Manager.GetRegion(regionName: Regions.Navigation).GetViewModel(key: AppModules.Module2));
        Assert.IsNull(anObject: Manager.GetRegion(regionName: Regions.Documents).GetViewModel(key: AppModules.Module1));
        Assert.IsNull(anObject: Manager.GetRegion(regionName: Regions.Documents).GetViewModel(key: AppModules.Module2));
    }

    /// <summary>
    /// Verifies that navigating through the navigation region creates document
    /// modules on demand and keeps the selected navigation item synchronized
    /// with the active document.
    /// </summary>
    [Test]
    public void Navigation()
    {
        Assert.IsNull(anObject: Manager.GetRegion(regionName: Regions.Documents).GetViewModel(key: AppModules.Module1));
        Assert.IsNull(anObject: Manager.GetRegion(regionName: Regions.Documents).GetViewModel(key: AppModules.Module2));

        Manager.Navigate(regionName: Regions.Navigation, key: AppModules.Module1);
        Assert.IsNotNull(anObject: Manager.GetRegion(regionName: Regions.Documents).GetViewModel(key: AppModules.Module1));
        Assert.IsNull(anObject: Manager.GetRegion(regionName: Regions.Documents).GetViewModel(key: AppModules.Module2));

        Manager.Navigate(regionName: Regions.Navigation, key: AppModules.Module2);
        Assert.IsNotNull(anObject: Manager.GetRegion(regionName: Regions.Documents).GetViewModel(key: AppModules.Module1));
        Assert.IsNotNull(anObject: Manager.GetRegion(regionName: Regions.Documents).GetViewModel(key: AppModules.Module2));

        Manager.Navigate(regionName: Regions.Documents, key: AppModules.Module1);
        Assert.AreEqual(expected: AppModules.Module1, actual: Manager.GetRegion(regionName: Regions.Navigation).SelectedKey);
    }

}
