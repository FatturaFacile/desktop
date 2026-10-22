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
[Category("Functional")]
[Apartment(ApartmentState.STA)]
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
        Manager.Navigate(Regions.Navigation, global::Common.Modules.Module1);
        DoEvents();

        var document1 = (IDocumentModule)Manager.GetRegion(Regions.Documents)
            .GetViewModel(global::Common.Modules.Module1);

        Assert.IsTrue(document1.IsActive);

        Manager.Navigate(Regions.Navigation, global::Common.Modules.Module2);
        DoEvents();

        var document2 = (IDocumentModule)Manager.GetRegion(Regions.Documents)
            .GetViewModel(global::Common.Modules.Module2);

        Assert.IsFalse(document1.IsActive);
        Assert.IsTrue(document2.IsActive);

        document1.IsActive = true;
        DoEvents();

        Assert.IsTrue(document1.IsActive);
        Assert.IsFalse(document2.IsActive);
        Assert.AreEqual(
            global::Common.Modules.Module1,
            Manager.GetRegion(Regions.Navigation).SelectedKey);
    }
}
