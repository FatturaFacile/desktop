// <copyright file="FunctionalNavigationTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using Common;

namespace FatturaFacile.Tests;

/// <summary>
/// Contains functional tests that verify navigation and document activation
/// behavior across navigation and document regions.
/// </summary>
[TestFixture]
[Category(name: "Functional")]
[Apartment(apartmentState: ApartmentState.STA)]
public class FunctionalNavigationTests : BaseTestFixture
{

    /// <summary>
    /// Gets a value indicating whether functional testing mode is enabled.
    /// </summary>
    protected override bool IsFunctionalTesting => true;

    /// <summary>
    /// Verifies that navigating between modules correctly updates document
    /// activation state and synchronizes the selected navigation item with
    /// the active document.
    /// </summary>
    [Test]
    public void Navigation()
    {
        Manager.Navigate(regionName: Regions.Navigation, key: global::Common.Modules.Module1);
        DoEvents();

        var document1 = (IDocumentModule)Manager.GetRegion(regionName: Regions.Documents)
                                                .GetViewModel(key: global::Common.Modules.Module1);

        Assert.IsTrue(condition: document1.IsActive);

        Manager.Navigate(regionName: Regions.Navigation, key: global::Common.Modules.Module2);
        DoEvents();

        var document2 = (IDocumentModule)Manager.GetRegion(regionName: Regions.Documents)
                                                .GetViewModel(key: global::Common.Modules.Module2);

        Assert.IsFalse(condition: document1.IsActive);
        Assert.IsTrue(condition: document2.IsActive);

        document1.IsActive = true;
        DoEvents();

        Assert.IsTrue(condition: document1.IsActive);
        Assert.IsFalse(condition: document2.IsActive);

        Assert.AreEqual(
                        expected: global::Common.Modules.Module1,
                        actual: Manager.GetRegion(regionName: Regions.Navigation).SelectedKey);
    }

}
